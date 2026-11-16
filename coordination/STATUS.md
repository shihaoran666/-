# 协同状态

更新：2026-11-16。当前 GitHub 共享仓库为 `shihaoran666/-`，游戏工程位于 `雾灯群岛/`。此页由集成者维护；其他 agent 在 `coordination/updates/` 交接，以减少并发冲突。原 `E:\Codex\游戏制作\雾灯群岛` 保留为历史副本；后续变更以本仓库为准。

## 当前基线

- Windows Demo 0.2.3；构建编号 `20261116011424-14EEE5716A67`。试玩 ZIP 不入库，保存在集成者本机 `雾灯群岛/Builds/Distribution/`。
- 用户已用实体键鼠完成晨湾港到潮汐浅滩的去程、靠岸和下船；历史试玩没有记录准确构建编号与用时。
- 自动虚拟手柄往返和探索/任务续档回归通过；实体手柄往返、陌生玩家独立找路仍待真人验收。

## 当前任务与负责人

| 负责人 | 下一交付 | 工作边界 |
| --- | --- | --- |
| 用户 | 从 0.2.3 ZIP 新档完成实体手柄往返，并观察遗迹残碑线索是否易懂；记录手柄型号、用时、靠岸次数、卡点、截图和日志 | 真人测试，不以脚本代替 |
| Codex | 整理反馈、修复阻塞项；集成 WorkBuddy 音频候选并完成实机试听与回归 | 唯一主工程集成与构建负责人 |
| DeepSeek | 已在独立分支实现“失落航图”残碑线索；后续按试玩反馈处理独立玩法小任务 | 不直接修改主工程，不运行构建 |
| WorkBuddy | 在 `agent/workbuddy/presentation` 分支按 `coordination/WORKBUDDY_TASK.md` 实现四种程序化交互确认音 | 独立组件和交接文件；尚未通过 Agent HUD 送达，待客户端钩子连通或用户手动转发 |
| 视听工具/制作人 | 交付地标辨识草图、单件低模原型、关键事件短音候选及来源/授权记录 | 独立目录，集成前实机审查 |

DeepSeek 已通过 Agent HUD 交付失落航图方案及实现，见 [`deepseek-2026-11-16-chart-clue.md`](updates/deepseek-2026-11-16-chart-clue.md) 和 [`deepseek-2026-11-16-chart-clue-impl.md`](updates/deepseek-2026-11-16-chart-clue-impl.md)。Codex 合入后补全任务闭环验证，并从共享仓库构建 0.2.3：Unity 构建、旧档兼容、新档、键鼠首任务、航图键鼠流程及跨进程续档、三任务闭环均 PASS；ZIP 独立解压后旧档验证 PASS，包内程序集哈希一致。QA agent 的 [`qa-2026-11-16-playtest-gate.md`](updates/qa-2026-11-16-playtest-gate.md) 与文案 agent 的 [`production-2026-11-16-store-fact-check.md`](updates/production-2026-11-16-store-fact-check.md) 仍是待真人和商店实证核对的交接。WorkBuddy 分支已推送 GitHub，但 Agent HUD 仍显示客户端钩子未回报，任务尚未直接送达。

## 交接规则

1. 先在自己的分支和 `coordination/updates/` 写交接。主工程代码、构建与发布由集成者串行处理。
2. 问题必须指向文件或画面、构建编号、重现步骤和验收条件；未知或未经真人验证的结果标为“待验证”。
3. 下一质量门槛是 11 月 18 日前收实体手柄往返与新人首航反馈。M3 的任务差异化目标截止 12 月 2 日。

详细分工见 [`雾灯群岛/AGENT_COORDINATION.md`](../雾灯群岛/AGENT_COORDINATION.md)。
