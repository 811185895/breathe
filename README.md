# Breathe

Windows 桌面呼吸提醒小组件：以透明悬浮窗的方式，用轻柔的呼吸动画提醒你注意节奏、放松身体，尽量不打扰当前工作。

技术实现与英文说明见 [`win/README.md`](win/README.md)。

## 使用前：本机要装什么

这是 **Windows 上的 .NET 8 WPF 程序**，不是「双击一个 exe 就能从网盘跑」的单文件包（当前仓库以源码方式开源）。在你自己的电脑上需要先安装：

1. **Windows 10/11**（桌面环境）。
2. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**（含 `dotnet` 命令）。  
   - 安装完成后在 PowerShell 里执行 `dotnet --version`，应看到 `8.x.x`。  
   - 若只装了「运行时」而没有 SDK，可能无法按下面方式 `dotnet run`，请改装 SDK 或自行发布 exe。

以上就绪后，无需再单独安装其他依赖即可编译运行本仓库中的工程。

## 获取代码并运行

在 PowerShell（或终端）中：

```powershell
git clone https://github.com/811185895/breathe.git
cd breathe\win
dotnet run --project BreatheWidget.App\BreatheWidget.App.csproj
```

首次运行会从源码编译，稍等片刻；成功后任务栏托盘会出现图标，桌面上会出现可点击穿透的呼吸动画层。

### 托盘菜单能做什么

右键托盘图标可以：

- 显示 / 隐藏 / 弱化（`Visible`、`Subtle`）
- 切换色调模式
- 调整位置（如居中、下三分之一、黄金分割点附近等）
- 退出（`Exit`）

更细的交互说明见 [`win/README.md`](win/README.md)。

## 构建与测试

```powershell
cd breathe\win
dotnet build BreatheWidget.sln
dotnet run --project BreatheWidget.Tests\BreatheWidget.Tests.csproj
```

## 使用效果（截图）

以下为实际运行时的界面参考（图片在仓库 `docs/使用效果/` 目录中，公开后以下相对路径在 GitHub 上会正常显示）：

![桌面效果](docs/使用效果/桌面效果.png)

![支持自定义位置和颜色](docs/使用效果/支持自定义位置和颜色.png)

## 开源与公开仓库前的提醒

公开 GitHub 仓库前，请确认代码与提交历史中 **没有** API Key、Token、密码、内网地址、数据库连接串或个人专属路径。若历史上曾提交过密钥，应轮换密钥并视情况清理 Git 历史（仅删文件不够）。

本仓库默认建议的公开顺序：**敏感信息检查 → 提交 `LICENSE` 与 `README` → 再在 GitHub 上将仓库设为 Public**。说明摘要见 [`docs/开源/开源操作方案--ChatGPT.md`](docs/开源/开源操作方案--ChatGPT.md)。

## License

[MIT](LICENSE)
