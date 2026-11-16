# 交接：Steam 英文稿事实核对（0.2.2）

- 负责人：production agent；分支 `agent/production/store-fact-check`
- 基线：Windows Demo 0.2.2，构建编号 `20261115132634-E3B97ADA4755`
- 范围：只读核对 `雾灯群岛/Assets/Docs/store/steam-store-en.md`、当前源码与项目 README；未改 Unity 文件、未运行构建或真人试玩。
- 结论：英文稿目前是**内部草案**，不能按文件开头的“可提交成品文案”发布。部分文字描述了未来愿景，部分与当前 0.2.2 功能相反。

## 商店承诺 → 0.2.2 实证 → 删改

| 英文稿位置 / 原承诺 | 当前证据与风险 | 建议删改文本或动作 |
| --- | --- | --- |
| Short Description: “fogbound archipelago”, “every find home — the harbor changes” | `GameBootstrap.cs:65-69,186-243` 仅有晨湾港与潮汐浅滩；港口变化由任务和集齐四件发现物触发，并非每次发现。 | “Explore Morningbay Harbor and the Tidal Shallows by boat. Follow a compass bearing, read stone clues, solve a three-beacon puzzle, and bring discoveries home to restore the harbor in this single-player demo.” |
| Short Description 备选: “No map markers, no quest arrows” | `MapOverlay.cs:90-92` 显示玩家、船、任务目标；`CompassHUD.cs:63-73` 显示目标菱形，`GameBootstrap.cs:738-874` 设定目标。与事实相反。 | 删除备选句。可改成 “A compass and sea chart help you find your next objective.” |
| About: “The fog took the charts”, “old sea roads are gone from every map” | 这是世界观设定；实际 `MapOverlay.cs` 仍提供区域海图与目标标记。玩家可能理解为没有地图。 | 保留氛围句时紧接说明：“Your sea chart shows the route and current objective; landmarks help you recognize the shore.” |
| About: “You navigate by sight”, “read the water”, “fog thins” | 代码确有罗盘、地标、灯塔光柱；未找到“读水流”或动态散雾作为可操作玩法。 | 改为 “Use the compass, sea chart, harbor lights and island landmarks to orient yourself.” 删除水流与散雾机制暗示。 |
| About: “The islands are hand-built, not generated” | `GameBootstrap.cs:65-69,186-243` 在运行时用 `Primitive(...)` 创建灰盒几何。岛屿布局有人工坐标，但不是正式手工资产。 | 暂删；待正式场景与资产替换后再描述制作方式。 |
| About: “Each one hides a small environmental puzzle”, “a wreck ... at the right angle of light” | 当前只有潮汐浅滩的三信标顺序谜题（`GameBootstrap.cs:198-202,316-362`）；未见沉船/角度光谜题。 | 改为 “At the Tidal Shallows, read a stone clue and light three tide beacons in order.” 删除沉船与每岛谜题承诺。 |
| About: “Getting lost is part of the route”, “one player, one boat” | 单人、一船属实；海面有边界保护与指引，刻意迷路并非已验证的体验价值。 | 保留 single-player / boat，删去鼓励迷路的保证性表述，等陌生玩家测试。 |
| About / features: “every return changes Morningbay”, “lights ... become permanent routes” | `GameBootstrap.cs:724-730,429-447` 有灯塔、航图陈列、信号桅杆的完成状态；每次返港不一定变化，灯光并未被证明为可追踪道路系统。 | “Completing commissions restores the lighthouse, chart display, and signal mast.” 不称每次返港或永久道路。 |
| Feature: “No floating arrows, no minimap dots, no objective 340 m overlay” | 无常驻世界箭头的说法大致符合代码，但海图有目标菱形，船上 HUD 显示码头距离（`README.md` 操作段）。 | “A compass bearing and an optional sea chart show your objective while landmarks help you navigate.” |
| About demo: “30–45 minute”, “fully voiced in text” | 尚无真人计时样本；`AmbientSoundscape.cs` 只有程序化风潮，NPC 为文本对白。“fully voiced in text”自相矛盾，易被理解为有配音。 | 删除时长与 voiced。改为 “The current demo includes two explorable areas, two harbor characters, three linked commissions, one tide-beacon puzzle and four optional discoveries. Progress saves locally.” |
| Feature: “Full gamepad support” | `README.md` 列手柄映射、自动虚拟手柄往返通过；实体手柄往返与新人易用性尚未验收（`coordination/STATUS.md`）。 | 发布前完成实体手柄往返并确认标题、地图、日志、暂停全流程；在此之前只内部写 “gamepad controls implemented, physical test pending”。 |
| Supported Languages: English ✔ | `Assets/Scripts/Localization/Localizer.cs` 与 `Resources/Localization/strings.json` 是草案，`rg 'Localizer\.' Assets/Scripts` 无调用；玩家可见 HUD/菜单仍中文。 | 当前商店语言仅按已验证的简体中文填写。英文界面、字幕需接线、逐屏校对与构建验证后再勾选。 |
| System Requirements: 最低/推荐硬件与 30/60 FPS | 英文稿自己标 `[DRAFT]`；当前只有开发机性能基线，目标配置未实测。 | 不把表格作为已验证配置提交；在目标硬件上记录画质、分辨率、平均及低帧率后填写。 |

## 可作为下一版英文 Demo 文案的简短草案

> Explore Morningbay Harbor and the Tidal Shallows by boat. Follow a compass bearing, read stone clues, solve a three-beacon puzzle, and bring discoveries home to restore the harbor in this single-player demo.

> Meet Eileen and Jon, complete three connected commissions, and search both areas for four optional discoveries. Your sea chart and quest journal help you keep track of the route; progress saves locally.

此文案只取当前代码支持的事实，仍须在**实际 0.2.2 构建和真人试玩**中核对英语术语与玩家理解，不能因为代码存在就标记“商店审核通过”。

## 英文接线后的 640×360 验收

1. 先将英语真正接入构建；当前 JSON 存在不等于界面已本地化。建立新档并分别抓 640×360、1920×1080 实际游戏截图。
2. 检查标题/新档确认、暂停与设置、任务 HUD、罗盘、地图目标、日志两页、NPC 长对白、交互提示、靠岸 HUD、通关提示。不得裁切、遮挡按钮或覆盖目标文字。
3. 键鼠和手柄分别操作；英文长词与占位符要在最大 FOV、不同显示模式及安全区下复看。发现溢出时先缩短文本，再审布局。
4. 执行语言切换、重启续档与新档流程，确认缓存标签刷新；再次收一名英文读者的首航理解反馈。

## 商店截图镜头清单（待实际构建拍摄）

1. 晨湾港首屏：码头、灯塔、告示牌与玩家目标；确认 UI 可读。
2. 晨鹱号航行：雾海、可辨地标、航行 HUD 与靠岸提示；实体操控时复拍。
3. 潮汐浅滩：石碑与三信标的场景关系；不要把谜题解法全部剧透。
4. 返港变化：灯塔修复前后同机位对照，必要时补航图台与信号桅杆。
5. 自由探索回报：四件发现物后的旧航线陈列，明确为可选奖励。

所有截图应来自实际构建，附构建编号、分辨率、拍摄地点与素材来源。概念主视觉 `Assets/Art/Concept/foglight-isles-steam-keyart-v1.png` 只能作为方向参考，不应冒充实机画面。

## 尚需集成者与用户处理

- 集成者：把当前英文草案标记为内部待核对，尤其删除“可直接复制粘贴”的状态声明；接线完成后再做英文 640×360 截图和任务全流程验证。
- 用户/试玩者：确认实际试玩时长、实体手柄使用、英文读者理解与目标硬件表现。
- Steam 页面格式、素材尺寸、审核时间等**平台规则**可能变化；此审查未联网核实，正式提交前按官方 Steamworks 文档重新检查。
