---
name: oss-github-open-source-checklist
description: >-
  Guides a small GitHub repo through a safe public release: secret scan, MIT
  LICENSE, README for end users vs developers, Windows self-contained zip on
  Releases via Actions, and README demo video without duplicate embeds. Use
  when the user wants to open source a project, make a repository public, add
  LICENSE or Releases, publish a portable Windows build, or fix GitHub README
  video display.
disable-model-invocation: true
---

# GitHub 小项目开源检查清单（用户向）

面向「要把自己的仓库公开给别人用」的维护者：步骤短、可照做；不展开大段理论。

## 建议顺序（不要先点 Public）

1. **扫敏感信息**：在源码与文档里搜 `api_key`、`secret`、`token`、`password`、`connectionString`、`Bearer `、`ghp_`、`AKIA`、`-----BEGIN`；检查是否误提交 `.env`、密钥、内网地址、个人绝对路径；若历史提交里有过密钥，应轮换密钥并评估是否清历史（仅删文件不够）。
2. **加 `LICENSE`**：个人小工具常用 **MIT**；仓库页 Add file → LICENSE → 选模板，或本地写好再推送。
3. **写好根 `README.md`**：先写「下载/怎么用」，再写「开发者怎么编译」；公开后再改回私有也可能已被 fork，所以顺序别反。
4. **最后再改可见性**：`Settings` → `General` 底部 `Danger zone` → `Change repository visibility` → `Make public`，按提示输入仓库名确认。

## README 里建议写清什么

- **给谁用**：一句话说明产品做什么。
- **初衷 / 能力**（可选但推荐）：解决什么问题、有哪些主要能力（例如氛围层 + 本地推断 + 轻提示），可链到仓库内更长文档，README 本身保持短。
- **免配置使用（若有构建物）**：指向 **Releases** 里自包含 zip；说明解压后运行哪个 exe、仅支持哪些架构（如 win-x64）。
- **开发者**：需要安装哪一版 **.NET SDK**、`clone` / `dotnet run` 或 `dotnet publish` 命令。
- **效果图**：截图用仓库相对路径即可。
- **演示视频（GitHub 特性）**：
  - **不要**在 README 里写 `<video src="docs/...">` 指向仓库内相对路径——渲染时会被去掉，看起来像「没有播放器」。
  - **只保留一种**能出视频卡片的方式：在 GitHub **网页编辑 README** 底部 **Attach files** 上传 mp4，把生成的那**一行** `https://github.com/user-attachments/assets/...` 单独放在正文里；**不要**再用第二个 Markdown 链接指向**同一个** mp4，否则会出现两个视频块。
  - 视频上方用**一两句**说明内容即可；技术原因、排错说明放在对话里或维护文档，不必写进 README。

## Windows 桌面（.NET WPF 等）发 Release 的常见做法

- **不要用 Linux runner 编 WPF**：WPF/WinForms 需在 **`windows-latest`** 上 `dotnet publish`。
- **自包含**：`dotnet publish -c Release -r win-x64 --self-contained true`，用户无需单独安装 .NET；产物是**一整文件夹**，打 zip 分发；说明里写清「勿只拷单个 exe」。
- **触发发版**：对 `v*` 标签（如 `v1.0.0`）`git push origin` 触发 workflow；用 `softprops/action-gh-release` 或 `gh release` 上传 zip（及可选固定文件名的演示 mp4，便于 README 用 `releases/latest/download/文件名` 做备用链）。

## 协议与文案

- **MIT**：Copyright 行写当年 + 你的名字或 GitHub 用户名即可。
- **README 语气**：面向访客与使用者；实现细节、排障、与助手之间的说明分开存放。

## 快速自检清单（复制使用）

```
- [ ] 已扫敏感信息与误提交路径
- [ ] 已添加 LICENSE（如 MIT）
- [ ] README：下载/使用 + 开发者 +（可选）效果图/单链视频
- [ ] 若有 CI Release：tag 推送后附件齐全
- [ ] 最后一步：仓库改为 Public
```
