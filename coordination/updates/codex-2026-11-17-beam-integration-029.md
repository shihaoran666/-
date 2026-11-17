# 0.2.9 灯塔光束集成交接

- 独立候选：`agent/codex/beam-visual` 的 `b3c9f2c`；集成至本机 `main` 后，保留两层 12 面渐宽光锥、58 米长度、原 5 秒旋转和灯塔亮灭继承。
- 集成调整：候选的 Standard 透明材质在 640×360 浅滩固定机位仍呈大片灰色。改用 `Assets/Resources/FoglightBeam.shader` 的暖色无光照透明渲染，边缘随距离淡出；两层 Mesh 无 Collider。资源与 `.meta` 成对提交。
- 画面证据：`Builds/ReleaseCandidate/review-lighthouse-sweep-1.png` 至 `-3.png` 显示暖橙色横扫和朝向镜头的光锥；`review-lighthouse-unlit.png` 无光束。截图留在本机，不入分发 ZIP。固定机位图形检查不能代替玩家在不同视角和距离下的观感。
- 自动证据：Unity 6.0 Windows 0.2.9 构建、`-verify-quest-loop`、`-verify-quest-resume`、`-verify-beacon-partial-prepare`、`-verify-beacon-partial-resume`、`-verify-gamepad-voyage`、ZIP 独立解压后 `-verify-quest-loop` 均 PASS；包内程序集哈希一致。
- 构建：`FoglightIslesDemo-Windows-0.2.9.zip`，编号 `20261117054424-833F2613ED34`，ZIP SHA-256 `8E6A37A33965AD53A8EE24BEEA17EADDAB0BDACF13A498EA736A471DCE318AF2`。下一接手人：用户与 QA 记录实体手柄和灯塔远景观感。
