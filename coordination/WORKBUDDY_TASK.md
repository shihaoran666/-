# WorkBuddy 实现任务：关键交互确认音

项目：《雾灯群岛》。共享仓库：`https://github.com/shihaoran666/-.git`。此任务的独立工作目录是 `F:\github\agent-workbuddy-presentation`，分支是 `agent/workbuddy/presentation`。先读根目录 `AGENTS.md`、`coordination/STATUS.md`、`雾灯群岛/Assets/Docs/audio/audio-plan.md`，再核对当前 `雾灯群岛/Assets/Scripts/AmbientSoundscape.cs`。

## 需要实现

在自己的分支新增一个**不依赖外部音频文件**的 Unity C# 组件，例如 `雾灯群岛/Assets/Scripts/InteractionCueAudio.cs` 及配套 `.meta`。用程序化音频生成四种可区分的短提示：信标成功、信标失败、拾取发现物、任务交付。组件提供明确的公开播放方法或枚举，供集成者随后在事件处调用。不要在本任务中修改 `GameBootstrap.cs`、`PrototypeHUD.cs`、`QuestJournalPanel.cs` 或任务验证器；这些文件由 DeepSeek 与 Codex 处理。

音效要短、音量受现有总音量设置控制，在标题/暂停/地图/日志时遵守 `AudioListener.pause`，连续触发不能刺耳或无限叠音。使用固定参数和确定性生成，避免每次播放时分配大数组。不要引入需要授权的音源、插件或音乐。

## 交付与验收

1. 只改本任务所需的新文件；提交前确认 Unity `.meta` 成对存在，代码遵循当前 `FoglightIsles` 命名空间。
2. 在 `coordination/updates/workbuddy-2026-11-16-interaction-audio.md` 写明设计、四种音的差异、使用方法、文件清单、静态检查结果、未验证项。不要声称已实机试听或 Unity 构建通过，除非确实完成。
3. 在分支 `agent/workbuddy/presentation` 提交并推送。回复提交号；由 Codex 负责主分支合并、事件接线、Unity 构建、耳机/扬声器实听和回归。

如无法读取本地工作目录，使用 GitHub 上同名分支；若仍无法写入，请只给出具体错误，不要在其他项目目录落地代码。
