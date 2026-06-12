using System.Windows.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LegionFanControl.Models;
using LegionFanControl.Services;

namespace LegionFanControl.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly WmiService _wmi;
    private readonly HardwareMonitorService _hwMonitor;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private int _cpuFanRpm;
    [ObservableProperty] private int _gpuFanRpm;
    [ObservableProperty] private int _cpuFanPercent;
    [ObservableProperty] private int _gpuFanPercent;
    [ObservableProperty] private double _cpuTemp;
    [ObservableProperty] private double _gpuTemp;
    [ObservableProperty] private ThermalMode _currentThermalMode = ThermalMode.Balanced;
    [ObservableProperty] private bool _isWmiAvailable;
    [ObservableProperty] private string _statusMessage = "Initializing...";

    public MainViewModel()
    {
        _wmi = new WmiService();
        _hwMonitor = new HardwareMonitorService();

        IsWmiAvailable = _wmi.IsAvailable;
        StatusMessage = _wmi.IsAvailable ? "Connected" : "WMI unavailable — run as Administrator";

        if (_wmi.IsAvailable)
            CurrentThermalMode = _wmi.GetThermalMode();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();

        Refresh();
    }

    private void Refresh()
    {
        var fans = _wmi.GetFanData();
        CpuFanRpm = fans.CpuFanRpm;
        GpuFanRpm = fans.GpuFanRpm;
        CpuFanPercent = fans.CpuFanPercent;
        GpuFanPercent = fans.GpuFanPercent;

        var temps = _hwMonitor.GetTemperatures();
        CpuTemp = temps.CpuTemp;
        GpuTemp = temps.GpuTemp;
    }

    [RelayCommand]
    private void SetThermalMode(ThermalMode mode)
    {
        if (_wmi.SetThermalMode(mode))
        {
            CurrentThermalMode = mode;
            StatusMessage = $"Thermal mode: {mode}";
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _wmi.Dispose();
        _hwMonitor.Dispose();
    }
}
