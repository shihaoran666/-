# 首任务交付后灯塔扫掠光束候选（art-upgrade-plan A4）

- 分支：`agent/deepseek/lighthouse-beam`
- 基线：`main` @ `acb92bb`（Demo 0.2.5，构建编号 `20261116131325-E3D9BA2C9216`）
- 状态：候选实现，未运行 Unity 构建、未合入 main，等集成者审查与实机对照

## 1. 目标

对应 `Assets/Docs/art/art-upgrade-plan.md` 的 **A4 灯塔光束语言**：灯塔点亮时出现一支可从浅滩辨认的暖橙扫掠光束，扫掠周期 4–6 s，成为远处的地理定位点。

先核对了现有代码，确认三件事：

1. `GameBootstrap.BuildLighthouse`（`Assets/Scripts/GameBootstrap.cs`）在 `(10, 0, 18)` 建塔，灯头 `_lighthouseBeacon` 在 `y=8.2`，现有 `Lighthouse Light Column` 是一根**静态红色竖柱**（`(1, .08, 0)` 发光，`y=15`，高 12 m），没有任何水平扫掠。
2. 点亮状态由 `RefreshWorldForQuest` 的 `_lighthouseBeacon.SetActive(lighthouseRestored)` 驱动（`stage == PrismQuestStage.Complete`），`_lighthouseLight.enabled` 同步。**没有独立的灯塔存档位**，所以光束必须挂在这个开关下面，不能自建状态。
3. 夜间/雾里真正的可见性问题在距离：晨湾港 `z=12` 与潮汐浅滩 `z=-82` 相距约 94 m，点光源 `range=120` 但亮度随距离与雾衰减，浅滩方向实际读不到「灯亮了」。光柱要承担状态变化，而不是把灯的 range 调大。

## 2. 修改文件

| 文件 | 变更 |
| --- | --- |
| `雾灯群岛/Assets/Scripts/LighthouseBeam.cs` | 新增（约 30 行）：只做绕本地 Y 轴匀速旋转，周期可配置，默认 5 s |
| `雾灯群岛/Assets/Scripts/LighthouseBeam.cs.meta` | 新增，GUID `9c4f1a7de2b34c8f9e6d5a12b7c3e841`（全库无重复） |
| `雾灯群岛/Assets/Scripts/GameBootstrap.cs` | `BuildLighthouse` 内新增约 18 行：在 `_lighthouseBeacon` 下建 `Lighthouse Beam Pivot`，挂一根细长自发光圆柱作为光束；文件头新增两个常量 `LighthouseBeamLength = 58f`、`LighthouseBeamRadius = .42f` |

没有改：任务状态机、`PrismQuestStage` 枚举、存档键、`ProgressStore`、`RefreshWorldForQuest` 的既有分支、任何场景文件（场景仍是运行时程序化拼装）、现有红色竖柱。

## 3. 实现细节

- **挂载点**：`beamPivot` 是 `_lighthouseBeacon` 的子物体，`localPosition = 0`，即灯头中心 `(10, 8.2, 18)`。灯头 `SetActive(false)` 时光束一并消失，**暗灯/亮灯状态零新增逻辑**。
- **几何**：`Cylinder` 原始高度 2、半径 0.5，`localScale = (0.84, 29, 0.84)` → 长 58 m、半径 0.42 m；`localRotation = Euler(90,0,0)` 把轴从 Y 转到 +Z；`localPosition = (0,0,29)` 让光束从灯心向外延伸 0→58 m。
- **颜色/亮度**：材质沿用 `CreateMaterial`（`FoglightRuntimeMaterial` + Standard），颜色 `(1, .66, .3)`，`_EMISSION` 打开后把 `_EmissionColor` 覆写到 `color * 4.2`（默认 `CreateMaterial` 是 2.5 倍），保证穿雾后仍有明显橙白。
- **扫掠**：`LighthouseBeam.Update` 每帧 `transform.Rotate(0, 360/周期 * dt, 0, Space.Self)`，周期 5.0 s（72°/s，落在计划的 4–6 s 区间）。
- **碰撞**：`Destroy(beam.GetComponent<Collider>())`，与同函数里 flare/column 的既有写法一致。
- **零新增 Light**：没有加实时点光/聚光，避免 8 GB 目标机的额外开销；亮度全部来自 emission。

## 4. 可见性论证（浅滩能不能看见）

- 雾是内置指数平方：`density = .009`，94 m 处 `fogFactor = exp(-(0.009 × 94)²) ≈ 0.50`，即约一半的本体色能到达眼睛，剩下一半混向雾色 `(.34,.44,.49)`。暖橙 × 4.2 的 HDR 强度即使减半仍与雾色形成明显色相差。
- 角尺寸：0.42 m 半径在 90 m 处约 0.53°——单帧像细线，但**它是移动的**：5 s 一圈、末端线速度约 73 m/s，从浅滩看是一道扫过雾层的橙色掠影，这正是计划 A4 要求的「光柱掠过雾层」。
- 仰角：灯心 `y=8.2`、距离 90 m → 约 5.2° 仰角，第三人称相机稍微抬视角即可进入画面；贴近海面时可能在画面上边缘。
- 光束长度 58 m，末端约到 `z ≈ -40`，不覆盖整段航程，但从浅滩看「从港口方向扫过来」的方向感足够；如果实测偏短，调 `LighthouseBeamLength` 即可。

## 5. 风险与已知限制

1. **穿模**：光柱高度 8.2 m，高于港口所有房屋（Office 顶约 2.4 m、Shipwright 约 2.7 m）与信号桅灯（6.3 m），在港口上空扫过，预期不穿建筑；但会从塔身上方 8.2 m 掠过，近景可能看到光束「穿过」塔顶装饰球（flare 半径 1.25，中心 9.1 m）。**待集成者近景抓帧确认**，必要时把 pivot 抬高 0.3 m 或缩短光束。
2. **遮挡**：旋转光束每圈有一半朝向海面背面，从浅滩只能周期性看到；这是旋转光束的固有特性，不是 bug。若集成者希望「常亮可见」，可加一支低强度的辅助长柱（本次未做，避免加几何）。
3. **与现有红色竖柱并存**：红色竖柱 `(1,.08,0)` 静止在 `y=15`，与暖橙扫掠光束同屏会显得两套灯语；我没动它（避免越界改动），建议集成者统一为暖橙或调暗。
4. **硬边圆柱**：不透明实心几何在近景像一根发光棒，不是体积光。灰盒阶段可接受；若要柔化需换 additive/透明材质或自定义 shader，超出本次「最小可回退」范围。
5. **未验证项**：没有运行 Unity，因此编译、实际亮度、雾中观感、640×360 下是否仍可辨识，全部**待集成者实机确认**；代码只做了静态自检（命名空间、命名冲突、GUID 唯一、父子变换无跳变）。
6. **与罗盘 HUD 的间距（640×360 实测风险）**：基线图里灯头在 `y ≈ 172–178`，罗盘条占 `y ≈ 185–215`，两者只差约 10–13 px。光束水平扫掠时远端接近地平线，可能贴着罗盘条上沿掠过。这不算穿模，但会让「光束」和 HUD 条在视觉上粘住；建议集成者在 640×360 对照图里确认光束主体落在条上方的天空区，必要时把 `beamPivot` 抬高 0.2–0.4 m（不改长度/半径）。

## 6. 集成者验收步骤与需要的画面

1. 新建分支或直接 `git checkout agent/deepseek/lighthouse-beam` 后跑 Unity 构建；确认 `Assets/Scripts/LighthouseBeam.cs` 被编译进 `Assembly-CSharp`，Editor.log 无编译错误与 NullReference。
2. **对照图 A（暗灯）**：`-verify-quest-loop` 之前 / 新档未完成棱镜任务，把角色放到潮汐浅滩码头（约 `(8, 1.2, -70)` 一带）朝晨湾港方向（+Z）截图，确认看不到光束。
3. **对照图 B（亮灯）**：完成棱镜任务（或直接用已验证的完成态存档），**与基线 `Builds/ReleaseCandidate/review-lighthouse-lit.png` 同机位同朝向同分辨率**截图，按第 6b 节的判据确认能看到扫掠光束，并附上与基线图的并排对照。
4. **对照图 C（旋转）**：同一机位间隔约 1.25 s（四分之一圈）连拍两张，确认明显位移、方向一致、不抖动。
5. **近景 D**：站到港口内（例如 `(4, 0, 14)`）朝灯塔看 2–3 帧，检查是否穿模/遮挡塔顶、颜色是否与红色竖柱冲突。
6. 分辨率：640×360 与 1080p 各一组，确认小窗口下仍能辨认。
7. 回归：任务闭环、续档（完成态读档后光束仍在）、旧档（未完成态读档后光束不出现）、键鼠/手柄自动回归照常；HUD/罗盘无变化。

## 6b. 基线证据与「可辨认」判据（集成者提供，2026-11-17）

集成者用 0.2.5 跑：

```
-capture-review -capture-lighthouse-review -capture-small-review -screen-width 640 -screen-height 360
```

落盘 `雾灯群岛/Builds/ReleaseCandidate/review-lighthouse-lit.png`（完成态、灯已点亮、潮汐浅滩朝晨湾港）。这张图是本次候选光束的**可见性目标**：达不到它的对照读数就是不合格。

我对该图做了像素量测（640×360）：

- 塔身约 8–9 px 宽；灯头只是塔顶一个约 6 px 高的灰蓝色小凸起，RGB ≈ `(104, 103, 112)`；
- 在 `x 295–345 / y 120–205` 范围内按「暖色」判据（`R > 150` 且 `R > B + 25` 且 `G > 90`）搜索：**命中 0 个像素**——完成态灯塔在这张 640×360 图里没有任何「亮着」的读数；
- 罗盘 HUD 条覆盖 `y ≈ 185–215`，塔基与港口水面大半被它挡住；灯头露在条上方约 10 px；
- 全图最亮的暖色点其实是水面上的 NPC/N 标记装饰，不是灯塔。

结论：在浅滩这个机位，**「灯已点亮」这个状态变化只能由水平光束承担**，点光与灯头本身在 640×360 下不可读——这条基线正好是 A4 的必要性证据。

### 验收判据（建议按这条判，不按感觉）

1. 亮灯帧同机位同分辨率，出现一条**连续暖色像素带**：`R − B ≥ 60` 且 `R ≥ 170`；该带与暗灯同帧差分面积 ≥ 120 px；
2. 该带最长连续长度 ≥ 12 px（0.42 m 半径在 5–10 px/m 的屏幕比例下大致就是 3–8 px 粗，长度靠 58 m 的透视展开）；
3. 间隔 1.25 s 的两帧，光束远端像素位置位移 ≥ 15 px，方向一致、不抖；
4. 暗灯同机位按同一判据命中 0 px；
5. 若把这套判据放到 1680×1050 上仍读不出光束，判不合格（小窗口是下限，不是唯一目标）。

第 1–4 条是我从上面那张基线图反推的**建议阈值**，集成者可自行调整；但「暗灯 0 命中、亮灯成带、两帧位移」这三条建议保留，否则无法区分「光束」与「水面反光」。

## 7. 回退

单次提交，独立于其他改动：`git revert <commit>` 或直接删除 `LighthouseBeam.cs(.meta)` 并还原 `GameBootstrap.cs` 两处（常量块 + `BuildLighthouse` 内 18 行）。不涉及场景、预制体、存档，回退后行为与 0.2.5 完全一致。

## 8. 可调参数一览（集成者按实测改这里即可）

| 参数 | 位置 | 当前值 | 作用 |
| --- | --- | --- | --- |
| `LighthouseBeam.Length` | `GameBootstrap.LighthouseBeamLength` | 58 m | 光束长度；更远会更长但更早穿出视野 |
| `Radius` | `GameBootstrap.LighthouseBeamRadius` | 0.42 m | 粗细；太大像光棒，太小 90 m 外看不见 |
| 发光强度 | `beamMaterial.SetColor("_EmissionColor", …*4.2f)` | 4.2× | 穿雾亮度 |
| 扫掠周期 | `LighthouseBeam.DefaultSweepSeconds` | 5 s | 计划要求 4–6 s |
