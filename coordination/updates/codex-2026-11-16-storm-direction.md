# 风暴透镜：指向晨湾港的风向标

## 机制与验收

第三条任务先在坍塌灯塔旁校准一座四向风向标，再取走透镜。玩家从海图和两岛位置判断晨湾港位于浅滩北方；每次与风向标交互，箭头顺时针转 90°，指北时解除透镜锁定。它是空间方位操作，不是第一任务的三目标顺序谜题，也不是第二任务的阅读门槛。HUD、日志、乔恩提示和登岛罗盘在校准前指向风向标，校准后指向透镜。航行中仍指向码头。

验收：未校准取镜无效；转到北方后可取镜；风向标方向跨进程保存，新档清除；旧档阶段整数不改，已到返港/完成阶段的旧档不回退。键鼠路线与完整任务回归应能走通。实际可辨性和手柄理解必须由集成者实机及真人试玩验证。

## 交接

基线：`main` @ `6b5b21d`，分支 `agent/codex/storm-direction`，工作树 `F:\github\agent-storm-direction`。仅交付候选，尚未运行 Unity 构建或图形试听。

修改：

- `StormSignalQuest.cs`、`ProgressStore.cs`：独立保存四向状态（旧档缺键默认东）；未对准时禁止取镜；阶段枚举整数未改。新档清除该键。
- `GameBootstrap.cs`：坍塌灯塔旁生成可交互风向标和可见箭头；未校准时罗盘指风向标，校准后指透镜；乔恩提示更新。
- `MapOverlay.cs`、`PlayerController.cs`、`QuestJournalPanel.cs`：海图标北，靠岸提示与日志说明空间判断。
- `PrototypeKeyboardStormValidator.cs`、`PrototypeQuestLoopValidator.cs`：覆盖未校准阻拦、三次转向、罗盘切换、取镜；`PrototypeNewSaveValidator.cs` 覆盖清档；新增 `PrototypeStormOldSaveValidator.cs` 和 `.meta`，标志 `-verify-storm-old-save`。
- `PrototypeVisualCapture.cs`：修复航图线索加入后透镜截图预置状态不能推进的问题。

静态检查：`git diff --check` 无空白错误；确认新增 `.cs` 与 `.meta` 成对、`StormSignalStage` 整数顺序未修改。**没有编译、运行或截图证据。**

集成者验收顺序：Unity 6.0 发布候选构建；`-verify-storm-old-save`、`-verify-new-save`；从新档依次运行 `-verify-keyboard-first-quest`、`-verify-keyboard-chart-quest`、`-verify-keyboard-storm-quest`、独立进程 `-verify-keyboard-storm-resume`；另起新档跑 `-verify-quest-loop`。图形模式复拍风向标、海图北标和透镜锁定提示，人工检查箭头在四方向可辨、转动时无遮挡、地面步行可达，真人试玩确认能从海图推断港口方向。

风险：风向标坐标 `(-7, .65, -87.4)`、交互站位 `(-7, -85.8)` 是根据灰盒地形推定，尚未在 Unity 里检验碰撞/路径；三个方向旋转后的实际视角和箭头辨认也未实测。旧 `SearchTideflats` 档会出现新增校准步骤，旧返港和已完成档不受门槛影响。若实际路径受塔体阻挡，集成者应调整风向标位置并同步键鼠验证器站位。

补充旧档可见状态：旧 `ReturnToShipwright` / `Complete` 缺少新方向键时，任务对象的 `VaneAligned` 和 `DisplayVaneDirection` 将风向标显示为朝北，且不更改原阶段或写入旧档；未接任务的交互会提示先找乔恩。`-verify-storm-old-save` 新增对此两项游戏层状态的断言，仍待 Unity 运行验证。

接手：Codex 主集成者审查、构建、运行回归和打包；真人测试由用户或试玩者完成。
