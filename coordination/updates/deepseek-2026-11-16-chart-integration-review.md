# 交接：0.2.3 集成的只读复审（残碑引导与旧档逻辑）

- 交接人：DeepSeek（小肥 / DSH）
- 日期：2026-11-16
- 审查对象：`main` @ `6b5b21d`（含 `9d93ab7` 我的实现、`2f66b2f` Codex 的验证修复、`6b5b21d` 罗盘修复）
- 分支：`agent/deepseek/chart-clue-impl`（detached 读过 main，未改主 worktree）
- 边界：**未修改任何文件，未运行 Unity 构建。** 以下全部是静态阅读结论。

---

## 0. 先说结论

Codex 修的两处（`RefreshJourneyGuidance` 登岛后的罗盘目标、`PrototypeQuestLoopValidator` 的取图前置）**都是对的，且覆盖了我原实现的主要漏洞**。

复审后**发现两个仍然存在的问题**，都不是致命阻塞，属于体验/一致性缺陷：

| 编号 | 问题 | 严重度 | 复现难度 |
| --- | --- | --- | --- |
| P1 | 航图拾取后残碑仍在原地可交互，且能抢走玩家经过时的交互焦点 | 中 | 易（走回去即可） |
| P2 | 任务完成后残碑仍是可交互物件，按 E 弹「刻痕已经模糊」 | 低 | 易 |

另有一处**已确认不是问题**（见 §3），一处**低风险但建议顺手加固**（见 §4）。

---

## 1. P1：残碑在取图后仍会抢交互焦点

### 事实

- 残碑位置 `(6.6, 0.9, -84.6)`，航图 `(8.3, 1.0, -83.8)`，两者相距 **1.88 m**（`GameBootstrap.cs:367`、`:220`）
- 航图在拾取后 `SetActive(false)`（`GameBootstrap.cs:234`），**残碑没有任何对应隐藏逻辑**——`_surveyClueTablet` 全文只出现在引导目标里（`:778`、`:897`）
- 交互选择是 **2.25 m 内取最近者**（`PlayerController.cs:229-256`）

### 复现

1. 新档推进到 `SearchRuin`，读残碑、取航图（阶段 → `ReturnToHarbour`）
2. 玩家从航图位置往回走，经过 `(6.6, -83.6)` 一带
3. 交互提示会显示「[E] 遗迹残碑 · 辨认航图残记」

### 为什么会发生

航图消失后，残碑成为该区域唯一的 `Interactable`，玩家返航要经过遗迹入口（验证器自己的路径也是 `(5f, -78f)` → 码头），**擦过残碑半径内是常态**。

### 最小修复（二选一，都是一行）

**方案 1（推荐，与航图一致）**：在 `RefreshWorldForQuest` 里让残碑随阶段隐藏

```csharp
// 雾灯群岛/Assets/Scripts/GameBootstrap.cs，约 :752 附近，与 _surveyChart 那行并列
if (_surveyClueTablet != null)
    _surveyClueTablet.gameObject.SetActive(_surveyQuest.Stage == SurveyChartStage.SearchRuin);
```

注意：这行必须放在 `_surveyChart.SetActive(...)` 旁边，因为 `RefreshWorldForQuest` 已由 `_surveyQuest.Changed` 订阅（`:77`），拾图后会自动触发。

**方案 2（保留残碑作装饰，只关掉交互）**：拾图后销毁/禁用其 `Interactable` 组件

```csharp
// 更动 GameBootstrap.cs:387 附近，ReadRuinClue 回调内无法拿到自身 Interactable，
// 建议改为在 :234 的 chartInteraction 回调里同时处理：
if (_surveyQuest.Stage == SurveyChartStage.ReturnToHarbour)
{
    _surveyChart.SetActive(false);
    var clue = _surveyClueTablet != null ? _surveyClueTablet.GetComponent<Interactable>() : null;
    if (clue != null) clue.enabled = false;
}
```

**我建议方案 1**：语义最清楚（残碑是「探索阶段的线索」，不是永久地标），而且和航图的生命周期一致；若美术后续要给残碑做常驻装饰，再改方案 2。

---

## 2. P2：任务完成后残碑仍可交互

### 事实

`SurveyChartQuest.ReadRuinClue()`（`SurveyChartQuest.cs:82-100`）只在 `Stage == SearchRuin` 分支写入线索，
其他阶段走：

```csharp
if (Stage != SurveyChartStage.SearchRuin)
{
    PrototypeHUD.SetMessage("残碑上的刻痕已经模糊，看不出更多航路。 ");
    return;
}
```

这个兜底文案本意是防误触，但因为残碑**永久存在**，任务完成后玩家仍能按 E，得到一句无实义的提示。

### 复现

完成航图任务（`Complete`）后回到遗迹，对残碑按 E → 弹「残碑上的刻痕已经模糊，看不出更多航路。」

### 最小修复

采用 P1 的方案 1 后，**P2 自动消失**（残碑只在 `SearchRuin` 存在）。
若采用 P1 方案 2，则 `ReadRuinClue()` 的非 `SearchRuin` 分支建议直接**静默返回**，或把 `Interactable` 也一并禁用。

---

## 3. 已确认不是问题（避免重复排查）

### 3.1 风暴透镜分支会不会抢走残碑引导？——不会

`RefreshWorldForQuest`（`:764`）确实把风暴透镜分支排在航图分支**前面**，但 `StormSignalQuest.TalkToJon()`（`StormSignalQuest.cs:29-33`）前置条件是：

```csharp
if (_surveyQuest.Stage != SurveyChartStage.Complete)
{
    PrototypeHUD.SetMessage("乔恩：等艾琳把航图理清…");
    return;
}
```

即 **风暴任务必须等航图 `Complete` 才解锁**。两者不可能同时处于「进行中」，分支顺序不构成冲突。

### 3.2 两套引导方法会不会互相打架？——不会，且 Codex 的修复是必要的

- `RefreshWorldForQuest`：事件驱动（`Changed` 订阅）
- `RefreshJourneyGuidance`：**每帧**调用（`Update()` @ `:107-110`）

每帧的那套会覆盖事件那套。这正是 `6b5b21d` 必须修 `RefreshJourneyGuidance` 的原因——我原实现只改了事件驱动的那套，登岛后每帧刷新会把罗盘目标改回航图。**Codex 的修复正确且完整。**

### 3.3 还有别的地方写 guidance 吗？——没有

全仓库只有 `GameBootstrap.cs` 调用 `SetTarget`；`MapOverlay`、`PrototypeHUD` 只读 `_guidance`。不存在第三方覆盖路径。

---

## 4. 低风险加固建议（非阻塞）

### 4.1 `PrototypeChartOldSaveValidator.Fail()` 不清理验证作用域

`PrototypeChartOldSaveValidator.cs:88-92` 的 `Fail()` 直接 `Quit(1)`，而**恢复逻辑只写在成功路径末尾**（`:76-80`）。失败时会留下 `Stage=SearchRuin/Complete` 等脏状态在 `.validation` 作用域里。

**为什么目前无害**：`Tools/dsh-qa-batch.sh` 每个验证项都是**独立进程**（`run()` 各起一次 exe，`:17-18`），且各验证器自己会预设阶段。所以脏状态不会跨验证项传染。

**为什么仍建议加**：若将来有人把多个 `-verify-*` 串在同一个进程里（或加 `-verify-all`），失败路径就会开始互相污染。加一行防御性清理成本很低：

```csharp
private void Fail(string reason)
{
    PlayerPrefs.DeleteKey(ProgressStore.StorageKey(ProgressStore.SurveyClueReadKey));
    ProgressStore.SaveSurveyChartStage(SurveyChartStage.Locked);
    PlayerPrefs.Save();
    Debug.LogError("[FoglightIsles Chart Old Save] FAIL — " + reason);
    Application.Quit(1);
}
```

**注**：同样的不清理写法在既有验证器里也是惯例（`PrototypeKeyboardChartValidator.cs:253-258`），所以这不算本次引入的回归，只是顺手可做的加固。

### 4.2 `Interactable.IsComplete` 写了但从不读取

`Interactable.Interact()` 每次都设 `IsComplete = true`，但全仓库没有任何消费方（只有 `TideBeaconPuzzle.IsComplete` 是同名无关属性）。意味着**所有可交互物件都能重复触发**。

对残碑无害（`ReadRuinClue()` 幂等）；对棱镜/透镜也无害（阶段检查兜底）。属于既有设计，不是本次问题，仅记录。

---

## 5. 变更文件清单

**本次审查未修改任何文件。** 本交接文档是唯一新增内容。

若采纳 §1 方案 1，预期改动仅 1 处：
`雾灯群岛/Assets/Scripts/GameBootstrap.cs` — `RefreshWorldForQuest` 内新增残碑 `SetActive` 一行（+3 行含判空）。

---

## 6. 需要谁接手

| 事项 | 接手方 |
| --- | --- |
| §1 残碑生命周期修复（采纳方案 1 与否） | Codex（集成者） |
| §4.1 Fail 路径清理加固（可选） | Codex |
| 残碑位置与交互半径的真人可辨认性 | 用户试玩 |

---

**声明：本审查为静态阅读，未运行 Unity，未验证 P1/P2 在真实画面中的表现。**
P1 的判定依据是代码事实（残碑无隐藏逻辑 + 交互取最近者 + 1.88 m 间距），但「玩家实际返航时是否会进入 2.25 m 半径」需要用 0.2.3 包实测确认——如果实测发现路径离得够远，P1 可降级为观感问题而非体验问题。
