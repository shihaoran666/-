# 协作约定

- 以 `雾灯群岛/` 为唯一 Unity 工程源目录。不要提交 `Library/`、`Builds/`、`Logs/`、临时文件、账户信息或凭据。Unity 资产及其 `.meta` 文件必须成对保留。
- 开始前读取 `coordination/STATUS.md` 和 `雾灯群岛/AGENT_COORDINATION.md`，核对当前构建、任务负责人和验收缺口。文档中的旧结论以当前代码与实际测试为准。
- 每个 agent 使用独立分支，格式 `agent/<名称>/<任务>`；不要同时修改另一个 agent 正在负责的 Unity 文件。若不能建分支，先只读审查并在自己的交接文件中给出建议。
- 交接写入 `coordination/updates/<agent>-<日期>-<任务>.md`：目标、修改文件、验证命令与结果、未解决问题、需要谁接手。重要代码变更通过 Pull Request 交给集成者，不直接合并到 `main`。
- Codex 是当前主工程集成者，负责审查 diff、Unity 构建、回归与打包。其他 agent 可在独立分支或目录产出代码/素材候选，但不要并发启动同一 Unity 工程的构建。
- 发布、账号、收费与 Steam 后台操作由用户决定；商店文字只描述已实现且已验证的功能。
