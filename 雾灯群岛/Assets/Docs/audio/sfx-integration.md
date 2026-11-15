# 《雾灯群岛》音效接入方案（sfx-integration）

> 状态：**纯方案文档，不修改任何现有 .cs**。
> 针对 `AmbientSoundscape.cs` 在未来接入采样音频（波次二）时的扩展路径，
> 以及 `PauseMenu.cs` 音量设置应如何接管分组。
>
> 现状底牌（阅读方案前先对齐）：
> - `AmbientSoundscape.cs`：Awake 里 AddComponent 一个 `AudioSource`，运行时生成
>   18 s 单声道 44.1 kHz 循环，`volume = .72`，`spatialBlend = 0`；
>   Update 里每帧用 `AudioListener.pause` 做标题/暂停/海图/日志的静音。
> - `PauseMenu.cs`：无 AudioMixer；音量用 `AudioListener.volume = _volume`
>   直接写全局监听器（PlayerPrefs 键 `foglightisles.settings.volume.v1`，默认 0.8）。
>   菜单现为 9 个选项（`PauseMenu.OptionCount`），音量滑杆是 index 1（`AdjustSelection` 第 121 行）。
> - `ProjectSettings/AudioManager.asset`：DSP buffer 1024，real voices 32，
>   virtual voices 512，Default Speaker Mode 2（立体声）。工程**没有 AudioMixer 资产**。
> - `PrototypeNewSaveValidator.cs` 两处断言 `AudioListener.volume == .42f`——
>   **改方案时必须评估对这条回归断言的影响**。

---

## 1. AudioMixer 分组设计

工程目前没有 Mixer。建议新增一条 `Assets/Audio/MainMixer.mixer`，结构如下：

```
Master
├── Music
├── Ambient
└── SFX
    └── UI          (SFX 的子组，方便 UI 音量独立微调，可选)
```

| 组 | 内容 | 子混音要点 |
| --- | --- | --- |
| **Master** | 总出口 | 暂停菜单"总音量"的最终落点（见 §2） |
| **Music** | 标题乐、港口/航行/浅滩乐段、任务完成、灯塔点亮、试玩结束 | 每个 Music AudioSource 走 Music 组；避免夹在 SFX 上，单独便于未来加 ducking |
| **Ambient** | 现 `AmbientSoundscape` 的程序化海风潮声、未来的环境层（船层/信标共鸣/雾） | 程序音与采样环境音**同组同活动**，接收同一组音量 |
| **SFX** | 脚步/船/交互/拾取/信标/灯塔/UI/对白 | 3D 声源在这一组；键鼠与手柄共用同一套 |

实现要点（写给未来的实现者，不动现有文件也能落地）：
- `AmbientSoundscape` 的 `AudioSource` 接入方式：
  `source.outputAudioMixerGroup = mixer.FindMatchingGroups("Ambient")[0]`。
- 新建 AudioMixer 时在 Master 上挂一个 `Duck Volume`（Duck by Music），
  用于"任务完成乐/灯塔点亮"压低 Ambient 2–4 dB，是氛围游戏最便宜的"仪式感"。
- 快照（Snapshot）最小集三个：
  1. `Gameplay` —— 日常（Ambient 全量，Music 视场景淡入）；
  2. `Menu` —— 标题/暂停（Ambient 淡出至 −8 dB 以下，Music 峰值）；
  3. `Dialogue` —— 对白时 SFX 场景音轻微避让。
  每条快照只调音量/低通参数，暂不上效果器。
- `AudioMixerSnapshot.TransitionTo(f)` 建议用 **0.5–1.0 s** 的过渡，
  替代现有"暂停时 `AudioListener.pause = true`"的瞬时静音
  （这一项与 `audio-plan.md` §2.3 同源）。

---

## 2. 暂停菜单音量设置如何接管分组

现状的痛点：`AudioListener.volume` 是**全局单值**——程序音写它、验证器读它，
一旦引入 Mixer，**Mixer 的分贝系统与 `AudioListener.volume` 的 0–1 线性系统**并存，
会出现"菜单里看不出谁生效"的问题。推荐的迁移路径：

### 2.1 键位与 UI 不变，落点移动

- `PlayerPrefs` 键沿用 `foglightisles.settings.volume.v1`（语义从"监听器音量"改为
  "Master 的归一化音量"），**不改键名**就保证老档位（含回归）自然沿用。
- 音量滑杆的 `AdjustSelection` 分支（index 1）从写 `AudioListener.volume` 改为：
  `mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(_volume, 0.0001f)) * 20f)`
  —— `MasterVolume` 是 Master 组暴露出的 `Volume` 参数（在 Mixer 界面右键该组
  Volume → Expose）。其他分组**不暴露**给 UI；单项设置等到有真实需求再加。

### 2.2 默认值与刻度

- `_volume` 默认 0.8 保持；因为淡向 dB 后 0.8 ≈ −1.9 dB，听感与现在的 0.8 基本一致
  （考虑到 `AmbientSoundscape` 的音量基线本身低，差距更小）。
- 滑杆显示仍用 `$"{_volume:P0}"`，不改 OnGUI 结构。

### 2.3 回归断言的处理

- `PrototypeNewSaveValidator` 两处断言 `AudioListener.volume == .42f`
  （`Mathf.Approximately`）。迁到 Mixer 后 `AudioListener.volume` 不再被菜单驱动
  （维护主音量的是 Master 的 dB 参数），**这条断言必须同步改为读
  `mixer.GetFloat("MasterVolume", out v)` 后比较 `v ≈ dB(0.42)`**，
  否则新档回归会误报。这是迁移时唯一动到验证文件的地方，实现日要一并提。
- 过渡期方案（若暂不迁移 Mixer）：保持写 `AudioListener.volume`，
  但把 Ambient 的 `AudioSource` 音量从 0.72 交给 Mixer 之外的独立变量控制，
  确保只有一个"总音量"来源。

### 2.4 静音语义分层（关键）

明确三个层次的"静音"，现有代码混在一起，接入采样后必须分开：

| 层次 | 谁控制 | 何时 |
| --- | --- | --- |
| **总音量** | 暂停菜单 Master 参数 | 用户设置 |
| **暂停静音** | `Menu` 快照（Ambient 淡出、Music 保持轻） | 标题/暂停/地图/日志打开 |
| **听者暂停** | `AudioListener.pause` | 仅"需要整机静默"的极端场景（如切后台/失焦）；建议最终**取消**主循环里每帧写 `AudioListener.pause = true` 的做法，改由快照接管 |

`StartScreen.IsOpen` 时单独一个更深的 `Title` 快照（可在 Menu 基础上把 SFX 也压低），
保证标题界面的音乐与游戏内没有"同一段声音在两种场景里两种命运"的错乱。

---

## 3. 程序化音与采样音的混合优先级

### 3.1 混音总线上的关系

- **程序音是"保底"**：采样素材接错、漏接或授权未落实时，程序化事件音（audio-plan §2.1 那批）始终可用；**采样音是上层替换，不是必需依赖**。
- 具体地说，同一事件（如"信标点亮"）实现为：
  ```
  SfxBus.PlayConfirm()   // 入口不变
   ├─ 若已有采样版本 → 播放采样（一次性 OneShot，走 SFX 组）
   └─ 否则           → 播放程序生成短音（同一入口的 fallback）
  ```
  入口统一在 SFX 组，混合电平与未来替换无耦合。
- `AmbientSoundscape` 的程序化环境音在**采样环境层接好后降级为垫底**：
  变成"采样海风潮声的静默兜底 + 低频厚度层"，音量随采样层存在与否自动对半
  （`Ambient` 组内做两个子 AudioSource 的相对电平即可，不需要动态
  增删 AudioSource）。

### 3.2 优先级数字（真实声源数上限）

`AudioManager.asset` real voices = 32。建议按"谁偶尔可见"分配预算：

| 层 | real voices 占用上限 | 说明 |
| --- | --- | --- |
| Music | 1–2（单双声道交错淡入） | 任何时刻 1 首曲目 |
| Ambient | 2–3（底层 + 船层 + 事件层） | 常驻 |
| 世界 SFX | ≤6 | 灯塔/信标/雾板感应点等常驻环境 |
| UI/事件 SFX | ≤8 | 一次操作只允许 1 个新 voice，防叠音 |
| **总实时** | ≤20 | 留 12 个余量给未知的偶然重音 |

音频源超过上限时 Unity 会自动虚拟化（`m_VirtualizeEffects: 1` 已开），
但**不要依赖虚拟化做混音**——超限是最先炸低优先级的，别让 UI 音被虚拟化掉。
实现时用 `AudioSource.priority`：Music=32、Ambient=64、世界环境=100、UI=128、
点击类=160。

### 3.3 空间策略

- 现在一切都是 2D（`spatialBlend = 0`）。采样接入后：
  - **Ambient/Music/UI**：保持 2D（`spatialBlend = 0`）。
  - **脚步/船/信标/灯塔/NPC 对白**：3D（`spatialBlend = 1`），并配
    `rolloffMode = Linear`、`maxDistance = 30–60`（浓雾里太大的可听半径没有意义，
    听见雾里传来的声音正好是氛围卖点，30 m 是"雾中远处有动静"的甜点值）。
  - `AudioListener` 当前挂在玩家相机上（`GameBootstrap` AddComponent AudioListener），
    位置随镜头收放，浓雾里总站镜头后 2.5 m——**不必改**；
    3D 音源的 audible 距离以 listener 为准已足够。

---

## 4. 内存与流式加载策略

### 4.1 现状与总预算

- 工程目前仅 `AmbientSoundscape` 运行时生成（无文件 IO；18 s × 44.1 kHz × 单声道 float ≈ 3.2 MB
  内存，常驻；若打包成 16-bit PCM 则约 1.6 MB）。
- Demo 的音频资产预算建议**总量 ≤ 60 MB 压缩后**、内存常驻 ≤ 25 MB：
  - Music：OGG 格式，每首 60–150 s，连续播放率 100% → **全流式（Streaming）**，
    内存占用仅环形 buffer（数百 KB/条）。
  - 环境（采样海风/雾鸣/信标共鸣）：OGG，循环用 → **Compressed In Memory**，
    每条 ≤8 s（长环境素材先剪成 ≤8 s 的可循环段，再在 Unity 内双源交叠）。
  - 事件 SFX：WAV → **Decompress On Load**，单条 0.05–3 s，总池 ≤200 条 × 平均 0.5 s
    PCM ≈ 8–10 MB，可接受。

### 4.2 加载体量与内嵌方式

- Demo 体量原则：**音频全部打进 Asset/Build，不做 Addressables**——
  垂直切片的音频池已知且小，Addressables 的复杂度会反噬。
  "Build 只引用用到的 AudioMixer + 直接放置的预加载 AudioSource"是最稳的路径。
- 音频文件命名规范（为未来审计与授权台账服务）：
  `Assets/Audio/{source}/{category}/{name}.{ogg|wav}`，
  `source` 记来源站名（freesound / kenney / sonniss…），**与授权台账一一对应**。
- 循环无缝：
  - 采样环境层接入后，把"18 s 程序循环"替换为"采样循环段"时，须保证
    采样段首尾为零交叉点（在 DAW 里处理），或在 Unity 内用双 AudioSource
    交叠拼接（A 播到 90% 时 B 提前淡入）——**双源交叠**是无需额外工具的稳妥做法。
  - `AudioSource.loop = true` 对 Compressed In Memory 的 OGG 可能在循环点出现微小隙音，
    WAV/PCM 无此问题；事件音不用担心，环境循环音在验收时必须连听三分钟。
- `AudioClip` 的导入设置：每个 AudioClip 用"Load Type + Preload"两个维度检查，
  Demo 期所有 SFX 设 **Preload = true**（避免第一次点击卡顿），
  Music 设 **Preload = false**（由首个场景搭载后台预载）。

### 4.3 `AmbientSoundscape` 的命运（未来时）

| 时期 | 程序音的状态 | 入口 |
| --- | --- | --- |
| 现在 | 唯一声源，主循环里 `AudioListener.pause` 决定静音 | `GameBootstrap.BuildSoundscape` |
| 波次二（采样接入） | 降为 Ambient 组内的垫底层（低频厚度 + 失效兜底），音量自动减半 | 同上，AudioSource 指向 Ambient 组 |
| 灰盒 → 风格化之后 | 若采样环境层稳定，**删除主循环对 `AudioListener.pause` 的每帧写入**，静音语义收进快照（§2.4） | 快照切换 |

**迁移原则**：`GameBootstrap.BuildSoundscape()` 的调用点不动、`AddComponent` 不动，
只让组件内部把 `outputAudioMixerGroup` 指向 Ambient ——
这样程序音与采样音的交接是**在组件内部完成**的，对调用方（Bootstrap/验证器）零影响。

---

## 5. 落地清单（将来实现时逐条打勾）

1. [ ] 新建 `Assets/Audio/MainMixer.mixer`（Master/Music/Ambient/SFX(/UI)），暴露 MasterVolume。
2. [ ] `AmbientSoundscape`（新副本或扩展组件，不回改现有文件）把 AudioSource
       接入 Ambient 组；新增程序化事件音组件（audio-plan §2.1 的那批事件）。
3. [ ] 键位沿用；暂停菜单滑杆改写 `mixer.SetFloat("MasterVolume", dB)`；
       同步修改 `PrototypeNewSaveValidator` 两处断言。
4. [ ] 取消主循环每帧 `AudioListener.pause = true`，由 `Menu`/`Title`/`Gameplay` 快照接管，
       过渡时长 0.5–1.0 s。
5. [ ] 采样 SFX 按授权台账逐条核实后导入，先 P0（脚步/撞岸/UI），
       后音乐（audio-plan §4 的优先级）。
6. [ ] 验收：暂停淡出不突兀；环境循环连听三分钟无隙音；
       `AudioListener.volume` 在新档回归中由**快照路径**验证通过。