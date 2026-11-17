# 垂直切片试玩验收清单

在 Unity 6 中打开 `Assets/Scenes/Main.unity` 并进入 Play Mode。每轮测试开始前，使用菜单 `Foglight Isles > Clear Prototype Progress` 清除任务、谜题和升级存档，确保从新档开始。

## 启动与控制

- Console 出现 `[FoglightIsles Validation] PASS`，且没有红色异常。
- 首先显示标题界面；在 640×360 小窗口中“开始探索”（有进度时为“继续探索”）、“新档试玩”和“退出”仍应可见、可点。按 Enter、手柄 A 或点击首个按钮后才可移动、驾驶和交互。
- 640×360 下港口首屏的目标与步行操作、船上航速与码头距离、靠岸说明和航行操作应完整显示；海图、任务日志与探索笔记的正文、页签和关闭说明不得裁切。
- 角色可用 WASD 与左摇杆移动；鼠标与右摇杆可改变镜头。
- Shift 与按下左摇杆可奔跑；Esc/Menu 可暂停、恢复并保存音量、视野角和镜头速度。将镜头速度调到 50% 与 200%，分别试转鼠标和右摇杆，重启后确认设置保留。
- 暂停菜单切换全屏/窗口后重启游戏，确认显示模式被保存。
- 暂停菜单切换 60 FPS 限制与垂直同步后重启游戏，确认模式保留；观察 60 FPS 模式是否撕裂，以及垂直同步模式是否跟随显示器刷新率。
- 使用手柄 `Menu` 打开暂停菜单；用十字键选择并调整音量、视野角与显示模式，`A` 确认，`B` 返回；新档按钮必须先进入二次确认。
- 靠近可互动对象时，显示 `[E / A]` 的名称与操作提示；港口与浅滩路标可读取航线或地点线索。
- 在港口观察艾琳和乔恩：距角色约 2–11 米且可见时应显示姓名与身份牌，靠近后由交互提示接替；640×360 下姓名牌应避开顶部 HUD、罗盘和中央角色，房屋遮住 NPC 时不得隔墙显示。与两人交互时显示角色名和上下文对白；对白打开时姓名牌隐藏、角色不能移动或驾驶，且第一次按交互键不会跳过首句。640×360 下长对白不得与任务 HUD、罗盘和底部操作说明重叠，关闭后这些指引应恢复。
- 顶部罗盘显示 N/E/S/W，金色菱形应随任务目标和角色朝向实时移动；目标在身后时应出现“左后方/右后方”，场景内不应常驻精确目标箭头。
- 按 `M` 或手柄 `View` 打开海图；海图应显示两座岛、航线、玩家、船只和当前目标，且不会被 HUD 遮挡。按 `Esc`、手柄 `B` 或点击“关闭”应返回游戏。
- 按 `J` 或手柄 `Y` 打开日志；三条任务显示各自状态与下一步线索。用 `Tab` / 手柄 `RB` 切换到探索笔记，已找到的发现物显示名称和短线索，未找到的只显示区域。打开时角色、船只和环境音应暂停；按 `J`、`Y`、`Esc` 或手柄 `B` 关闭。

## 主任务与探索

1. 到告示牌接取棱镜委托；日志、HUD 和罗盘目标随任务阶段更新。
2. 在晨湾港码头靠近“登船”交互点上晨鹱号，驶向潮汐浅滩；航行 HUD 应显示当前速度和目标码头距离；远离码头时不得继续显示旧码头下船提示，低于 1.2 m/s 且进入靠岸范围时提示当前码头可下船。区域标题应从晨湾港切换为晨湾航道，再切换为潮汐浅滩。下船后确认落在实体码头上，并可从潮汐浅滩码头再次登船。
3. 上岸后找到码头旁的蓝纹石碑并读取顺序；依西→中→东点亮信标。故意错误触发一次，确认谜题重置并重新提示石碑线索。
4. 取得棱镜，返港交给艾琳；港口灯塔点亮，向乔恩领取勘测帆。
5. 重启 Play Mode，确认已完成的棱镜任务、灯塔状态、勘测帆和谜题进度仍存在。

## 后续任务

1. 向艾琳接取失落航图，从地面走到遗迹入口。先在石台前尝试拾取航图，确认未读线索时航图仍在；辨认旁边的遗迹残碑后，罗盘应改指航图，再拾取并返港交付。不应需要跳跃或穿墙。重启游戏后，线索状态和任务阶段应保持；交付后港口导航台应出现纸质航图并跨进程保留。
2. 在已取得勘测帆且航图任务完成后，向乔恩接取风暴透镜。
3. 在海图上比较晨湾港与潮汐浅滩的位置；从地面走到坍塌灯塔旁，先尝试取透镜，确认未校准时仍锁住。将风向标转向港口所在的北方，再取透镜交给乔恩；港口信号桅杆应亮起蓝灯并在重启后保持。确认风向标方向和任务提示跨进程恢复、每次靠岸提示随当前任务变化，出现试玩版主线完成对白，关闭对白后仍可自由探索，目标标记不再强迫返回灯塔。

## 记录项

- 记录每个流程的完成时间、卡住位置、误解的提示和任何 Console 异常。
- 记录港口到潮汐浅滩的航行时间，以及在 1080p 目标设备上的平均/最低帧率。
- 图形模式下可运行 `-verify-performance -screen-fullscreen 1 -screen-width 1920 -screen-height 1080`，分别记录晨湾港、潮汐浅滩静态视角和自动航行视角的平均、1% 低帧率、最慢帧和 p95 帧时；仍需在目标配置与真人操控航行中复测。
- 要记录每帧 GC 分配，请用 `Builds/Windows/FoglightIslesDemo.exe` 开发构建运行同一基准。日志仅在 4 KB 已知分配校准成功后输出 `GC allocated in frame`；此值包含开发构建和性能计数器开销，发布候选版不报告 GC 数值。
- 需要交互截图时，可用 `-interactive-review` 启动 Windows 候选包并按 `F12`；截图保存在 `%USERPROFILE%\AppData\LocalLow\Foglight Isles\Foglight Isles Demo\Screenshots`，日志记录完整路径。此模式使用独立进度与设置，不影响正常试玩存档。

## 试玩截图检查

在普通图形窗口运行以下命令；验证器用虚拟键盘按 F12，检查截图写入后自动退出。不要加 `-nographics`。

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -interactive-review -verify-playtest-screenshot -screen-fullscreen 0 -screen-width 800 -screen-height 600 -logFile .\screenshot-validation.log
```

日志出现 `[FoglightIsles Screenshot] PASS` 后，检查所列路径中的 PNG 文件可打开。验证标志优先使用 `.validation` 存档，不会读取试玩入口的 `.playtest.v2` 进度。

## 自动航程检查

Windows 开发构建可使用以下命令检查靠岸与返航路径，不会写入任务进度：

```powershell
& .\Builds\Windows\FoglightIslesDemo.exe -batchmode -nographics -verify-journey -logFile .\journey-validation.log
```

日志出现 `[FoglightIsles Journey] PASS` 表示船只控制算法可沿航线从晨湾港到潮汐浅滩并返航，两个下船点落在实体码头上，角色能通过两座码头的实际交互入口登船与再登船，且船只不能驶入两座岛岸。此检查通过代码输入控制船只，不验证真人键鼠或手柄操控手感。

## 自动步行路线检查

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-walk-route -logFile .\walk-route-validation.log
```

日志出现 `[FoglightIsles Walk Route] PASS` 表示角色碰撞体可从港口出生点步行到告示牌、航线路标和晨湾港码头，随后从潮汐浅滩码头走到石碑、信标、棱镜、透镜、航图入口并返回码头；各检查点落在实体地面，港口交互点可选中。此检查不验证实际键鼠/手柄输入、视觉可读性或玩家对路线的理解。

## 自动任务闭环检查

以下命令会从新档开始，验证信标错误顺序重置、正确解谜、棱镜/航图/风暴透镜三条任务、两名 NPC 交付及存档状态：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-quest-loop -logFile .\quest-loop-validation.log
```

日志出现 `[FoglightIsles Quest Loop] PASS` 表示任务状态机、三个任务物品在地面接近位置可被玩家交互选中、NPC 交付、勘测帆升级及通关后目标标记清除的回归通过。随后可另起进程执行 `-verify-quest-resume`，确认存档中的三条任务和船只升级在重启后恢复。验证模式只使用独立的原型存档，正常启动试玩版不会清档；完整步行路线仍需人工试玩确认。

信标部分进度要用两个**独立进程**依次验证，第一步清空验证专用进度并保存前两座，第二步检查读档后两根竖柱及东侧目标，再故意点错使其全灭并重新解谜：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-beacon-partial-prepare -logFile .\beacon-partial-prepare.log
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-beacon-partial-resume -logFile .\beacon-partial-resume.log
```

依次出现 `[FoglightIsles Beacon Partial Prepare] PASS` 和 `[FoglightIsles Beacon Partial Resume] PASS` 才算通过。此检查复用独立验证存档，不能与其他验证命令交错运行；运行完成后该验证存档的信标为 3/3，不影响玩家试玩进度。

## 可选探索发现检查

- 在晨湾港与潮汐浅滩各找两件发现物，留意贝壳旁的浅水洼、护符下的旧石台能否帮助辨认位置。集齐 4/4 后检查 HUD 提示和探索笔记，返港分别与艾琳、乔恩交谈，确认他们对四件旧物有回应；记录是否能凭环境和 NPC 传闻自然找到物品。

```powershell
& .\Builds\Windows\FoglightIslesDemo.exe -batchmode -nographics -verify-discoveries -logFile .\discoveries-validation.log
```

日志出现 `[FoglightIsles Discoveries] PASS` 表示四个可选发现物的拾取、HUD 计数和持久化状态均正常。
随后另起进程使用 `-verify-discoveries-resume`，日志出现 `[FoglightIsles Discoveries Resume] PASS` 才能确认四条探索笔记在重启后恢复。

## 新档重载检查

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-new-save -logFile .\new-save-validation.log
```

日志出现 `[FoglightIsles New Save] PASS` 表示实际调用暂停菜单的新档操作后，任务、船只升级、谜题、探索发现和未读对白均已清除，音量设置仍保留。此模式使用独立验证存档。

## 试玩入口续档检查

`PLAYTEST_START.cmd` 使用独立试玩进度。解压最新 ZIP 后双击，接取港口告示牌任务并退出；再次双击，确认标题可继续、任务仍为已接取，且 `playtest.log` 中显示 `.playtest.v2`。主动选择“新档试玩”时才应清除这份进度。该实体输入与真实入口检查仍需试玩者执行。

自动回归可先在隔离验证存档依次运行 `-verify-quest-loop` 和 `-verify-discoveries`，再另起进程运行 `-interactive-review -verify-keyboard-free-roam-resume`；组合参数优先使用 `.validation` 存档，可检查试玩参数不会在启动时清档，但不能替代上述 `.playtest.v2` 真实入口验收。

## 显示模式持久化检查

按顺序运行两个独立进程，第二轮会读取第一轮保存的设置。测试使用独立验证存档，完成后清理验证用显示设置。

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-display-mode -logFile .\display-validation.log
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -verify-display-resume -logFile .\display-resume-validation.log
```

分别出现 `[FoglightIsles Display] PASS` 与 `[FoglightIsles Display Resume] PASS`，表示全屏/窗口切换、原窗口尺寸和重启后的全屏恢复均通过。实际鼠标和手柄操作仍需试玩。

## 虚拟手柄镜头输入检查

该检查在图形模式中添加 Unity Input System 虚拟手柄，鼠标未锁定时推动右摇杆，确认角色镜头实际转动；实体手柄的手感仍需人工试玩。

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-gamepad-look -logFile .\gamepad-look-validation.log
```

日志出现 `[FoglightIsles Gamepad Look] PASS` 表示虚拟右摇杆到角色镜头的运行路径通过。

## 虚拟手柄暂停菜单检查

在普通图形窗口运行下列命令，以虚拟手柄 A 进入游戏、Menu 暂停、十字键与 A 调整帧率设置、B 返回：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-gamepad-menu -logFile .\gamepad-menu.log
```

日志出现 `[FoglightIsles Gamepad Menu] PASS` 表示手柄菜单绑定、设置保存与暂停恢复路径通过；实体手柄和实际垂直同步观感仍须人工检查。

## 虚拟手柄出航检查

在普通图形窗口运行以下命令，使用独立验证存档，以虚拟手柄 A 和左摇杆从新档标题进入、接任务、登船、驶向浅滩、靠岸下船、再登船返港并再次下船：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-gamepad-voyage -logFile .\gamepad-voyage.log
```

日志出现 `[FoglightIsles Gamepad Voyage] PASS` 表示游戏内手柄绑定的双向航程与两个实体码头下船路径通过；仍须用实体手柄验证摇杆死区、镜头、航行手感和真人靠岸理解。

## 虚拟键盘旅程检查

该检查使用独立验证存档，启动时清除验证用任务进度并重载场景，然后以虚拟 Enter、W/S/A/D、E 走过标题界面、告示牌、码头登船、跨海操船、减速靠岸、E 下船、读取石碑、依序点亮三座信标并拾取棱镜。仍需真人操作手感及线索理解度验收。

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-journey -logFile .\keyboard-journey-validation.log
```

日志出现 `[FoglightIsles Keyboard Journey] PASS` 表示上述游戏键盘绑定的运行路径通过。

再另起进程运行存档恢复检查：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-keyboard-island-resume -logFile .\keyboard-island-resume.log
```

日志出现 `[FoglightIsles Keyboard Island Resume] PASS`，表示三信标与棱镜拾取后的任务阶段在重启后恢复。

如需审阅该路径的实际玩家视角，在普通图形窗口运行（不要加 `-batchmode`）：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-journey -capture-keyboard-journey -logFile .\keyboard-journey-graphics.log
```

告示牌、登船、航行、航道中段、到港、上岸、石碑和棱镜截图写入 `Builds/ReleaseCandidate/review-keyboard-*.png`，不会进入分发包。

完整首任务键盘闭环使用独立验证存档：拾取棱镜后步行返回浅滩码头、再登船、键盘掉头返航、靠泊晨湾港、步行到艾琳交付并点亮灯塔，再关闭对白、步行至乔恩领取勘测帆。此命令会清除上面部分流程使用的验证进度：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-first-quest -capture-keyboard-journey -logFile .\keyboard-first-quest.log
```

日志出现 `[FoglightIsles Keyboard First Quest] PASS` 后，在独立进程检查持久化状态：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-keyboard-first-quest-resume -logFile .\keyboard-first-quest-resume.log
```

日志出现 `[FoglightIsles Keyboard First Quest Resume] PASS`，表示交付后的任务、灯塔、谜题进度和勘测帆跨启动恢复，且船只最高航速为 13.5、转向速度为 60。返港、交付和升级抓帧为 `review-keyboard-return.png`、`review-keyboard-hand-in.png`、`review-keyboard-upgrade.png`。

## 失落航图键盘闭环

先运行上方 `-verify-keyboard-first-quest` 生成首任务完成且勘测帆已安装的独立验证存档，再运行：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-chart-quest -capture-keyboard-chart -capture-small-review -logFile .\keyboard-chart-quest.log
```

日志出现 `[FoglightIsles Keyboard Chart] PASS`，表示通过实际键盘绑定接取航图、驾驶升级船往返浅滩、从遗迹地面拾取航图并交给艾琳。独立进程检查任务与港口展示恢复：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-keyboard-chart-resume -logFile .\keyboard-chart-resume.log
```

日志出现 `[FoglightIsles Keyboard Chart Resume] PASS`，表示完成状态、港口导航台航图和勘测帆跨启动恢复。抓帧写入 `Builds/ReleaseCandidate/review-keyboard-chart-*.png`，不会进入分发包。虚拟输入不代替真人路线理解、实体键鼠或手柄驾驶验收。

## 风暴透镜与主线完成键盘闭环

先依次运行上方 `-verify-keyboard-first-quest` 与 `-verify-keyboard-chart-quest`，形成两条任务已完成的独立验证存档，然后运行：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-storm-quest -capture-keyboard-storm -logFile .\keyboard-storm-quest.log
```

日志出现 `[FoglightIsles Keyboard Storm] PASS`，表示通过游戏键盘绑定从乔恩接任务、往返浅滩、拾取透镜、点亮信号灯、推进完成对白并恢复自由步行。独立进程检查存档：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-keyboard-storm-resume -logFile .\keyboard-storm-resume.log
```

日志出现 `[FoglightIsles Keyboard Storm Resume] PASS`，表示任务完成、信号灯亮起和无强制目标状态跨启动恢复。抓帧写入 `Builds/ReleaseCandidate/review-keyboard-storm-*.png`，不会进入分发包。仍需真人检验任务理解、实体键鼠与手柄驾驶。

## 通关后自由探索键盘闭环

先依次运行三条主线的键盘验证命令，形成主线完成的独立验证存档。以下命令只清除该验证存档中的探索发现物进度，随后以键盘步行和航行收集四件发现物、返港打开探索笔记与海图：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -screen-fullscreen 0 -screen-width 800 -screen-height 600 -verify-keyboard-free-roam -capture-keyboard-free-roam -logFile .\keyboard-free-roam.log
```

日志出现 `[FoglightIsles Keyboard Free Roam] PASS`，表示四件发现物的地面到达和 E 拾取、往返航行、J/Tab 探索笔记、M 海图、返港步行至艾琳和乔恩并按 E 触发两段集齐回应均通过。抓帧含两名 NPC 的对白；若需确切 640×360 截图，在同一命令加入 `-capture-small-review`，验证器会在保存的显示模式应用后重新切到小窗口。再运行：

```powershell
& .\Builds\ReleaseCandidate\FoglightIslesDemo.exe -batchmode -nographics -verify-keyboard-free-roam-resume -logFile .\keyboard-free-roam-resume.log
```

日志出现 `[FoglightIsles Keyboard Free Roam Resume] PASS`，表示四条笔记跨启动恢复、世界物件保持隐藏且主线仍完成。`Builds/ReleaseCandidate/review-keyboard-roam-*.png` 用于审阅四件物品、探索笔记和无强制目标的海图，不进入分发包。此自动检查仍需真人验证路线可读性与实体操作手感。
