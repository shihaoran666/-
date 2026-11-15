# Steam 商店页 · 英文成品文案（Foglight Isles）

> **中文说明（不提交给 Steam，仅内部使用）**
>
> - 本文件是英文商店页的**可提交成品文案**：字段名对应 Steamworks「Store Page → Edit Store Page」里的实际栏位，可直接复制粘贴。
> - 短描述硬上限 300 字符，本稿 268 字符（含空格），留了余量；改动后必须重新数字符。
> - **文案纪律**：当前可玩内容是 30–45 分钟垂直切片。凡本文写到「完整游戏」的段落，都是**商店页定位语**，必须在正式 Early Access / 1.0 页面上线前逐条核对是否属实；Demo 页只用「About the demo」一节的措辞，不得把未来功能写成已有。
> - 主胶囊 / 截图 / 预告尚未产出，见 `capsule-specs.md`、`screenshot-shotlist.md`、`trailer-script.md`。缺图时**不要**先公开商店页（Steam 要求页面公开后至少 2 周才能发售，且缺图会被审核退回）。
> - 标题字标（logo）尚未制作。Steam 要求胶囊上除游戏 logo 外不得有任何文字，因此 logo 是**硬阻塞项**，见 `launch-checklist.md`。
> - 所有系统需求为草案，未经目标机实测，标 `[DRAFT]`，不得当作已验收数据。

---

## 1. Store Name

```
Foglight Isles
```

> 中文名《雾灯群岛》用于中文商店页与国内宣发。Steam 同一 App 只能有一个商店名，若要中英双语标题，官方做法是在 About 首段首句点出中文名，**不要**在标题里塞 `雾灯群岛 / Foglight Isles`（会拉低搜索与列表可读性）。

---

## 2. Short Description（≤300 characters）

```
Sail a small boat through a fogbound archipelago where the map gives you a bearing, not an arrow. Read the water, follow lighthouse beams and half-remembered landmarks, solve tide-lit puzzles, and bring every find home — the harbor changes because you came back.
```

字符数：**268 / 300**（含空格与标点）。

备选（更短，偏氛围，232 字符）：

```
A single-player sailing exploration game. No map markers, no quest arrows — just fog, lighthouse beams, and the landmarks you learn to trust. Every discovery you carry home changes the harbor you left.
```

---

## 3. About This Game

```
The fog took the charts. It did not take the lights.

Foglight Isles is a single-player exploration game about moving through a sea you cannot see across. You start in Morningbay Harbor with a borrowed boat, a lantern, and a handful of commissions nobody else wanted. Beyond the breakwater, the islands are out there — but the old sea roads are gone from every map, and the only things still pointing the way are lighthouses, stone markers, and the things people half-remember.

You navigate by sight. Your compass tells you which way you are facing and roughly where your objective lies; it never plants a glowing arrow in the world. To find a route you read the water, watch for a beam cutting the fog, and learn which silhouette belongs to which island. Getting lost is part of the route. So is the moment the fog thins and the shape you have been chasing finally resolves into a shore.

The islands are hand-built, not generated. Each one hides a small environmental puzzle — tide beacons that must be lit in the right order, an altar that only opens when the water agrees, a wreck that gives up its cargo at the right angle of light. Nothing here is solved by combat; it is solved by looking, listening, and trying the wrong order once.

And then you sail home. This is the part that matters. The harbor you return to is not a menu you pass through: the lighthouse comes back to life, the chart table fills with paper, the signal mast runs a new color, and the people who sent you out have something new to say. Upgrades you earn change how the boat handles on the next crossing, so the sea you already crossed is never quite the same sea twice.

Foglight Isles is a quiet game about attention — about learning a coastline well enough that you stop needing to be told where you are. It is built for one player, one boat, and the long way around.
```

### About the demo

```
The current demo is a 30–45 minute vertical slice: Morningbay Harbor, the Tidal Shallows, two harbor characters (Eileen and Jon), three connected commissions, one three-step tide-beacon puzzle, and four optional discoveries. It runs on keyboard/mouse or gamepad, is fully voiced in text, and saves your progress automatically. Everything above describes the direction of the full game; the demo shows the loop, not the whole coastline.
```

> **合规提醒**：若商店页对应的是「完整游戏」而非 Demo 页，`About the demo` 段应移到 Demo App 的商店页，或改写为 `What's in the current build`。不要在正式页把 Demo 内容说成完整游戏体量。

---

## 4. Feature List

```
• Navigate without markers — A compass bearing and your own eyes. No floating arrows, no minimap dots, no "objective 340 m" overlay. Landmarks are the map.
• Lighthouse beams as roads — Follow a light through the fog to find a shore. Lights you restore become permanent routes for later crossings.
• Hand-built islands, not generated terrain — A small, deliberate archipelago where every silhouette is placed to be recognized from the water.
• Tide-beacon puzzles — Read the stone, light the beacons in the right order, and watch the sea open a door it had closed.
• A harbor that answers back — Every return changes Morningbay: the lighthouse relights, the chart table fills, the signal mast changes color, and the people who sent you out react to what you brought home.
• A boat that grows with you — Earn upgrades that change top speed and handling, so old routes feel different on the second pass.
• Built for one player, keyboard or gamepad — Full gamepad support, adjustable volume, field of view, camera speed, and display mode. Progress saves automatically.
```

（7 条，符合 5–7 条要求。若需压到 5 条，删「Hand-built islands」与「A boat that grows with you」，但这两条是差异化卖点，建议保留。）

---

## 5. System Requirements `[DRAFT — 未实测]`

### Minimum

| Field | Value |
| --- | --- |
| OS | Windows 10 64-bit |
| Processor | Intel Core i5-4460 / AMD Ryzen 3 1200 |
| Memory | 8 GB RAM |
| Graphics | NVIDIA GeForce GTX 960 (2 GB) / AMD Radeon RX 560 (4 GB) |
| DirectX | Version 11 |
| Storage | 4 GB available space |
| Additional Notes | Targets 1920×1080 at 30 FPS on Low. Requires a keyboard and mouse or an XInput-compatible gamepad. |

### Recommended

| Field | Value |
| --- | --- |
| OS | Windows 11 64-bit |
| Processor | Intel Core i5-10400 / AMD Ryzen 5 3600 |
| Memory | 16 GB RAM |
| Graphics | NVIDIA GeForce GTX 1660 (6 GB) / AMD Radeon RX 5600 XT (6 GB) |
| DirectX | Version 12 |
| Storage | 4 GB available space |
| Additional Notes | Targets 1920×1080 at 60 FPS on High. |

> **内部备注**：当前 Release Candidate 包体约 33 MB / 163 文件，存储写 4 GB 是为正式版留量，**上线前必须按真实安装体积复核**。Steam 不接受「未测试」的推荐配置作为承诺，M5 外测后要用至少 3 台真机填实测帧率，再决定是否降档。不支持 macOS / Linux，不要勾选。

---

## 6. Steam Tags（候选，≤15）

按优先级排列，前 5 个决定推荐算法归类，不要凑数：

| # | Tag | 理由 |
| --- | --- | --- |
| 1 | Exploration | 核心动词 |
| 2 | Sailing | 唯一性的第一识别 |
| 3 | Adventure | 大类归属 |
| 4 | Singleplayer | 明确单人 |
| 5 | Atmospheric | 雾海氛围是卖点 |
| 6 | Puzzle | 潮汐信标谜题 |
| 7 | Mystery | 失落航线/旧物传闻 |
| 8 | Relaxing | 无战斗、节奏舒缓 |
| 9 | Indie | 身份标签 |
| 10 | Third Person | 第三人称角色+船 |
| 11 | Stylized | 美术方向（灰盒阶段慎用，见备注） |
| 12 | Naval | 船只题材 |
| 13 | Story Rich | 艾琳/乔恩与港口变化（**正式版再定**） |
| 14 | Open World | **不要用**：设计边界明确写了「不制作无缝开放世界」 |
| 15 | Walking Simulator | 不建议：会引来错误预期 |

实际建议提交 12 个：1–13 去掉 14/15，并暂缓 11、13 直到美术与剧情体量到位。**不要**用 `Great Soundtrack`（当前无音乐）、`Female Protagonist`、`Multiplayer`、`VR`。

---

## 7. Supported Languages（占位表）

| Language | Interface | Full Audio | Subtitles |
| --- | --- | --- | --- |
| English | ✔ | — | ✔ |
| Simplified Chinese | ✔ | — | ✔ |
| Traditional Chinese | 待定 | — | 待定 |
| Japanese | 待定 | — | 待定 |
| French / German / Spanish (EU) / Russian / Portuguese (BR) | 待定 | — | 待定 |

> **内部备注**：当前工程为**全中文硬编码，无本地化层**（见 `Assets/Docs/launch-command-plan.md`）。上表英文列是**目标状态**，不是现状。在本地化层落地并完成英文全流程回归前，商店页只能勾选 Simplified Chinese；否则属于虚假陈述，退款与差评风险极高。Full Audio 一栏当前无配音资产，保持 `—`。

---

## 8. 提交前必须补齐（Checklist 摘要）

- [ ] 标题字标 logo（透明底 PNG，含中英两版）
- [ ] 主胶囊 616×353、header 460×215、small 231×87、vertical 374×448、library 600×900、library hero 3840×1240、page background 1438×810
- [ ] 至少 5 张（建议 8 张）1920×1080 截图
- [ ] 30 秒预告（MP4 / H.264 / 1920×1080）
- [ ] 英文文案由母语者或专业译者过一遍（当前稿为可提交级，但仍建议润色）
- [ ] 系统需求按实测填写，删除 `[DRAFT]`
- [ ] 隐私政策 URL 与 EULA（见 `launch-checklist.md`）

## 9. 参考链接（官方）

- Steamworks 文档总入口：https://partner.steamgames.com/doc/home
- 商店素材规范：https://partner.steamgames.com/doc/store/assets
- 上架流程入门：https://partner.steamgames.com/doc/gettingstarted
- 预告片规范：https://partner.steamgames.com/doc/store/trailer
- 成就：https://partner.steamgames.com/doc/features/achievements
- Steam 云：https://partner.steamgames.com/doc/features/cloud
