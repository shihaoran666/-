# WorkBuddy 交接 · 中英本地化与 Steam 草稿核对

- 日期：2026-11-16
- 分支：`agent/workbuddy/copy-review`
- 核对对象：`Assets/Docs/store/steam-store-en.md`、`Assets/Docs/steam-store-page.md`、`Assets/Resources/Localization/strings.json`、`Assets/Scripts/`（只读）
- 边界：**未修改任何 Unity 脚本、未动场景、未发布商店页**。本文件是我唯一的写入。

## 0. 一句话结论

商店稿的**方向**没错，但有 3 处承诺与 0.2.2 实证相反。最硬的一条是「英文界面 ✔」：本地化表 227 个键中英齐全，**全工程 0 处调用**，玩家在游戏里看不到任何一个英文字符。这条不修，英文列不能勾。

## 1. 核对方法（可复现）

```bash
cd 雾灯群岛/Assets/Scripts
grep -rn "Localizer\." --include=*.cs . | grep -v "Localization/Localizer.cs"   # → 空
grep -rn "ui.map.legendTarget" --include=*.cs .                                  # → 空
grep -nP '"[^"]*[\x{4e00}-\x{9fff}]' MapOverlay.cs | grep -v Debug.Log
grep -c "" ../Resources/Localization/strings.json                                # 227 键 / 语言
```

结论均来自**静态代码与文本**，我没有运行 Unity、没有实机跑 0.2.2。凡涉及画面与手感，一律标「待实机/待真人」。

## 2. 承诺 → 0.2.2 实证 → 删改（主表）

| # | 商店稿条款（节选） | 0.2.2 实证 | 判定 | 处置 |
|---|---|---|---|---|
| 1 | “No floating arrows, **no minimap dots**, no ‘objective 340 m’ overlay” | `MapOverlay.cs:94` 海图明确画出 `◆ 当前目标` 与两处地名；`CompassHUD.cs:65` 罗盘条画 `◆` 并输出「目标在左后方」；`hud.boat.dockDistance` 显示「距码头 {0} m」 | **与实证冲突** | 只保留「世界空间无悬浮箭头」这一半（`GuidanceMarker.cs` 确认只是数据持有者，无 3D 标记）。删掉 “no minimap dots / no distance overlay”。 |
| 2 | “Lighthouse beams as roads … Lights you restore become permanent routes” | 港口有灯塔 `Light`（`GameBootstrap.cs:433`）与信号桅杆灯，但**没有任何导航光柱、也无线解锁航线机制** | **未实现** | 降级为氛围描述（“the harbour light is the only thing you can see across the fog”），不要写成机制。 |
| 3 | “an altar that only opens when the water agrees” | 棱镜祭台 + 三信标顺序谜题 ✓（`TideBeaconPuzzle`） | 一致 | 保留 |
| 4 | “a wreck that gives up its cargo at the right angle of light” | 工程内无 wreck 对象 | **未实现** | 删 |
| 5 | “chart table fills with paper, the signal mast runs a new color” | `Harbour Chart Route`×3、`Harbour Signal Lamp`+`Light`(`GameBootstrap.cs:469-477`) 均存在 | 一致 | 保留（可点名这两处物件，可信度更高） |
| 6 | “Upgrades … change how the boat handles” | 勘测帆：最高航速 +2.5、转向 +8（`hud.message.boatUpgrade`） | 一致 | 保留 |
| 7 | “Progress saves automatically” | `ProgressStore` 每次状态变更即 `PlayerPrefs.Save()` | 一致 | 保留 |
| 8 | “Full gamepad support” | 自动虚拟手柄往返 PASS；**实体手柄往返待真人验收**（见 STATUS.md 基线） | **待验证** | 改为 “gamepad support (verified on a virtual pad; physical pad test in progress)”，或等真人签收后再写 Full。 |
| 9 | Supported Languages：English / Interface ✔ | 表齐全但**零调用**；默认 `zh-CN`；无任何脚本设置 `Localizer.Language`；仓库内无语言切换 UI | **与实证相反** | **P0：英文列不得勾**，只能勾 Simplified Chinese。详见 §5。 |
| 10 | “is fully voiced in text” | 无配音资产，纯文本 | 语病 | 改 “text only — no voice acting”。 |
| 11 | “30–45 minute vertical slice” | 无计时记录可查 | 待验证 | 保留，但内部标注「未实测」，别写进正式页的硬承诺。 |
| 12 | 三委托 / 四发现物 / 三步信标谜题 / 双角色 | 与代码逐条一致（`SurveyChartQuest`、`StormSignalQuest`、`ProgressStore`、4 个 discovery key） | 一致 | 保留 |

## 3. 术语对照：建议**改稿**而不是改代码

商店稿与游戏内英文是**两套地名**。本地化一旦接线，玩家会看到商店里没有的名字。

| 商店稿 | 游戏内实际（`strings.json` en） | 建议 |
|---|---|---|
| Morningbay Harbor | **Morning Bay** | 全稿统一为 Morning Bay |
| the Tidal Shallows | **Tideflats** | 全稿统一为 Tideflats |
| （未提船名） | **Dawn Petrel**（晨鹱号） | 首段点名船名，比 “a borrowed boat” 更有记忆点 |
| Eileen / Jon | Eileen · Harbourmaster / Jon · Shipwright | 一致，保留 |

理由：游戏内 en 文本已经固定这些名字，且被 `PrototypeRuntimeValidator` 等断言间接依赖；改代码的连带风险明显大于改稿。

## 4. 短描述改写（字符数已复核）

原稿标注「268 / 300」，**实测 262**——差 6 字符，说明上次改完没重新数。这本身违反稿子自己写的「改动后必须重新数字符」。

| 版本 | 文本 | 字符数 |
|---|---|---|
| A（建议主用） | A single-player sailing game set in a fogbound archipelago. No floating arrows over the water: you read the coastline, the lighthouse light and the stone markers, light three tide beacons in the right order, and carry every find home to a harbour that changes because you came back. | 282 |
| B（更短） | A single-player sailing game in a fogbound archipelago. No arrows over the water - you read the coastline and the lighthouse, light three tide beacons in order, and carry every find home to a harbour that changes because you came back. | 235 |
| C（最小改动） | Sail a small boat through a fogbound archipelago where your compass gives a bearing, not a glowing arrow in the world. Read the water, follow the harbour light, solve three tide-beacon puzzles, and bring every find home - the harbour changes because you came back. | 264 |

A/B/C 均去掉「no map markers」式的绝对化表述，改为可被代码支持的说法。

## 5. 本地化接线现状（P0 的根因）

| 项 | 实测 |
|---|---|
| `strings.json` | zh-CN 227 键 / en 227 键，无缺键 |
| `Localizer.cs` | 完整可用（回退 zh-CN、`FormatException` 保护、`LanguageChanged` 事件） |
| **调用方** | **0**（排除自身后 grep 为空） |
| 玩家可见硬编码中文（排除 Debug.Log） | MapOverlay 6 行 / PauseMenu 20 / StartScreen 16 / QuestJournalPanel 35 / PrototypeHUD 21 / GameBootstrap 114 |
| 表与代码脱节证据 | `PauseMenu` 有「窗口尺寸」标签，`strings.json` 里**没有对应键** |
| 海图 | `ui.map.*` 6 个键从未被使用，`MapOverlay.cs` 全部硬编码 |
| 语言切换 | 无入口，`PlayerPrefs` 恒为 `zh-CN` |

**接线陷阱（务必转告集成者）**：`PrototypeRuntimeValidator.cs:12` 的 `RequiredObjects` 里有 5 个中文对象名（`晨湾港码头`、`潮汐浅滩码头`、`港口管理员艾琳`、`船匠乔恩`、`西侧/中央/东侧潮汐信标`、`潮汐箭纹石碑`）。这些名字同时是**玩家看到的交互提示**和**回归断言值**——改名做本地化会直接把自动回归打红，必须同步改成按 tag 或查表值断言。

## 6. 最小接线顺序（建议，由集成者决定是否采纳）

1. UI 三层先接表：`StartScreen` / `PauseMenu` / `MapOverlay`，key 对齐现有 227 键，补「窗口尺寸」新键。
2. `PauseMenu` 加一行语言选择（zh-CN / en），切换后靠 `LanguageChanged` 重建界面。
3. 物件名与提示文案改走表，同步修 `PrototypeRuntimeValidator` 断言。
4. 英文全流程回归 + 640×360 溢出检查。重点键：`ui.start.controls`、`ui.pause.controlsKeyboard`、`ui.pause.controlsGamepad`、`ui.journal.discoveryFooter`、`hud.discovery.allFound`。

## 7. 需要 Codex / 用户决定

1. 术语：接受「商店向游戏内靠拢」（我的建议），还是反过来改代码？
2. 英文勾选时机：接线 + 全流程回归通过后再勾，还是先勾中文、英文留到 M4？
3. 短描述选 A / B / C。
4. gamepad 条款措辞：等实体手柄真人验收，还是先写「in progress」。

## 8. 我没有做的事

- 没有实机运行 0.2.2，没有截取 640×360 溢出实证。
- 没有做英文母语者润色（当前只能保证术语一致与事实为真）。
- 没有建分支之外的任何改动，也没有向 `main` 提交。
