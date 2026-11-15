# 交接：失落航图 · 可保存遗迹线索步骤方案

- 交接人：DeepSeek（小肥 / DSH）
- 日期：2026-11-16
- 分支：`agent/deepseek/chart-clue-review`
- 任务来源：`coordination/STATUS.md` 中「DeepSeek：只读检查第二条任务与现有引导，提出可执行的线索步骤和新旧存档验收用例」
- 基线：Windows Demo 0.2.2，构建编号 `20261115132634-E3B97ADA4755`
- 边界：**未修改任何 Unity 源文件，未运行任何 Unity 构建。** 本文档仅为集成者提供可实施的设计与验收用例。

---

## 1. 现状核对（读代码得到的事实）

### 1.1 `SurveyChartStage` 与持久化

`Assets/Scripts/SurveyChartQuest.cs:6`

```csharp
public enum SurveyChartStage { Locked, SearchRuin, ReturnToHarbour, Complete }
```

整数值依次为 `0/1/2/3`。存档在 `Assets/Scripts/ProgressStore.cs:12` 与 `:80-91`：

- 键名 `foglightisles.quest.survey-chart.v1`
- 以 `int` 存入 PlayerPrefs，读取时校验范围 `Locked ≤ value ≤ Complete`，越界回落到 `Locked`

**结论：枚举顺序是旧存档契约的一部分，任何插入新值到中间的改动都会把旧档的 `SearchRuin(1)` 读成别的阶段。新状态必须走独立键。**

### 1.2 当前拾取没有任何前置条件

`SurveyChartQuest.cs:55-64`

```csharp
public void CollectChart()
{
    if (Stage != SurveyChartStage.SearchRuin)
    {
        PrototypeHUD.SetMessage("一张被潮水浸湿的航图。也许艾琳会知道它的用途。 ");
        return;
    }
    SetStage(SurveyChartStage.ReturnToHarbour);
    PrototypeHUD.SetMessage("取得失落航图：" + Objective);
}
```

只要 `Stage == SearchRuin`，走到航图旁按交互即可拿到，**与是否看过任何线索无关**。这正是 `coordination/STATUS.md` 指出的「第二、三条任务仍有同岛拾物返港的重复感」的根因：第二条任务只是换了坐标的第一次任务。

### 1.3 仓库里已有一条“先读线索”的既成模式

`GameBootstrap.cs:332-362`（`BuildTideClue`）已经实现过一次同类设计，且带持久化：

```csharp
interaction.Title = "潮汐箭纹石碑";
interaction.Prompt = "查看信标顺序";
interaction.OnInteracted = _ =>
{
    _tideClueRead = true;
    ProgressStore.SaveTideClueRead();
    PrototypeHUD.SetMessage("石碑上的灯纹从西向东排列：…");
};
```

对应存储：`ProgressStore.cs:14, 108-113` → 独立键 `foglightisles.tide-clue-read.v1`，`HasReadTideClue` / `SaveTideClueRead`。

**结论：「失落航图」的线索步骤应完全复用这套已验证的模式，而不要发明新机制。集成风险因此主要集中在阶段分支与引导分支，而不是持久化本身。**

### 1.4 受影响的联动点（全部在 `GameBootstrap.cs`）

| 位置 | 作用 | 线索步骤需要的影响 |
| --- | --- | --- |
| `:74-77` | `Changed` 事件挂到 `RefreshWorldForQuest` | 线索读取后必须触发一次世界刷新 |
| `:232-233` | 航图交互回调 `CollectChart()` + 隐藏自身 | 未读线索时应拦截并提示，而不是拾取 |
| `:724` | `_surveyChart.SetActive(Stage == SearchRuin)` | 未读线索时航图**仍应可见但不可取**（或改为残碑才可见，见 §2 方案 B） |
| `:740-744` | `GuidanceMarker` 指向航图 / 晨湾港 | 未读线索时应指向残碑，读后才指向航图 |
| `:729` | `_harbourChartDisplay` 在完成时出现 | 不变 |
| `PrototypeHUD.cs:150` | HUD 目标文案取 `_surveyQuest.Objective` | 未读线索时目标文案应切换为“先找遗迹线索” |
| `QuestJournalPanel.cs:161-172` | 日志中该任务的状态与明细文案 | 需补“线索未读”子状态文案 |

---

## 2. 线索步骤方案（推荐 A，附 B 备选）

### 2.1 方案 A（推荐）：遗迹残碑 + 就地解锁，不改枚举

**新增存储（独立键，向前兼容）**

在 `ProgressStore.cs` 新增，与 `TideClueReadKey` 同构：

```csharp
private const string SurveyClueReadKey = "foglightisles.quest.survey-chart.clue.v1";

public static bool HasReadSurveyClue => PlayerPrefs.GetInt(StorageKey(SurveyClueReadKey), 0) == 1;

public static void SaveSurveyClueRead()
{
    PlayerPrefs.SetInt(StorageKey(SurveyClueReadKey), 1);
    PlayerPrefs.Save();
}
```

并在 `:144` 附近的清档逻辑中一并 `DeleteKey`，保证“新档试玩”能完整回到零状态。

**新增世界物件（推荐放在现有石台旁，不新开地块）**

现有航图位置：`GameBootstrap.cs:231` → `(8.3f, 1f, -83.8f)`，石台 `(8.3f, .25f, -83.8f)`。

建议残碑放在 `(6.6f, .9f, -84.6f)`（同一遗迹内、玩家必经路径上，避免让玩家多跑一趟），命名与交互：

- `Title = "遗迹残碑"`，`Prompt = "辨认航图残记"`
- 交互回调写入 `ProgressStore.SaveSurveyClueRead()`，向 `PrototypeHUD.SetMessage` 输出一段**指向性**线索（不是氛围文字），例如：

> 残碑上刻着旧航线的起笔：「航图藏于石台，须先记下此纹，方得取走。」

**拾取拦截**

`SurveyChartQuest.CollectChart()` 改为：

```csharp
public void CollectChart()
{
    if (Stage != SurveyChartStage.SearchRuin) { /* 原文案不变 */ return; }

    if (!ProgressStore.HasReadSurveyClue)
    {
        PrototypeHUD.SetMessage("航图被石台的旧纹压住，先辨认旁边的遗迹残碑。");
        return;   // 关键：不改变 Stage，不出世界目标
    }

    SetStage(SurveyChartStage.ReturnToHarbour);
    PrototypeHUD.SetMessage("取得失落航图：" + Objective);
}
```

**为什么放在 `CollectChart()` 而不是世界物件回调里**：拦截点是唯一的（所有进入路径都经过 `CollectChart()`），且不需要改动 `GameBootstrap.cs:232` 的 `SetActive(false)` 时序——未读时 `Stage` 不变，航图不会被隐藏。

**引导联动**

`RefreshWorldForQuest`（`:740-744`）当前分支：

```csharp
if (_surveyQuest.Stage == SurveyChartStage.SearchRuin)
    _guidance.SetTarget(_surveyChart.transform, "失落航图");
```

改为：

```csharp
if (_surveyQuest.Stage == SurveyChartStage.SearchRuin)
{
    if (ProgressStore.HasReadSurveyClue)
        _guidance.SetTarget(_surveyChart.transform, "失落航图");
    else
        _guidance.SetTarget(_surveyClueTablet, "遗迹残碑");
}
```

为此需在 `GameBootstrap` 增加字段 `private Transform _surveyClueTablet;`，并在 `BuildTideflats`（或残碑构建方法）里赋值，与现有 `_tideClueTablet`（`:361`）写法一致。

**HUD 与日志文案**

- `PrototypeHUD.cs:150` 附近：`SearchRuin` 且未读线索时，objective 用「辨认遗迹残碑上的旧航线」替代 `Objective`
- `QuestJournalPanel.cs:171`：`SearchRuin` 且未读线索时，明细显示「先辨认遗迹残碑，才能取走航图」

**实现成本估计**：`ProgressStore` +3 个成员，`SurveyChartQuest` 改 1 个方法，`GameBootstrap` +1 字段 / +1 构建方法 / 改 1 个引导分支，`PrototypeHUD` 与 `QuestJournalPanel` 各改 1 处文案分支。**不新增资产，不改枚举，不改存档键名。**

### 2.2 方案 B（备选）：航图在读完线索前不生成

把 `:724` 的 `SetActive` 条件从 `Stage == SearchRuin` 收紧为 `Stage == SearchRuin && HasReadSurveyClue`，让航图在读完残碑后**凭空出现**（或加一个短暂的显现反馈）。

- 优点：玩家不可能点到一个“点了没反应”的物件，交互体验更干净
- 缺点：需要额外处理“残碑读完后如何提示航图已出现”（否则玩家不知道回石台），并且会改动 `:232` 的隐藏时序，回归面比 A 大

**建议**：先做 A 保证语义正确与回归安全；B 作为后续手感优化，等真人反馈再决定。

---

## 3. 旧存档兼容策略

### 3.1 必须保留的不变量

1. **`SurveyChartStage` 枚举顺序不可变**（`Locked=0 … Complete=3`）
2. **存档键 `foglightisles.quest.survey-chart.v1` 的名称与类型不可变**
3. **`SurveySailKey`、`PrismQuest` 相关键、繁荣度键不动**

### 3.2 旧档进入时的三种情形

| 旧档状态 | 新增线索键 | 期望行为 | 处理方式 |
| --- | --- | --- | --- |
| `SearchRuin`，玩家尚未取图 | 不存在 | **不应把玩家卡住** | 视为未读：引导指向残碑；玩家读一次即可继续（这是新设计意图，可接受） |
| `ReturnToHarbour`（已取图未交付） | 不存在 | 正常交付，不受影响 | 拾取阶段早已通过，`CollectChart()` 的拦截分支不会被执行 |
| `Complete` | 不存在 | 正常，港口陈列照旧 | 不涉及 |

**注意上表第一行的取舍**：如果希望老玩家完全无感，可改为“`Stage >= SearchRuin` 且键缺失时默认视为已读”。这会让老档跳过新线索步骤（好处是无摩擦，坏处是新内容对老玩家不可见）。

**我的建议**：**默认按未读处理**（上表写法），理由是新步骤是这次交付的玩法内容，老档也应该看到；而且它只多一步交互，不会让玩家丢失进度。但如果集成者判断“老档不能出现任何新增阻碍”，改为默认已读也只需在 `HasReadSurveyClue` 里加一行回落判断，两种都能在 10 分钟内切换。

### 3.3 回滚安全性

新键独立，旧版本程序读到它只会忽略。**旧版客户端打开新档不会崩，也不会误读阶段。** 这是选择独立键而非扩展枚举的主要收益。

---

## 4. 验收用例（新旧档分别执行）

以下用例面向集成者在 Unity 内的验证，命令沿用仓库现有 `-verify-*` 约定。**注意：本机我只做只读审查，未运行以下任何一项。**

### 4.1 新档 · 未读拦截（核心）

| 编号 | 步骤 | 通过判据 |
| --- | --- | --- |
| N1 | 新档 → 完成灯塔棱镜 → 找艾琳接下航图委托（`Stage = SearchRuin`） | `SurveyChartStage.SearchRuin`，`HasReadSurveyClue == false` |
| N2 | 直接走到石台对航图按交互 | **阶段仍为 `SearchRuin`**；收到「先辨认遗迹残碑」提示；航图**未被隐藏**（`GameObject.Find("失落航图") != null`） |
| N3 | 走到残碑按交互 | `HasReadSurveyClue == true`；提示含“石台 / 取走”指向性文字 |
| N4 | 再对航图按交互 | `Stage = ReturnToHarbour`；航图对象消失；引导改为「返回晨湾港」 |
| N5 | 返港交付艾琳 | `Stage = Complete`；`Harbour Chart Display` 出现；繁荣度 +1 |

### 4.2 新档 · 读线索后跨进程续档

| 编号 | 步骤 | 通过判据 |
| --- | --- | --- |
| R1 | 在 N3 之后（已读线索、未取图）退出游戏 | —— |
| R2 | 重启并继续游戏 | `HasReadSurveyClue == true`；`Stage == SearchRuin`；引导**直接指向航图**而不是残碑；残碑可再次交互但不改变状态 |
| R3 | 取图 → 退出 → 重启 | `Stage == ReturnToHarbour`；航图对象不存在；引导指向晨湾港 |

### 4.3 旧存档兼容

| 编号 | 前置 | 步骤 | 通过判据 |
| --- | --- | --- | --- |
| L1 | 旧档停在 `SearchRuin`（本次改动前产生） | 启动新版本继续 | 不崩溃；`Stage` 仍为 `SearchRuin`（未被误读）；引导指向残碑 |
| L2 | 同上 | 读残碑 → 取图 → 返港交付 | 完整走通，与其他新档路径一致 |
| L3 | 旧档停在 `ReturnToHarbour` | 继续并交付 | 直接进入 `Complete`，**不要求**补读线索 |
| L4 | 旧档 `Complete` | 继续游戏 | 港口陈列与船只升级原样恢复，`Harbour Chart Display` 存在 |
| L5 | 旧档 `Complete` | 用“清除进度 / 新档试玩” | 新键被一并 `DeleteKey`，重开后 `HasReadSurveyClue == false` 且 `Stage == Locked` |

### 4.4 回归（不得回退的既有行为）

| 编号 | 判据 |
| --- | --- |
| G1 | `-verify-keyboard-chart-quest` 仍 PASS（该验证器路径会经过拾取点；**若它在新拦截下失败，说明它需要在取图前补一次残碑交互** —— 见 §5 风险 1） |
| G2 | `-verify-keyboard-chart-resume` 仍 PASS（完成态与港口陈列、`MaxForwardSpeed == 13.5f`、`TurnSpeed == 60f`） |
| G3 | `-verify-keyboard-first-quest` 与 `-verify-keyboard-storm-quest` 不受影响 |
| G4 | 键鼠与虚拟手柄往返自动回归仍 PASS（`-verify-gamepad-voyage`） |

---

## 5. 已识别的风险与需要集成者决定的事项

1. **`PrototypeKeyboardChartValidator` 会因新拦截而失败（预期）。**
   `:101-106` 是「走到航图 → 按 E → 断言 `Stage == ReturnToHarbour`」。加上拦截后，这一步在未读线索时会停在 `SearchRuin`。
   **建议**：在该验证器的拾取之前插入一次残碑交互（坐标可用 §2.1 建议的 `(6.6f, .9f, -84.6f)`），并新增一条断言「未读时拾取被拒」。这属于验证器改动，超出本次只读授权范围，故未实施。

2. **残碑位置需要实机确认可达性。**
   建议坐标 `(6.6f, .9f, -84.6f)` 是我根据航图 `(8.3f, 1f, -83.8f)` 与石台 `(8.3f, .25f, -83.8f)` 推的相对位置，**未在游戏内验证**碰撞、遮挡与交互半径。请集成者在实机确认，或按现有 `BuildSignpost` 的可达性判据调整。

3. **老档默认“未读”还是“已读”需要产品判断。**（见 §3.2）我倾向未读，但这是可逆的开关。

4. **第二条任务的“重复感”只解决了一半。**
   本方案给它加了「先读线索再取物」的步骤差异，但**行动骨架仍是同岛拾取 + 返港交付**，与第一条任务同构。若 M3（12/02）要求的是“行动类型差异”，还需要第三条任务或新交互类型来承担（例如需要两次点位、需要携带某物、需要限时）。本次未展开，建议列入 M3 议题。

5. **未验证项声明**：本文档所有“通过判据”均为设计意图，**没有任何一条在本机实际执行过**。所有涉及运行时的结论都应视为待验证。

---

## 6. 变更文件清单

**本次提交只新增本文档，未改动任何 Unity 源文件。**

若集成者采纳方案 A，预期改动文件为：

- `Assets/Scripts/ProgressStore.cs`：+`SurveyClueReadKey`、+`HasReadSurveyClue`、+`SaveSurveyClueRead`、清档时 `DeleteKey`
- `Assets/Scripts/SurveyChartQuest.cs`：`CollectChart()` 增加未读拦截分支
- `Assets/Scripts/GameBootstrap.cs`：+`_surveyClueTablet` 字段、+残碑构建方法、改 `RefreshWorldForQuest` 引导分支
- `Assets/Scripts/PrototypeHUD.cs`：未读时的 objective 文案分支
- `Assets/Scripts/QuestJournalPanel.cs`：未读时的日志明细文案
- `Assets/Scripts/PrototypeKeyboardChartValidator.cs`：拾取前补残碑交互 + 新增“未读拦截”断言（**§5 风险 1**）

---

## 7. 需要谁接手

| 事项 | 接手方 |
| --- | --- |
| 审查本方案、决定 §3.2 与 §5.2 的取舍 | Codex（集成者） |
| 按方案 A 实施代码改动、跑 §4 全部用例 | Codex（集成者） |
| 残碑位置与交互半径的实机确认 | Codex 实机，或用户试玩时反馈 |
| M3“第三条任务行动差异”议题 | 下一轮规划（我可在只读范围内继续出方案） |

---

**交接完毕。** 本分支仅含此文档，未触碰主工程。
