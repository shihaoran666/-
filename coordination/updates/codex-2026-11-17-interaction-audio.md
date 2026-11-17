# 交互短音候选交接

- 基线：Windows Demo 0.2.5，构建编号 `20261116131325-E3D9BA2C9216`。
- 分支：`agent/codex/interaction-audio`；只新增 `InteractionCueAudio.cs`、对应 `.meta` 和本文档。
- 目标：在没有外部音频文件的情况下提供四种短反馈：信标成功（双上行音）、信标失败（双下行音）、发现物（三段高音）、任务交付（三段上行中音）。所有采样在 Awake 时确定性生成，播放时不分配采样数组。

## 接线方式

在现有引导/任务服务 GameObject 上添加一次 `InteractionCueAudio`。事件处调用 `cueAudio.Play(InteractionCue.BeaconSuccess)`，其他值为 `BeaconFailure`、`Discovery`、`QuestHandIn`。组件使用单个非空间化 AudioSource；新事件会停止上一段，且至少间隔 0.09 秒，避免叠音。`AudioListener.volume` 由现有暂停菜单控制；`AudioListener.pause` 和标题、暂停、地图、日志状态会阻止播放。组件不自建音量设置。

## 验证与边界

- 静态检查：`git diff --check`；文件与 `.meta` 成对；不依赖外部音频、插件或 Unity 编辑器文件。
- 未运行 Unity 构建，也未使用耳机或扬声器试听。成功/失败是否足够容易区分、音量是否舒适、重复触发节奏、暂停后是否有残音均待集成者实机确认。
- 下一接手人：Codex 主集成者审查并将四个事件接线，构建后在耳机与扬声器上试听；用现有总音量与暂停/地图/日志操作核对行为。
