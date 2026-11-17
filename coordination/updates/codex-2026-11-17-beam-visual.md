# 灯塔扫掠光束形态候选交接

- 基线：`main` @ `a3f7a26`，Windows Demo 0.2.8。
- 分支：`agent/codex/beam-visual`；只改 `雾灯群岛/Assets/Scripts/GameBootstrap.cs`。
- 目标：将侧视如硬杆的实心圆柱替换成两层渐宽的低面数光锥。内层较亮，外层较淡，均从灯头向外延伸原有 58 米；灯塔原有旋转、修复状态和存档路径不变。

## 实现与静态检查

`BuildBeamShell` 为每层生成 12 面开口锥壳，使用项目现有 `FoglightRuntimeMaterial` 的 Standard 材质克隆，设置透明混合和发光。两层均为纯 MeshFilter/MeshRenderer，没有 Collider；仍挂在原有反向缩放的 Beam Pivot 下，随 Lighthouse Beacon 的启停继承亮/灭状态。

- `git diff --check` PASS。
- 未运行 Unity 构建、抓帧或真人试玩；主集成者需在 640×360 和 1280×720 比较灯塔亮/灭与扫掠连拍，确认从潮汐浅滩可见但不遮挡航线 HUD。
- 需要回归 `-verify-quest-loop`、`-verify-quest-resume`、`-verify-gamepad-voyage`，并确认光束无碰撞。

## 风险

透明材质与雾的叠加依赖当前 Standard 渲染设置，亮度、远端边缘和两面叠加只能通过实际画面判断。若光束太实，可先降内层 alpha；若 640×360 远景消失，可提高内层 alpha 或远端半径。该候选未加入动画衰减或新 shader 资产。

下一接手人：Codex 主集成者。
