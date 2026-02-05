using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart.ProxyPilot.UI.Models;

namespace Smart.ProxyPilot.UI.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private bool _autoScroll = true;

    [ObservableProperty]
    private LogLevel? _logFilter;

    public PoolStatusViewModel PoolStatus { get; } = new();
    public ProxyListViewModel ProxyList { get; } = new();
    public ObservableCollection<LogEntry> Logs { get; } = [];

    public MainViewModel()
    {
        // 加载模拟数据
        PoolStatus.LoadMockData();
        ProxyList.LoadMockData();
        LoadMockLogs();
    }

    [RelayCommand]
    private void OnStart()
    {
        if (IsRunning) return;

        IsRunning = true;
        AddLog("系统", "代理池已启动", LogLevel.Info);

        // TODO: 启动代理池
    }

    [RelayCommand]
    private void OnStop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        AddLog("系统", "代理池已停止", LogLevel.Info);

        // TODO: 停止代理池
    }

    [RelayCommand]
    private void OnRefresh()
    {
        PoolStatus.LoadMockData();
        ProxyList.LoadMockData();
        AddLog("系统", "数据已刷新", LogLevel.Info);
    }

    [RelayCommand]
    private void OnClearLogs()
    {
        Logs.Clear();
    }

    private void AddLog(string category, string message, LogLevel level)
    {
        Logs.Insert(0, new LogEntry
        {
            Category = category,
            Message = message,
            Level = level
        });

        // 限制日志数量
        while (Logs.Count > 500)
        {
            Logs.RemoveAt(Logs.Count - 1);
        }
    }

    private void LoadMockLogs()
    {
        AddLog("验证", "192.168.1.5:8080 - Success (156ms)", LogLevel.Info);
        AddLog("状态", "192.168.1.3:8080 - Available -> Cooldown", LogLevel.Warning);
        AddLog("获取", "192.168.1.1:8080 - 被消费者获取", LogLevel.Debug);
        AddLog("验证", "192.168.1.6:8080 - Timeout", LogLevel.Error);
    }
}
