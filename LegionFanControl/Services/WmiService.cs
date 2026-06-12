using System.Management;
using LegionFanControl.Models;

namespace LegionFanControl.Services;

public class WmiService : IDisposable
{
    private const string WmiNamespace = @"root\WMI";
    private ManagementObject? _fanMethod;
    private ManagementObject? _gameZoneData;
    private bool _disposed;

    public bool IsAvailable { get; private set; }

    public WmiService()
    {
        TryInitialize();
    }

    private void TryInitialize()
    {
        try
        {
            _fanMethod = new ManagementObject(
                new ManagementScope(WmiNamespace),
                new ManagementPath("LENOVO_FAN_METHOD.InstanceName='ACPI\\PNP0C14\\0_0'"),
                null);
            _fanMethod.Get();

            _gameZoneData = new ManagementObject(
                new ManagementScope(WmiNamespace),
                new ManagementPath("LENOVO_GAMEZONE_DATA.InstanceName='ACPI\\PNP0C14\\0_0'"),
                null);
            _gameZoneData.Get();

            IsAvailable = true;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    public FanData GetFanData()
    {
        if (!IsAvailable) return GetMockFanData();

        try
        {
            var result = new FanData();

            var cpuFanResult = _fanMethod!.InvokeMethod("Fan_GetCurrentFanSpeed", new object[] { 0 });
            var gpuFanResult = _fanMethod!.InvokeMethod("Fan_GetCurrentFanSpeed", new object[] { 1 });

            result.CpuFanRpm = Convert.ToInt32(cpuFanResult);
            result.GpuFanRpm = Convert.ToInt32(gpuFanResult);

            var cpuMaxResult = _fanMethod!.InvokeMethod("Fan_GetMaxFanSpeed", new object[] { 0 });
            var gpuMaxResult = _fanMethod!.InvokeMethod("Fan_GetMaxFanSpeed", new object[] { 1 });

            int cpuMax = Convert.ToInt32(cpuMaxResult);
            int gpuMax = Convert.ToInt32(gpuMaxResult);

            result.CpuFanPercent = cpuMax > 0 ? (int)((double)result.CpuFanRpm / cpuMax * 100) : 0;
            result.GpuFanPercent = gpuMax > 0 ? (int)((double)result.GpuFanRpm / gpuMax * 100) : 0;

            return result;
        }
        catch
        {
            return GetMockFanData();
        }
    }

    public TemperatureData GetTemperatureData()
    {
        if (!IsAvailable) return GetMockTempData();

        try
        {
            var cpuTemp = _gameZoneData!.InvokeMethod("GetCPUTemp", null);
            var gpuTemp = _gameZoneData!.InvokeMethod("GetGPUTemp", null);

            return new TemperatureData
            {
                CpuTemp = Convert.ToDouble(cpuTemp),
                GpuTemp = Convert.ToDouble(gpuTemp)
            };
        }
        catch
        {
            return GetMockTempData();
        }
    }

    public ThermalMode GetThermalMode()
    {
        if (!IsAvailable) return ThermalMode.Balanced;

        try
        {
            var result = _gameZoneData!.InvokeMethod("GetSmartFanMode", null);
            int mode = Convert.ToInt32(result);
            return mode switch
            {
                0 => ThermalMode.Quiet,
                1 => ThermalMode.Balanced,
                2 => ThermalMode.Performance,
                255 => ThermalMode.Custom,
                _ => ThermalMode.Balanced
            };
        }
        catch
        {
            return ThermalMode.Balanced;
        }
    }

    public bool SetThermalMode(ThermalMode mode)
    {
        if (!IsAvailable) return false;

        try
        {
            int modeValue = mode switch
            {
                ThermalMode.Quiet => 0,
                ThermalMode.Balanced => 1,
                ThermalMode.Performance => 2,
                ThermalMode.Custom => 255,
                _ => 1
            };

            _gameZoneData!.InvokeMethod("SetSmartFanMode", new object[] { modeValue });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SetFanSpeed(int fanIndex, int percent)
    {
        if (!IsAvailable) return false;

        try
        {
            percent = Math.Clamp(percent, 0, 100);
            _fanMethod!.InvokeMethod("Fan_SetSpeed", new object[] { fanIndex, percent });
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Returns simulated data when not running as admin or on unsupported hardware
    private static FanData GetMockFanData() => new()
    {
        CpuFanRpm = 0,
        GpuFanRpm = 0,
        CpuFanPercent = 0,
        GpuFanPercent = 0
    };

    private static TemperatureData GetMockTempData() => new()
    {
        CpuTemp = 0,
        GpuTemp = 0
    };

    public void Dispose()
    {
        if (_disposed) return;
        _fanMethod?.Dispose();
        _gameZoneData?.Dispose();
        _disposed = true;
    }
}
