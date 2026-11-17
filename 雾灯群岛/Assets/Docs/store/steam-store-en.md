# Foglight Isles · Steam 英文 Demo 文案（内部待验收草案）

> **状态：内部草案，不可直接提交或公开。** 对照 Windows Demo 0.2.8（构建编号 `20261117035653-A2978CFB2517`）和当前源码撰写。英语尚未接入游戏界面；实体手柄往返、陌生玩家找路、目标硬件性能和英文小窗口排版仍待验收。文案与玩家实际体验核对后才能转为商店稿。
>
> 下文的 **Demo 现有内容** 仅描述当前可运行切片；**完整游戏方向** 是开发意向，不表示已经制作或承诺具体岛屿、谜题、时长与平台。Steam 页面字段、素材尺寸、语言勾选和审核要求在提交前须以当时的官方 Steamworks 文档核对，本文件不据此作平台规则断言。

## Store Name · 候选

`Foglight Isles`

中文名暂用《雾灯群岛》；正式上架名称由用户决定。

## Short Description · Demo 候选

```text
Explore Morningbay Harbor and the Tidal Shallows by boat. Follow compass bearings and sea chart markers, read stone clues, solve a three-beacon puzzle, and complete commissions that change the harbor in this single-player demo.
```

当前短描述计数为 **227 字符**（含空格与标点）；最终提交前须重新计算。本句不声称没有地图标记、每次发现都会改变港口，或有多座完整岛屿。

## About This Game · Demo 现有内容

```text
Foglight Isles is a single-player sailing and exploration demo. Begin at Morningbay Harbor, take the helm of a small boat, and cross the fog to the Tidal Shallows. Your compass and sea chart show the direction of the current objective; signs and landmarks help you recognize the route.

Meet Eileen, the harbor administrator, and Jon, the shipwright. Follow three connected commissions: restore the harbor lighthouse, recover a lost chart, and bring back a storm lens. At the shallows, read a stone clue and light three tide beacons in the correct order. Read a ruin inscription before taking the chart, then use the sea chart to align a wind vane before retrieving the storm lens.

Returning with completed commissions changes the harbor: the lighthouse lights up, a chart appears by the navigation table, and a signal lamp comes on. An earned sail upgrade changes how the boat handles. Beyond the main commissions, four optional discoveries lead to an additional route display at the harbor.

The demo saves quest and discovery progress locally. You can use a quest journal, a compass, and a sea chart while you explore.
```

本段是 0.2.8 的**候选英文表达**，不是英语已在游戏内可选的证明。实体手柄输入映射已实现，但真人手柄往返与菜单可用性仍待确认，因此暂不在公开文案宣称完整手柄支持。

## Full Game Direction · 开发方向，暂不作为现有功能宣传

```text
The project aims to grow beyond this harbor-and-shallows demo into a larger journey of sailing, exploration, local stories, and visible changes to the places you revisit. The scope and features of that larger game will be defined through playtesting and development.
```

此段只能在页面明确区分现有 Demo 与未来规划时使用。不要把未来岛屿数量、沉船、每岛谜题、动态雾、水流导航、配音或游玩时长写成现有功能。

## Feature Bullets · Demo 候选

```text
• Sail between Morningbay Harbor and the Tidal Shallows, dock, and continue exploring on foot.
• Use a compass, sea chart, journal, signs, and landmarks to follow the current objective.
• Read stone clues and solve a three-beacon sequence puzzle at the shallows.
• Complete three connected commissions that restore visible parts of the harbor.
• Find four optional discoveries and unlock a route display when you bring their story home.
```

## Supported Languages · 内部核对

| Language | 当前 0.2.3 状态 | 商店勾选前门槛 |
| --- | --- | --- |
| Simplified Chinese | 玩家可见界面目前以中文为主 | 在分发包中复查全流程和遗漏文本 |
| English | 有 `Localizer.cs` 与 `strings.json` 草案，但当前玩家界面未接线 | 完成接线、翻译复核、640×360 与 1080p 抓帧、键鼠/手柄全流程 |
| 其他语言 | 未实现 | 有实际构建与完整校对后再决定 |

当前无角色语音资产，不能勾选 Full Audio。英语文本草案不能作为“已支持英语界面”的证据。

## System Requirements · 内部待实测草案

以下仅为**待测试设备候选**，不是可提交的最低/推荐配置。目标设备、画质、分辨率、平均帧率和低帧率未形成完整实测报告。

| 候选档位 | OS | CPU | Memory | GPU | 目标（未验证） |
| --- | --- | --- | --- | --- | --- |
| 最低候选 | Windows 10 64-bit | Intel Core i5-4460 / AMD Ryzen 3 1200 | 8 GB | GTX 960 / RX 560 | 1080p Low 30 FPS |
| 推荐候选 | Windows 11 64-bit | Intel Core i5-10400 / AMD Ryzen 5 3600 | 16 GB | GTX 1660 / RX 5600 XT | 1080p High 60 FPS |

存储空间、DirectX 版本及最终配置应按实际发布包与目标机器验证后填写；不可用上述候选数字声称已通过性能门槛。

## 标签与素材 · 内部候选

- 与当前切片相符的标签候选：Exploration、Sailing、Adventure、Singleplayer、Puzzle、Third Person。是否使用 Atmospheric、Relaxing 等体验标签，待真人试玩反馈与最终画面核对。
- 暂不采用 Open World、Story Rich、Great Soundtrack、Multiplayer、VR 等会制造错误预期的标签。
- 截图应从实际构建拍摄并标注构建编号：晨湾港与 NPC、出航航线与船 HUD、浅滩石碑与三信标、灯塔修复前后、返港航图/信号桅杆及可选路线陈列。不要用概念主视觉冒充实机画面。
- 商店胶囊、标题字标、截图、预告和页面字段均待正式素材制作及官方规范复核。

## 转为可提交文案的验收门槛

1. 真人用实体手柄完成去程、靠岸下船、返港与标题/地图/日志/暂停操作；至少一名陌生玩家无口头指导完成首航。
2. 英语真正进入分发构建后，复查标题、HUD、罗盘、地图、日志、NPC 长对白、交互提示和设置菜单；640×360 与 1920×1080 均无裁切，英语读者理解任务目标。
3. 目标硬件实测后填写系统需求；实际试玩计时后才考虑时长描述。
4. 每个商店承诺对应当前构建截图、测试记录或代码证据；平台规则再查官方 Steamworks 文档。

核对记录见 [`coordination/updates/production-2026-11-16-store-fact-check.md`](../../../../coordination/updates/production-2026-11-16-store-fact-check.md)。
