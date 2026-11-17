# 0.2.12 探索笔记线索集成交接

- 基线：Windows Demo 0.2.12，构建编号 `20261117121236-DC4C7374DA61`，本地分发 ZIP SHA-256 `755A6F97017BB111EB1535A315E4596F5233CFDA4CE500184CF34E4CE751843B`。
- 来源：DeepSeek 经 Agent HUD 只读审查四件发现物，指出未发现态的探索笔记四次重复“留意发光物件”，离开 NPC 对话后缺乏可回看的方向。Codex 负责核对场景并集成。审查中“港务办公室不存在”与源码不符：`GameBootstrap.BuildHarbour()` 已创建该建筑，故保留办公室地标；“浅滩旧路”确无对应实体，已改为旧石台。
- 修改：`DiscoveryJournal.Entry` 加入未发现时的短线索，按港务办公室旁旧纸、修船屋门前铜光、浅滩西侧水洼回声、浅滩东南旧石台蓝光分别提示，不给坐标或常驻箭头。`QuestJournalPanel` 未发现态显示各自线索。`GameBootstrap` 给港口日志和罗盘加木箱/木座支撑，修改乔恩的护符传闻。试玩说明、反馈表和清单已同步。
- 验证：开发构建与候选构建 PASS；640×360 探索笔记截图中两条未发现线索和两条已发现笔记可读且无裁切；开发版 `-verify-keyboard-free-roam -capture-keyboard-free-roam -capture-small-review` 走完四件拾取、两端航行、返港 NPC 交谈 PASS。候选版 `-verify-new-save`、`-verify-discoveries`、独立进程 `-verify-discoveries-resume`、`-verify-quest-loop`、`-verify-gamepad-voyage` 均 PASS。ZIP 独立解压后发现物收集与跨进程恢复 PASS。
- 边界：自动测试只证明物件可达、存档和文本排版；玩家是否会打开探索笔记、是否能凭线索找到物品、是否觉得剧透，仍待真人观察。0.2.11 ZIP 保留，旧版反馈须注明构建编号。
