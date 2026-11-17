# 信标状态形态候选交接

- 基线：本机 `main` @ `699894e`，Windows Demo 0.2.7，构建编号 `20261117034116-32B4724E95DA`。
- 分支：`agent/codex/beacon-state-visual`。目标：让三座潮汐信标的点亮状态能从轮廓判断。
- 修改：`雾灯群岛/Assets/Scripts/GameBootstrap.cs`、`TideBeaconPuzzle.cs`。每座信标新增一根竖直暖色点亮柱（共 3 个 Primitive）；装饰 Collider 创建当帧禁用并销毁，不改变原信标交互物体与半径。

## 状态路径

`RegisterBeacon` 现在同时登记球体和点亮柱。`RefreshBeaconVisual` 使用原有 `index < _progress` 判定，在更新灯光的同一处开关点亮柱。因此新档初始三根隐藏，依次点亮后逐根出现；错误顺序令 `_progress = 0` 后 `RefreshAllVisuals` 全部隐藏；旧存档读入时每次登记会按已保存的进度同步。没有新增存档键，也没有改谜题顺序、NPC/任务阶段或船只逻辑。

## 静态检查与待验收

- `rg -n 'RegisterBeacon\(' 雾灯群岛/Assets/Scripts` 仅找到定义和 `GameBootstrap` 的调用，签名一致。
- `git diff --check` PASS；只改 2 个现有 C# 文件，无新 Unity 资产或 `.meta`。
- 未运行 Unity 构建、游戏截图或真人测试。集成者需构建 Windows，运行 `-verify-quest-loop`、`-verify-quest-resume`、`-verify-new-save`；在 640×360 和 1280×720 对比 0/1/2/3 根点亮柱，故意按错顺序后确认全灭，重启读取部分进度后确认轮廓一致。人测需判断远景是否真的能数清。

已知限制：新增柱体是灰盒硬边，且颜色沿用暖光材质；本候选没有实现艺术计划 A3 建议的 1.5 秒熄灭动画或闪白。若实测过亮或与祭台混淆，优先调柱高和宽度，不改谜题状态。下一接手人：Codex 主集成者。
