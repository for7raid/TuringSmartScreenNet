using System.Diagnostics;
using System.Management;

namespace TuringSmartScreenNet
{
    internal class HardwareInfoProvider : IDisposable
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _cpuPerfCounter;
        private readonly uint MaxCPUClockSpeed;
        private readonly ManagementObjectSearcher _wmiOperatingSystem;
        private List<PerformanceCounter>? _gpuCounters;
        private bool _disposedValue;

        public HardwareInfoProvider()
        {

            _cpuCounter = new PerformanceCounter(
                 "Processor",
                 "% Processor Time",
                 "_Total"
            );

            _cpuPerfCounter = new PerformanceCounter(
                 "Processor Information",
                 "% Processor Performance",
                  "_Total"
            );

            using var CPU0 = new ManagementObject("Win32_Processor.DeviceID='CPU0'");
            MaxCPUClockSpeed = (uint)CPU0["MaxClockSpeed"];

            _wmiOperatingSystem = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            CreateGPUCounters();
        }

        private void CreateGPUCounters()
        {
            try
            {
                var _gpuCounterCategory = new PerformanceCounterCategory("GPU Engine");

                _gpuCounters = _gpuCounterCategory
                                    .GetInstanceNames()
                                    .Where(counterName => counterName.EndsWith("engtype_3D"))
                                    .SelectMany(_gpuCounterCategory.GetCounters)
                                    .Where(counter => counter.CounterName.Equals("Utilization Percentage"))
                                    .ToList();
            }
            catch
            {

            }

        }

        public HardwareInfo CollectInfo()
        {

            var info = new HardwareInfo();

            try
            {
                info.CPUUsage = (int)_cpuCounter.NextValue();

                float cpuPerf = _cpuPerfCounter.NextValue();
                info.CPUFreq = MaxCPUClockSpeed * (cpuPerf / 100) / 1000;

                var memoryValues = _wmiOperatingSystem.Get().Cast<ManagementObject>().Select(mo => new
                {
                    FreePhysicalMemory = (ulong)mo["FreePhysicalMemory"],
                    TotalVisibleMemorySize = (ulong)mo["TotalVisibleMemorySize"]
                }).FirstOrDefault();

                if (memoryValues != null)
                {
                    info.RAMUsage = (int)(((memoryValues.TotalVisibleMemorySize - memoryValues.FreePhysicalMemory) / (double)memoryValues.TotalVisibleMemorySize) * 100);
                }
            }
            catch
            {

            }

            try
            {
                info.GPUUsage = (int)_gpuCounters.Sum(x => x.NextValue());
            }
            catch (Exception ex)
            {
                CreateGPUCounters();
            }
            return info;

            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {


                _cpuCounter?.Dispose();
                _cpuPerfCounter?.Dispose();
                _wmiOperatingSystem?.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class HardwareInfo
    {
        public int CPUUsage { get; set; }
        public int CPUTemperature { get; set; }
        public float CPUFreq { get; set; }
        public int GPUUsage { get; set; } = 100;
        public int FANFreq { get; set; }
        public int FANFreqPct { get; set; }
        public int RAMUsage { get; set; }

    }

}