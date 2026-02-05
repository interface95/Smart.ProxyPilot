using CommunityToolkit.Mvvm.ComponentModel;

namespace Smart.ProxyPilot.UI.ViewModels;

/// <summary>
/// 池状态概览 ViewModel
/// </summary>
public partial class PoolStatusViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _availableCount;

    [ObservableProperty]
    private int _validatingCount;

    [ObservableProperty]
    private int _cooldownCount;

    [ObservableProperty]
    private int _disabledCount;

    [ObservableProperty]
    private double _overallSuccessRate;

    [ObservableProperty]
    private double _avgResponseTime;

    public double AvailablePercent => TotalCount > 0 ? (double)AvailableCount / TotalCount * 100 : 0;
    public double ValidatingPercent => TotalCount > 0 ? (double)ValidatingCount / TotalCount * 100 : 0;
    public double CooldownPercent => TotalCount > 0 ? (double)CooldownCount / TotalCount * 100 : 0;
    public double DisabledPercent => TotalCount > 0 ? (double)DisabledCount / TotalCount * 100 : 0;

    partial void OnTotalCountChanged(int value) => NotifyPercentagesChanged();
    partial void OnAvailableCountChanged(int value) => NotifyPercentagesChanged();
    partial void OnValidatingCountChanged(int value) => NotifyPercentagesChanged();
    partial void OnCooldownCountChanged(int value) => NotifyPercentagesChanged();
    partial void OnDisabledCountChanged(int value) => NotifyPercentagesChanged();

    private void NotifyPercentagesChanged()
    {
        OnPropertyChanged(nameof(AvailablePercent));
        OnPropertyChanged(nameof(ValidatingPercent));
        OnPropertyChanged(nameof(CooldownPercent));
        OnPropertyChanged(nameof(DisabledPercent));
    }

    /// <summary>
    /// 加载模拟数据（后续替换为真实数据）
    /// </summary>
    public void LoadMockData()
    {
        TotalCount = 156;
        AvailableCount = 42;
        ValidatingCount = 8;
        CooldownCount = 12;
        DisabledCount = 5;
        OverallSuccessRate = 87.5;
        AvgResponseTime = 156;
    }
}
