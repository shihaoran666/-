# 《雾灯群岛》本地化提取报告

生成时间：2026-11-09　范围：`Assets/Scripts/` 下玩家可见文案（不含验证器日志）

## 1. 产出文件

| 文件 | 说明 |
| --- | --- |
| `Assets/Resources/Localization/strings.json` | 227 条，zh-CN / en 双语，键完全对齐，`json.load` 可解析 |
| `Assets/Scripts/Localization/Localizer.cs` | 运行时取串，未改动任何已有 .cs |
| `Assets/Docs/localization.md` | 本文件 |

## 2. 字符串总数与按文件分布

**总计 227 条**（zh-CN 227 / en 227，无缺键、无多余键）。

| # | 文件 | 条数 |
| --- | --- | --- |
| 1 | GameBootstrap.cs | 83 |
| 2 | QuestJournalPanel.cs | 24 |
| 3 | PauseMenu.cs | 20 |
| 4 | PrototypeHUD.cs | 19 |
| 5 | StartScreen.cs | 17 |
| 6 | PlayerController.cs | 11 |
| 7 | DiscoveryJournal.cs | 9 |
| 8 | StormSignalQuest.cs | 9 |
| 9 | SurveyChartQuest.cs | 9 |
| 10 | QuestState.cs | 9 |
| 11 | TideBeaconPuzzle.cs | 6 |
| 12 | MapOverlay.cs | 4 |
| 13 | CompassHUD.cs | 2 |
| 14 | DockingPoint.cs | 2 |
| 15 | BoatController.cs | 2 |
| 16 | DialoguePanel.cs | 1 |

另：`NpcNameplate.cs`、`Interactable.cs`、`DiscoveryPickup.cs` 自身无字面量，文案由调用方注入（已归入 GameBootstrap / DiscoveryJournal 的条目）。

### 键命名分区

`ui.*` 界面框架（开始页/暂停/海图/日志）· `hud.*` 常驻 HUD 与即时提示 · `quest.*` 任务名/状态/阶段目标 · `dialogue.<npc>.*` 对白 · `discovery.*` 发现物 · `guidance.*` 罗盘/引导标签 · `object.*` 可交互物名 · `prompt.*` 交互动词 · `sign.*` 路标 · `place.*` / `dock.*` 地名 · `status.*` 任务状态词 · `message.*` 石碑铭文。

### 验证器中的玩家可见文案

`Prototype*Validator.cs` 与 `PrototypeVisualCapture.cs` 里的中文字符串**全部是测试标记、`GameObject.Find` 查找名或断言关键字**（如 `"手柄交互输入回归标记"`、`"潮汐箭纹石碑"`），仅 `PrototypeVisualCapture.cs:190` 复用了与 `GameBootstrap.cs:510` 相同的结局对白。**无需翻译，但接线后必须同步改验证器的查找名**，否则 `GameObject.Find` 断言会全部失效。

## 3. 接入方案（最简）

不做批量改造，按需接线即可：

1. 新增 `Localizer` 已在 `FoglightIsles` 命名空间内，无需 asmdef 调整。
2. 每条硬编码处替换为 `Localizer.Get("键名")`；带占位符的用 `Localizer.Get("键名", arg0, arg1)`（模板里写 `{0}` `{1}`）。
3. 长文案优先改调用点而非改字段，例如 `QuestState.Objective` 返回 `Localizer.Get(...)`，HUD 无需改动。
4. 语言切换：`Localizer.Language = "en";` 会写入 PlayerPrefs 并触发 `LanguageChanged`；UI 若缓存字符串需订阅该事件重算（`PrototypeHUD.RefreshLabels` 已有脏检查，加一行清空 `_last*` 缓存即可）。
5. 建议先接 4 个高频文件：`PrototypeHUD.cs`、`QuestJournalPanel.cs`、`PauseMenu.cs`、`StartScreen.cs` —— 覆盖 80 条，占玩家高频阅读量的大头。

**注意**：`StartScreen.cs:97` 和 `:110` 的换行在源码里写成转义序列 `\n`（运行时是真换行）。strings.json 里已存成 JSON 的 `\n`（解析后为真换行），接线时直接用 `Localizer.Get` 的返回值，不要再自己拼换行。

## 4. 歧义 / 需人工拍板

1. **`晨鹱号` 的英文名**：现用 `Dawn Petrel`（晨鹱＝一种海鸟）。若设定里船名有典故，需确认。
2. **`港口管理员` vs `港务管理员`**：文中同时出现"港口管理员艾琳"和"港务办公室"。英文统一为 `harbourmaster`，中文是否统一待定。
3. **`潮汐浅滩` / `Tideflats`**：单复数与是否加冠词（the Tideflats）需统一，现按"加 the"处理。
4. **`棱镜祭台` / `prism altar`**：游戏里没有独立祭台模型，只是棱镜生成点。文案是否保留"祭台"这一说法需确认。
5. **`勘测帆` 的数值**：`最高航速 +2.5，转向速度 +8` 是裸数字，无单位，玩家可能读不懂是百分比还是绝对值。
6. **`试玩版`**：英文用 `demo`。若上架版要改成 `prologue`/`trial`，需全局替换。
7. **`风暴透镜`**：中文"透镜"与"棱镜"是两个不同物件，英文分别用 `storm lens` 与 `prism`，确认不会混淆。
8. **`守望者`**：`Watcher` 单数/复数、是否专有名词（the Watcher）待定。

## 5. HUD 布局风险（英文膨胀 1.5–2.5×）

按 CJK 1em / 拉丁 0.52em 粗算，640×360 窗口（此时走 `PrototypeHUD.DrawCompactHeader`，字号 14–19）最危险：

| 排名 | 位置 | 现框宽 | 英文估算 | 结论 |
| --- | --- | --- | --- | --- |
| 1 | `PrototypeHUD.cs:95` 紧凑模式目标行 `目标：{objective}` | 256px | ~459px | **1.8× 溢出，必爆** |
| 2 | `PrototypeHUD.cs:76` 交互提示 `[E] 标题 · 动词` | 440px | ~649px | **1.5× 溢出**，且居中会左右各切掉一截 |
| 3 | `PrototypeHUD.cs:80` 船上状态条 `航速 … · 距码头 … · 靠近…减速` | 596px | ~616px | 临界溢出，码头名一长就换行/被裁 |

次级风险（1920 宽下也会紧）：
- `QuestJournalPanel.cs:134` 任务详情 650px 框，英文约 1168px → 会折成 2 行，但框高固定 67px，**第二行被裁**。
- `StartScreen.cs:105` 标语、`:109` 操作说明 3 行块，640×360 下 `top+250` 起、68px 高，英文折行后压到 Quit 按钮。
- `NpcNameplate.cs:66` 固定 156×30，`Eileen · Harbourmaster` 已 ~172px，**必定截断**。
- `PauseMenu.cs:229` 二次确认文案 410px 框，英文 ~491px，会压到下面的按钮。

规避建议（不改代码也能做）：英文版对这些键改用短文案（如 `Objective: ` → `Goal: `），或把紧凑模式框宽从 272 提到 300、交互提示改为 `wordWrap` + 两行。
