# 0.2.13 安全码头续档交接

- 目标：M3 存档在潮汐浅滩下船后能恢复角色与可登船的小船，避免“继续探索”总回晨湾港。海上退出时回上次成功下船的码头；旧存档没有码头键则默认晨湾港。
- 修改：`ProgressStore` 新增作用域内的码头键，`BoatController.TryDisembark` 在成功下船后保存；`GameBootstrap` 在浅滩恢复人船；`StartScreen` 按码头调整欢迎语与继续按钮；新档清除码头键。验证器使用独立 `.validation.safe-dock` 作用域，避免影响其他回归。`PLAYTEST.txt` 和反馈模板写明检查点语义。
- DeepSeek 只读审查：指出初稿把删键放错方法、浅滩船壳与栈桥重叠；集成者修正为在 `ClearPrototypeProgress` 删键，并将船泊在栈桥东侧水域，恢复验证检查船壳与栈桥包围盒不相交。其他建议为真实玩家继续观察：海上退出回码头是否符合预期、岛上朝向是否易辨。
- 验证：Unity 6.0.85f2 发布候选构建 PASS；`-verify-safe-dock-prepare` 与另起进程 `-verify-safe-dock-resume` PASS，恢复后可从码头互动登船；`-verify-new-save`、`-verify-title-new-save`、`-verify-gamepad-voyage`、`-verify-quest-loop` PASS；0.2.13 ZIP 独立解压后再次跨进程浅滩续档 PASS。构建编号 `20261117122950-BFC4E7D9E609`；ZIP 165 文件，33.1 MB，SHA-256 `3B4187B6EF31A6007D4E4B0726FF656586D789AA71DD766FAD3C7C91236E10A4`。
- 待接手：用户用实体手柄与新人试航；在浅滩下船退出重开、再次登船和航行中退出回码头需真人确认。视听辨识、第三条任务差异化仍待反馈。试玩包保存在 `雾灯群岛/Builds/Distribution/FoglightIslesDemo-Windows-0.2.13.zip`，不入 Git。
