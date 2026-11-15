你是《雾灯群岛》(Foglight Isles) 的实测 QA 工程师。任务：真的把游戏跑起来，用自动化验证跑通完整流程，报告实际发生的阻塞与缺陷。不要只读代码猜测。

工程根目录：E:\Codex\游戏制作\雾灯群岛
Unity 编辑器：F:\Unity\6000.0.85f2\Editor\Unity.exe
发布候选包：Builds\ReleaseCandidate\FoglightIslesDemo.exe
开发构建：Builds\Windows\FoglightIslesDemo.exe（有 GC 统计，仅用于性能）

## 第一步：先摸清可用的验证参数

在 Assets/Scripts/Prototype*Validator.cs 里 grep 出所有真实支持的命令行参数名（不要猜），确认每个参数的用途、是否需要图形模式。已知存在的可能包括：
-verify-journey、-verify-quest-loop、-verify-quest-resume、-verify-discoveries、-verify-performance、-verify-walk-route、-verify-keyboard-journey、-verify-keyboard-island-resume、-verify-keyboard-first-quest、-verify-keyboard-chart-quest、-verify-keyboard-storm-quest、-verify-keyboard-free-roam、-verify-keyboard-free-roam-resume、-verify-gamepad-voyage、-verify-gamepad-menu、-verify-gamepad-look、-verify-new-save、-verify-title-new-save、-verify-display-mode、-verify-runner-selftest

## 第二步：必跑批处理验证

用 Builds\ReleaseCandidate\FoglightIslesDemo.exe 逐个跑，每项都要记录：退出码、日志中的 PASS/FAIL 行、异常堆栈。每个命令设 180 秒超时，超时即记失败。

1. -batchmode -nographics -verify-journey
2. -batchmode -nographics -verify-quest-loop
3. -batchmode -nographics -verify-quest-resume（必须独立进程）
4. -batchmode -nographics -verify-discoveries
5. -batchmode -nographics -verify-runner-selftest（或实际找到的自检参数）

日志重定向到文件再读，不要依赖控制台截断。注意：验证会写独立验证存档，不要污染常规存档。

## 第三步：性能实测（图形模式）

Builds\Windows\FoglightIslesDemo.exe -batchmode -verify-performance -screen-fullscreen 1 -screen-width 1920 -screen-height 1080
记录港口/浅滩/自动航行三段的平均 FPS、1% low、最慢帧、p95 帧时、GC 分配。

## 第四步：崩溃与边界测试（重点，别只跑 happy path）

1. 反复启动退出：连续启动游戏 5 次后立刻杀进程，检查存档是否损坏、能否正常续档。
2. 存档破坏测试：找到存档实际位置（用户 PlayerPrefs 注册表 或 AppData\LocalLow\Foglight Isles\Foglight Isles Demo），备份后用垃圾数据覆盖，再启动，看是否崩溃、是否安全回退新档。
3. 分辨率边界：-screen-width 640 -screen-height 360 与 2560x1440 各启动一次，检查崩溃；有抓帧参数就用（如 -capture-small-review），判断 UI 是否裁切。
4. 同时运行两个实例，看存档是否互踩。

## 第五步：汇总官方承认的未验证项

grep Assets/Docs/milestones.md 中包含「待真人」「仍待验收」「尚未」「未完成」的行，汇总成清单。

## 硬约束

- 只允许创建临时文件和独立验证存档；不要改 Assets/ 下的代码，不要删构建产物。
- 不要用 batchmode 跑需要图形的验证模式（会卡住）。图形验证要设超时并主动结束进程。
- 杀掉游戏进程时不要误杀其他程序（只杀 FoglightIslesDemo.exe）。

## 交付（中文，作为最终回答直接输出）

1. 实测结果表：参数 → 结果 → 关键日志证据
2. 真实缺陷清单：按严重度排序（崩溃 > 存档 > 阻塞流程 > UI 裁切 > 性能），每条给复现步骤与证据
3. 官方承认未验证的功能清单
4. 明确结论：这个包现在能不能稳定发给陌生玩家试玩？（能/不能 + 前提）
