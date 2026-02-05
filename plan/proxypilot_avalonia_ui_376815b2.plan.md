---
name: ProxyPilot Avalonia UI
overview: 为 Smart.ProxyPilot 代理池框架创建 Avalonia UI 监控界面，使用 Semi.Avalonia 主题，支持实时查看代理状态、统计信息和日志。
todos:
  - id: ui-setup
    content: 创建 Avalonia 项目和 Semi.Avalonia 主题配置
    status: pending
  - id: ui-models
    content: 实现 ViewModels（MainViewModel/PoolStatusViewModel/ProxyListViewModel）
    status: pending
  - id: ui-status
    content: 实现状态概览面板（PoolStatusView）
    status: pending
  - id: ui-list
    content: 实现代理列表视图（ProxyListView + 详情面板）
    status: pending
  - id: ui-log
    content: 实现实时日志视图（LogView）
    status: pending
  - id: ui-settings
    content: 实现设置面板（SettingsView）
    status: pending
  - id: ui-main
    content: 组装主窗口并集成代理池
    status: pending
isProject: false
---

# Smart.ProxyPilot Avalonia UI 界面

> 前置依赖：需要先完成 Smart.ProxyPilot 核心框架

## 项目结构

```
Smart.ProxyPilot/
└── samples/
    └── Smart.ProxyPilot.UI/                 # Avalonia UI 界面
        ├── Smart.ProxyPilot.UI.csproj
        ├── App.axaml                        # 应用入口（Semi.Avalonia 主题）
        ├── App.axaml.cs
        ├── Program.cs
        │
        ├── ViewModels/                      # MVVM ViewModels
        │   ├── ViewModelBase.cs
        │   ├── MainViewModel.cs             # 主窗口 ViewModel
        │   ├── ProxyListViewModel.cs        # 代理列表 ViewModel
        │   ├── PoolStatusViewModel.cs       # 池状态 ViewModel
        │   └── SettingsViewModel.cs         # 设置 ViewModel
        │
        ├── Views/                           # MVVM Views
        │   ├── MainWindow.axaml             # 主窗口
        │   ├── MainWindow.axaml.cs
        │   ├── ProxyListView.axaml          # 代理列表视图
        │   ├── PoolStatusView.axaml         # 池状态面板
        │   ├── LogView.axaml                # 实时日志视图
        │   └── SettingsView.axaml           # 设置面板
        │
        └── Converters/                      # 值转换器
            ├── ProxyStateToColorConverter.cs
            └── BoolToVisibilityConverter.cs
```

## UI 布局设计

```
┌─────────────────────────────────────────────────────────────────────┐
│  Smart.ProxyPilot - 智能代理池                              [─][□][×]│
├─────────────────────────────────────────────────────────────────────┤
│  [开始] [停止] [刷新] [设置]                                         │
├─────────────────────────────────────────────────────────────────────┤
│                          状态概览                                    │
│  ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐  │
│  │ 总数     │ │ 可用     │ │ 验证中   │ │ 冷却中   │ │ 已禁用   │  │
│  │   156    │ │    42    │ │    8     │ │   12     │ │    5     │  │
│  │          │ │  (27%)   │ │  (5%)    │ │  (8%)    │ │  (3%)    │  │
│  └──────────┘ └──────────┘ └──────────┘ └──────────┘ └──────────┘  │
├──────────────────────────────┬──────────────────────────────────────┤
│       代理列表               │          详细信息                     │
│  ┌────────────────────────┐  │  ┌────────────────────────────────┐  │
│  │ IP:Port     状态  延迟 │  │  │ 代理: 192.168.1.1:8080        │  │
│  │───────────────────────│  │  │ 类型: HTTP                     │  │
│  │ 192.168.1.1 ● 可用 45ms│  │  │ 状态: Available                │  │
│  │ 192.168.1.2 ● 验证 --  │  │  │                                │  │
│  │ 192.168.1.3 ● 冷却 --  │  │  │ [统计信息]                     │  │
│  │ 192.168.1.4 ○ 禁用 --  │  │  │ 验证: 成功 12 / 失败 2 / 超时 1│  │
│  │ ...                    │  │  │ 使用: 成功 45 / 失败 3         │  │
│  └────────────────────────┘  │  │ 响应: 45ms (avg) / 23ms (min)  │  │
│  [上一页] 第1页/共10页 [下一页]│  │ 连续成功: 8 / 连续失败: 0     │  │
│                              │  │                                │  │
│  筛选: [全部 ▼] 搜索: [____] │  │ 最后验证: 2026-02-05 10:23:45  │  │
│                              │  │ 最后使用: 2026-02-05 10:25:12  │  │
│                              │  └────────────────────────────────┘  │
├──────────────────────────────┴──────────────────────────────────────┤
│                          实时日志                                    │
│  ┌────────────────────────────────────────────────────────────────┐ │
│  │ 10:25:12 [验证] 192.168.1.5:8080 - Success (156ms)            │ │
│  │ 10:25:11 [状态] 192.168.1.3:8080 - Available -> Cooldown      │ │
│  │ 10:25:10 [获取] 192.168.1.1:8080 - 被消费者获取               │ │
│  │ 10:25:09 [验证] 192.168.1.6:8080 - Timeout                    │ │
│  └────────────────────────────────────────────────────────────────┘ │
│  [清空日志] [自动滚动 ✓] [显示: 全部 ▼]                              │
└─────────────────────────────────────────────────────────────────────┘
```

## 技术栈

- **UI 框架**: Avalonia UI 11.x
- **主题**: Semi.Avalonia
- **MVVM**: CommunityToolkit.Mvvm
- **依赖注入**: Microsoft.Extensions.DependencyInjection

## ViewModel 设计

```csharp
// MainViewModel - 主窗口
public partial class MainViewModel : ViewModelBase
{
    private readonly IProxyPool _pool;

    [ObservableProperty] private bool _isRunning;

    public PoolStatusViewModel PoolStatus { get; }
    public ProxyListViewModel ProxyList { get; }
    public ObservableCollection<LogEntry> Logs { get; }

    [RelayCommand]
    private async Task StartAsync() { ... }

    [RelayCommand]
    private async Task StopAsync() { ... }
}

// PoolStatusViewModel - 状态概览
public partial class PoolStatusViewModel : ViewModelBase
{
    [ObservableProperty] private int _totalCount;
    [ObservableProperty] private int _availableCount;
    [ObservableProperty] private int _validatingCount;
    [ObservableProperty] private int _cooldownCount;
    [ObservableProperty] private int _disabledCount;
    [ObservableProperty] private double _overallSuccessRate;
    [ObservableProperty] private double _avgResponseTime;

    public double AvailablePercent => TotalCount > 0 ? (double)AvailableCount / TotalCount : 0;
}

// ProxyListViewModel - 代理列表
public partial class ProxyListViewModel : ViewModelBase
{
    [ObservableProperty] private ProxyItemViewModel? _selectedProxy;
    [ObservableProperty] private ProxyState? _filterState;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private int _currentPage = 1;
    [ObservableProperty] private int _totalPages;

    public ObservableCollection<ProxyItemViewModel> Proxies { get; }

    [RelayCommand] private void NextPage() { ... }
    [RelayCommand] private void PrevPage() { ... }
}

// ProxyItemViewModel - 单个代理项
public class ProxyItemViewModel : ViewModelBase
{
    public ProxyInfo Proxy { get; }
    public string Address => $"{Proxy.Host}:{Proxy.Port}";
    public ProxyState State => Proxy.State;
    public string ResponseTime => Proxy.Statistics.AvgResponseTime > 0
        ? $"{Proxy.Statistics.AvgResponseTime:F0}ms" : "--";
}
```

## App.axaml 主题配置

```xml
<Application
    x:Class="Smart.ProxyPilot.UI.App"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:semi="https://irihi.tech/semi"
    RequestedThemeVariant="Light">
  <Application.Styles>
    <semi:SemiTheme Locale="zh-CN"/>
  </Application.Styles>
</Application>
```

## 功能清单

- 状态概览卡片（实时更新）
- 代理列表 DataGrid（筛选/搜索/分页）
- 代理详情面板（选中时显示完整统计）
- 实时日志视图（自动滚动/筛选级别）
- 设置面板（验证 URL、超时、并发数等）
- 深色/浅色主题切换
