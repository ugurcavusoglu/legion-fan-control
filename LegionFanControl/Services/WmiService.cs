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
            var scope = new ManagementScope(WmiNamespace);

            using var fanQuery = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT * FROM LENOVO_FAN_METHOD"));
            _fanMethod = fanQuery.Get().OfType<ManagementObject>().FirstOrDefault();

            using var zoneQuery = new ManagementObjectSearcher(scope,
                new ObjectQuery("SELECT * FROM LENOVO_GAMEZONE_DATA"));
            _gameZoneData = zoneQuery.Get().OfType<ManagementObject>().FirstOrDefault();

            IsAvailable = _fanMethod != null && _gameZoneData != null;
        }
        catch
        {
            IsAvailable = false;
        }
    }

    private int InvokeFanSpeed(byte fanId)
    {
        var inParams = _fanMethod!.GetMethodParameters("Fan_GetCurrentFanSpeed");
        inParams["FanID"] = fanId;
        var outParams = _fanMethod.InvokeMethod("Fan_GetCurrentFanSpeed", inParams, null);
        return Convert.ToInt32(outParams["CurrentFanSpeed"]);
    }

    private int InvokeIntResult(ManagementObject obj, string method, string resultProperty)
    {
        var outParams = obj.InvokeMethod(method, null, null);
        return Convert.ToInt32(outParams[resultProperty]);
    }

    public FanData GetFanData()
    {
        if (!IsAvailable) return GetMockFanData();

        try
        {
            int cpuRpm = InvokeFanSpeed(0);
            int gpuRpm = InvokeFanSpeed(1);

            const int maxRpm = 5000;

            return new FanData
            {
                CpuFanRpm = cpuRpm,
                GpuFanRpm = gpuRpm,
                CpuFanPercent = Math.Clamp(cpuRpm * 100 / maxRpm, 0, 100),
                GpuFanPercent = Math.Clamp(gpuRpm * 100 / maxRpm, 0, 100),
            };
        }
        catch
        {
            return GetMockFanData();
        }
    }

    public ThermalMode GetThermalMode()
    {
        if (!IsAvailable) return ThermalMode.Balanced;

        try
        {
            int mode = InvokeIntResult(_gameZoneData!, "GetSmartFanMode", "Data");
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

            var inParams = _gameZoneData!.GetMethodParameters("SetSmartFanMode");
            inParams["Data"] = (uint)modeValue;
            _gameZoneData.InvokeMethod("SetSmartFanMode", inParams, null);
            return true;
        }
        catch
        {
            return false;
        }
    }

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
