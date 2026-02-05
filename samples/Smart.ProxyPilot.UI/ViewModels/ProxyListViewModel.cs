using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Smart.ProxyPilot.UI.Models;

namespace Smart.ProxyPilot.UI.ViewModels;

/// <summary>
/// 单个代理项 ViewModel
/// </summary>
public partial class ProxyItemViewModel(ProxyInfo proxy) : ViewModelBase
{
    public ProxyInfo Proxy { get; } = proxy;

    public string Address => Proxy.Address;
    public string Host => Proxy.Host;
    public int Port => Proxy.Port;
    public string ProxyType => Proxy.ProxyType;
    public ProxyState State => Proxy.State;

    public string ResponseTimeText => Proxy.Statistics.AvgResponseTime > 0
        ? $"{Proxy.Statistics.AvgResponseTime:F0}ms"
        : "--";

    public string StateText => State switch
    {
        ProxyState.Available => "可用",
        ProxyState.Validating => "验证中",
        ProxyState.Cooldown => "冷却中",
        ProxyState.Disabled => "已禁用",
        _ => "未知"
    };
}

/// <summary>
/// 代理列表 ViewModel
/// </summary>
public partial class ProxyListViewModel : ViewModelBase
{
    private const int PageSize = 20;

    [ObservableProperty]
    private ProxyItemViewModel? _selectedProxy;

    [ObservableProperty]
    private ProxyState? _filterState;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    public ObservableCollection<ProxyItemViewModel> Proxies { get; } = [];

    [RelayCommand]
    private void OnNextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            RefreshList();
        }
    }

    [RelayCommand]
    private void OnPrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            RefreshList();
        }
    }

    [RelayCommand]
    private void OnRefresh()
    {
        RefreshList();
    }

    partial void OnSearchTextChanged(string value)
    {
        CurrentPage = 1;
        RefreshList();
    }

    partial void OnFilterStateChanged(ProxyState? value)
    {
        CurrentPage = 1;
        RefreshList();
    }

    private void RefreshList()
    {
        // TODO: 实现真实的筛选和分页逻辑
    }

    /// <summary>
    /// 加载模拟数据
    /// </summary>
    public void LoadMockData()
    {
        Proxies.Clear();

        var mockProxies = new[]
        {
            new ProxyInfo { Host = "192.168.1.1", Port = 8080, State = ProxyState.Available, Statistics = new() { AvgResponseTime = 45 } },
            new ProxyInfo { Host = "192.168.1.2", Port = 8080, State = ProxyState.Validating },
            new ProxyInfo { Host = "192.168.1.3", Port = 3128, State = ProxyState.Cooldown },
            new ProxyInfo { Host = "192.168.1.4", Port = 8080, State = ProxyState.Disabled },
            new ProxyInfo { Host = "10.0.0.1", Port = 8888, State = ProxyState.Available, Statistics = new() { AvgResponseTime = 120 } },
            new ProxyInfo { Host = "10.0.0.2", Port = 8080, State = ProxyState.Available, Statistics = new() { AvgResponseTime = 89 } },
            new ProxyInfo { Host = "172.16.0.1", Port = 3128, State = ProxyState.Cooldown },
            new ProxyInfo { Host = "172.16.0.2", Port = 8080, State = ProxyState.Available, Statistics = new() { AvgResponseTime = 200 } },
        };

        foreach (var proxy in mockProxies)
        {
            Proxies.Add(new ProxyItemViewModel(proxy));
        }

        TotalPages = 1;
    }
}
