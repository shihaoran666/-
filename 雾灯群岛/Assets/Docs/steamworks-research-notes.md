# Steamworks 接入调研备忘（暂缓，未接入）

> 2026-11-09：爸爸决定先验证可玩性，Steam Direct 暂不注册。本文件只保存调研结论，**未写任何接入代码、未安装任何 SDK**。日后恢复时按此执行即可，不必重做勘察。

## 工程现状（勘察结论）

- 无任何 `.asmdef`。
- `Packages/manifest.json` 仅 12 条依赖，无 Steamworks。
- `ProjectSettings` 的 `scriptingDefineSymbols` 为空。
- `cloudProjectId` 为空；`bundleVersion` = `0.1.0-prototype`。

## 存档边界（云存档必须复用）

`Assets/Scripts/ProgressStore.cs` 使用 5 个带版本号的 PlayerPrefs 键：

- `quest.prism.stage.v1`
- `boat.survey-sail.v1`
- `tide-beacons.v1`
- `quest.survey-chart.v1`
- `quest.storm-signal.v1`

并带 `StorageKey()` 后缀机制，区分命名空间：

- `regular`（正式档）
- `.validation`（自动验证）
- `.performance`（性能测试）
- `.playtest.v2`（外测试玩）

`Assets/Scripts/DiscoveryJournal.cs` 另有 4 个 `foglightisles.discovery.<id>` 键。

**关键约束**：云存档适配层必须复用 `StorageKey()`，否则验证跑会污染正式存档。

## 成就候选触发点（全部为已有事件，无需改原文件）

| 触发源 | 可挂成就 |
| --- | --- |
| `QuestState.Changed` | 接取任务 / 取得棱镜 / 交付 |
| `TideBeaconPuzzle.Changed` + `IsComplete` | 三信标按序点亮 |
| `SurveyChartQuest.Changed` | 航图取得 / 交付 |
| `StormSignalQuest.Changed` | 透镜取得 / 主线完成 |
| `DiscoveryJournal.Discover()` | 4 件探索物，可做「集齐」成就 |
| `GameBootstrap.TryUnlockSurveySail()` | 勘测帆升级 |

## 恢复接入的第一步

1. 从 GitHub `rlabrecque/Steamworks.NET` 的 Releases 下载 unitypackage（或走 OpenUPM `com.rlabrecque.steamworks.net`）。
2. 导入后先在 Player Settings → Scripting Define Symbols 加 `STEAMWORKS_NET`。
3. 再写 `SteamManager` 的安全降级初始化（无 Steam 客户端时回落单机，绝不阻塞启动）。
