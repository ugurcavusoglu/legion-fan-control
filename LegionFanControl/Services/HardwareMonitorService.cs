using LibreHardwareMonitor.Hardware;
using LegionFanControl.Models;

namespace LegionFanControl.Services;

public class HardwareMonitorService : IDisposable
{
    private readonly Computer _computer;

    public HardwareMonitorService()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
        };
        _computer.Open();
    }

    public TemperatureData GetTemperatures()
    {
        var result = new TemperatureData();

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            if (hardware.HardwareType == HardwareType.Cpu)
            {
                var tempSensor = hardware.Sensors
                    .Where(s => s.SensorType == SensorType.Temperature)
                    .OrderByDescending(s => s.Value)
                    .FirstOrDefault(s => s.Name.Contains("Package") || s.Name.Contains("Core Max") || s.Name.Contains("CPU"));

                tempSensor ??= hardware.Sensors
                    .FirstOrDefault(s => s.SensorType == SensorType.Temperature);

                if (tempSensor?.Value != null)
                    result.CpuTemp = Math.Round((double)tempSensor.Value, 1);
            }
            else if (hardware.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel)
            {
                var tempSensor = hardware.Sensors
                    .FirstOrDefault(s => s.SensorType == SensorType.Temperature);

                if (tempSensor?.Value != null)
                    result.GpuTemp = Math.Round((double)tempSensor.Value, 1);
            }
        }

        return result;
    }

    public void Dispose()
    {
        _computer.Close();
    }
}
