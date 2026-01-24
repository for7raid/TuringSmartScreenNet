using LibreHardwareMonitor.Hardware;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TuringSmartScreenNet
{
    internal class HardwareInfoProvider
    {
        private readonly Computer computer;

        public HardwareInfoProvider()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true,
                IsMotherboardEnabled = true,
            };
            computer.Open();
        }
        public HardwareInfo CollectInfo()
        {
            
            var info = new HardwareInfo();
            foreach (var hardware in computer.Hardware)
            {
                hardware.Update();

                var sensors = hardware.Sensors
                    .Where(s => s.Value.HasValue)
                    .ToList();


                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    info.CPUTemperature = (int)(sensors.FirstOrDefault(static s => s.SensorType == SensorType.Temperature && s.Name == "CPU Package")?.Value ?? 0);
                    info.CPUFreq = (sensors.FirstOrDefault(static s => s.SensorType == SensorType.Clock)?.Value ?? 0) / 1000;
                    info.CPUUsage = (int)(sensors.FirstOrDefault(static s => s.SensorType == SensorType.Load && s.Name == "CPU Total")?.Value ?? 0);

                }

                if (hardware.HardwareType == HardwareType.GpuIntel)
                {
                    info.GPUUsage = (int)(sensors.FirstOrDefault(static s => s.SensorType == SensorType.Load && s.Name == "GPU Core")?.Value ?? info.GPUUsage);
                }

                if (hardware.HardwareType == HardwareType.Memory && hardware.Name == "Total Memory")
                {
                    info.RAMUsage = (int)(sensors.FirstOrDefault(static s => s.SensorType == SensorType.Load)?.Value ?? 0);
                }

                if (hardware.HardwareType == HardwareType.Motherboard)
                {
                    foreach (var subHardware in hardware.SubHardware)
                    {
                        subHardware.Update();
                        var subSensors = subHardware.Sensors
                           .Where(s => s.Value.HasValue)
                           .ToList();
                        info.FANFreq = (int)(subSensors.FirstOrDefault(static s => s.SensorType == SensorType.Fan && s.Name == "CPU Fan")?.Value ?? 0);
                        info.FANFreqPct = (int)(subSensors.FirstOrDefault(static s => s.SensorType == SensorType.Control && s.Name == "CPU Fan")?.Value ?? 0);
                    }

                }
            }

            
            return info;
        }
    }

    internal class HardwareInfo
    {
        public int CPUUsage { get; set; } = 11;
        public int CPUTemperature { get; set; } = 45;
        public float CPUFreq { get; set; } = 1.6545f;
        public int GPUUsage { get; set; } = 5;
        public int FANFreq { get; set; } = 1234;
        public int FANFreqPct { get; set; } = 34;
        public int RAMUsage { get; set; } = 24;

    }

}