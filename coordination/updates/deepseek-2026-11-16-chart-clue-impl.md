# 交接：失落航图 · 遗迹线索步骤实现

- 交接人：DeepSeek（小肥 / DSH）
- 日期：2026-11-16
- 分支：`agent/deepseek/chart-clue-impl`（worktree `F:\github\agent-deepseek-chart-impl`）
- 起点：`main` @ `4a9a460`
- 依据：`AGENTS.md`、`coordination/STATUS.md`、`coordination/updates/deepseek-2026-11-16-chart-clue.md`（方案 A）
- 边界遵守：**未改主 worktree、未启动 Unity 构建**。改动仅限本任务所需脚本。**未跑任何 Unity 验证**——以下全部等待集成者执行。

---

## 1. 做了什么

按方案 A 实现：航图在**读完遗迹残碑之前不可拾取**，读后世界目标、罗盘指引、HUD、任务日志同步切换；线索独立存档，旧档兼容。

### 存档契约（关键决策）

`SurveyChartStage` 枚举**一个值都没动**（`Locked=0 / SearchRuin=1 / ReturnToHarbour=2 / Complete=3`），
线索走**独立键** `foglightisles.quest.survey-chart.clue.v1`（int，1=已读）。

旧档没有这个键 → 读作「未读」。这意味着旧档停在 `SearchRuin` 的玩家会看到一次新步骤，但**进度不会回退**；停在 `ReturnToHarbour` / `Complete` 的旧档完全不受影响（拦截只在 `SearchRuin` 分支生效）。

---

## 2. 变更文件

| 文件 | 改动 |
| --- | --- |
| `Assets/Scripts/ProgressStore.cs` | 新增 `SurveyClueReadKey`（`internal`，供验证器构造旧档）、`HasReadSurveyClue`、`SaveSurveyClueRead`；`ClearPrototypeProgress()` 增加删除该键；`IsValidationRun` 注册 `-verify-chart-old-save` |
| `Assets/Scripts/SurveyChartQuest.cs` | 新增 `ClueRead`、`RuinClueObjective`、`ReadRuinClue()`；`CollectChart()` 增加未读拦截（阶段不变、不隐藏物件）；`ReadRuinClue()` 读后触发 `Changed` 以刷新引导 |
| `Assets/Scripts/GameBootstrap.cs` | 新增 `_surveyClueTablet` 字段与 `BuildSurveyChartClue()`（残碑位于 `(6.6, 0.9, -84.6)`，交互标题「遗迹残碑」/提示「辨认航图残记」）；`RefreshWorldForQuest` 引导分支：未读指向残碑，已读指向航图；挂载 `PrototypeChartOldSaveValidator` |
| `Assets/Scripts/PrototypeHUD.cs` | 目标文案：`SearchRuin` 且未读时显示「辨认遗迹残碑上的旧航线记录。」 |
| `Assets/Scripts/QuestJournalPanel.cs` | 任务日志明细：未读时提示先辨认残碑再取图 |
| `Assets/Scripts/PrototypeKeyboardChartValidator.cs` | 拾取改为三步：先断言**未读时 E 无效且航图仍在**（截图 `review-keyboard-chart-blocked.png`）→ 走到残碑读线索 → 再拾取并断言阶段推进 |
| `Assets/Scripts/PrototypeNewSaveValidator.cs` | 造档时写入线索；清档后断言 `!HasReadSurveyClue`；PASS/FAIL 文案补线索 |
| `Assets/Scripts/PrototypeChartOldSaveValidator.cs`（新增） | `-verify-chart-old-save`：断言枚举整数值未变、缺键读作未读、旧 `SearchRuin` 档阶段仍正确、`ReturnToHarbour`/`Complete` 不回退；结束时恢复验证作用域 |

共 **7 改 + 1 新增**（含 `.meta`，GUID `a7765224e0b047929f2d6e23fc269b93`）。

---

## 3. 建议的验证顺序

```powershell
# 1) 旧档兼容（最先跑，最便宜）
Unity.exe -batchmode -quit -projectPath <雾灯群岛> -executeMethod ... -verify-chart-old-save
# 2) 主线图表回归（含新拦截断言）
-verify-keyboard-chart-quest
# 3) 续档
-verify-keyboard-chart-resume
# 4) 新档清档
-verify-new-save
# 5) 其余回归不得回退
-verify-quest-loop / -verify-keyboard-first-quest / -verify-keyboard-storm-quest / -verify-gamepad-voyage
```

`-verify-keyboard-chart-quest` 现在会多走两段（残碑往返），**单次耗时略增**，仍应在 540 帧的 `Walk` 上限内。

---

## 4. 未验证点（请重点复核）

1. **残碑位置与碰撞未实机验证。** 坐标 `(6.6, 0.9, -84.6)` 是按航图 `(8.3, 1.0, -83.8)` 推算的，两物件相距 **1.88 m**。交互半径是 **2.25 m** 且取最近者（`PlayerController.cs:229`）：
   - 站位 `(8.3, -82.3)`：距航图 1.50 m / 距残碑 2.86 m → 命中航图 ✓
   - 站位 `(6.6, -83.6)`：距残碑 1.00 m / 距航图 1.71 m → 命中残碑 ✓
   
   数学上成立，但**残碑是否会挡住航图的碰撞体**、两物件是否互相插入、玩家站位是否会卡住，均未在引擎内确认。

2. **`Walk` 路径可达性未验证。** 从航图前到残碑前需要横向绕行 1.88 m；`Walk` 是直线朝目标推进，若中间有石台碰撞体会卡住。若实测卡住，把残碑再往东/北挪 0.5–1.0 m 更安全（**建议集成者实测后决定，我不擅自再调**）。

3. **`ReadRuinClue()` 里 `Changed?.Invoke` 的连锁刷新未验证。** 它会触发 `RefreshWorldForQuest` → `RefreshHarbourProsperity`；阶段未变所以不会误点亮引航灯，但 HUD 提示是否被后续刷新覆盖未在实机确认。

4. **旧档默认「未读」是产品取舍。** 若集成者认为旧档不能出现新增阻碍，把 `HasReadSurveyClue` 改成「`Stage >= SearchRuin` 时缺键视为已读」即可，一行改动，回滚成本极低。

5. **静态检查只覆盖括号配平与符号存在性**（自写脚本 `_check.py`，已删除），**不等于编译通过**。C# 编译与所有运行时行为均未验证。

6. **`PrototypeNewSaveValidator` 的线索断言**依赖 `ClearPrototypeProgress()` 删掉新键——已改，但同样未经运行时确认。

---

## 5. 需要谁接手

| 事项 | 接手方 |
| --- | --- |
| Unity 编译、构建、跑 §3 全部验证 | Codex（集成者） |
| 残碑坐标/碰撞实测后微调（如需） | Codex |
| §4.4 旧档默认行为的最终取舍 | Codex 决定，需要改我随时配合 |

---

**提交后请以 `coordination/STATUS.md` 记录的验收结果为准；本文档的「应通过」均为设计意图，不是实测结论。**
