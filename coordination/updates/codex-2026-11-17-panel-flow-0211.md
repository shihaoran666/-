# 0.2.11 手柄面板切换集成交接

- 基线：Windows Demo 0.2.11，构建编号 `20261117115651-D29B72F60EE3`。分发 ZIP `雾灯群岛/Builds/Distribution/FoglightIslesDemo-Windows-0.2.11.zip`，SHA-256 `C2DD74801DADBA862C5399A157B017A0491D889B65FDB40FE406833337711D82`。
- 来源：DeepSeek 经 Agent HUD 对 M2 菜单状态做只读审查，指出 `PauseMenu.Update` 在海图/日志打开时直接返回。Codex 新增 `-verify-gamepad-panel-flow`，在 0.2.10 源码开发构建中重现失败，退出码 1，日志为 `Menu did not switch from map to pause`。
- 修改：`PauseMenu.cs` 在海图或日志打开时只由手柄 Menu 触发跨面板切换，先调用各面板 `CloseForPause()`，再打开暂停菜单；Esc/B 保留原面板关闭行为。新增验证又重现 A 从暂停返回时同帧推进一句对白，`DialoguePanel.cs` 现抑制恢复游戏当帧的对话推进。`ProgressStore.cs` 将新验证参数隔离到 `.validation` 进度；`PrototypeGamepadLookValidator.cs` 检查海图、日志、对话与暂停之间的状态、时间流速及 B/A 返回。更新试玩说明与反馈模板。
- 验证：Unity 6.0 候选版构建 PASS；`-verify-gamepad-panel-flow`、`-verify-gamepad-menu`、`-verify-gamepad-voyage`、`-verify-quest-loop`、`-verify-focus-pause`、`-verify-gamepad-look` 均 PASS。ZIP 独立解压后 `-verify-gamepad-panel-flow` PASS，反馈收集脚本可生成包含构建信息、模板和日志的 ZIP。
- 边界：以上为虚拟输入与自动验证，实体手柄上的 Menu/View/Y、面板文字理解、真人靠岸手感和新人首航仍待验收。0.2.10 ZIP 保留，已从该版开始试玩者须注明构建编号，不得把两个版本结果混记。
