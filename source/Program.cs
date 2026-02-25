using System.Runtime.InteropServices;
using System.Text;

class Test
{

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#684")]
    static extern int msvcr120d_684(int int_0);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#458")]
    static extern int msvcr120d_458();

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#515")]
    static extern int msvcr120d_515();

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#170")]
    static extern int msvcr120d_170(int int_0);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#630")]
    static extern int msvcr120d_630();

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#216")]
    static extern void msvcr120d_216(int int_0);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#531")]
    static extern void msvcr120d_531(int int_0, StringBuilder stringBuilder_0, int int_1);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#566")]
    static extern double msvcr120d_566(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#215")]
    static extern double msvcr120d_215(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#663")]
    static extern int msvcr120d_663(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#448")]
    static extern double msvcr120d_448(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#579")]
    static extern double msvcr120d_579(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#573")]
    static extern double msvcr120d_573(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#730")]
    static extern double msvcr120d_730(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2);

    [DllImport("msvcr120d.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "#139")]
    static extern double msvcr120d_139(int int_0, int int_1, StringBuilder stringBuilder_0, int int_2, StringBuilder stringBuilder_1, int int_3);

    void print()
    {
        bool flag = false;
        int num2 = 256;
        StringBuilder stringBuilder = new StringBuilder(256);
        StringBuilder stringBuilder2 = new StringBuilder(256);
        StringBuilder stringBuilder3 = new StringBuilder(256);


        Console.WriteLine(msvcr120d_684(64));
        int hardwareCount = msvcr120d_630();

        Console.WriteLine("Total Memoy Size = " + msvcr120d_515() + " MB");

        //Console.WriteLine($"msvcr120d_458() {msvcr120d_458()}");


        //for (int i = 0; i < hardwareCount; i++)
        //{
        //    msvcr120d_216(i);
        //    msvcr120d_531(i, stringBuilder, num2);
        //    Console.WriteLine(stringBuilder);
        //}


        int num5 = 0;

        int num7 = 1;

        Console.WriteLine($"num3 {hardwareCount}");

        for (int devIndex = 0; devIndex < hardwareCount; devIndex++)
        {
            msvcr120d_216(devIndex);

            msvcr120d_531(devIndex, stringBuilder, num2);
            Console.WriteLine("======== " + ((stringBuilder != null) ? stringBuilder.ToString() : null) + " ========\n");
            if (stringBuilder.ToString().Contains("ASUS EC"))
            {
                continue;
            }
            for (int sensorIndex = 0; sensorIndex < 512; sensorIndex++)
            {
                double num11;
                if (stringBuilder.ToString().Contains("CPU"))
                {
                    if (stringBuilder.ToString().Split(':').Count() > 1)
                    {
                        string string_ = stringBuilder.ToString().Split(':')[1].Replace("AMD", "").Replace("Intel", "").Trim();
                        //Console.WriteLine($"CPUMODEL {string_}");
                    }
                    num11 = msvcr120d_566(devIndex, sensorIndex, stringBuilder2, num2);
                    if (stringBuilder2.ToString().Contains("CPU"))
                    {
                        Console.WriteLine("CPUTEMP_b {0}= {1:N2} C\n", stringBuilder2, num11);
                    }
                    num11 = msvcr120d_573(devIndex, sensorIndex, stringBuilder2, num2);
                    string[] array = stringBuilder2.ToString().Split(' ');
                    if (array.Count() > 2 && array[0].ToLower().Contains("core") && array[2] == "Clock")
                    {
                        if (!array[0].ToLower().Contains("e-core"))
                        {

                            //諬.m_誃++;
                            Console.WriteLine("CPUCLOCK_b {0}= {1:N2} MHz\n", stringBuilder2, num11);
                        }
                        //諬.m_誈++;
                    }
                    num11 = msvcr120d_215(devIndex, sensorIndex, stringBuilder2, num2);
                    array = stringBuilder2.ToString().Split(' ');
                    if (array.Count() > 2 && array[0].ToLower().Contains("core") && array[2] == "VID")
                    {
                        Console.WriteLine("CPUVOLTAGE_b {0}= {1:N3} V\n", stringBuilder2, num11);
                    }
                    else if (stringBuilder2.ToString().Length > 0)
                    {
                        Console.WriteLine("CPUVOLTAGE_b {0}= {1:N2} V\n", stringBuilder2, num11);
                    }
                    num11 = msvcr120d_730(devIndex, sensorIndex, stringBuilder2, num2);
                    if (stringBuilder2.ToString().Contains("Total CPU Utility"))
                    {
                        Console.WriteLine("CPULOAD {0}= {1:N2}%%\n", stringBuilder2, num11);
                        flag = true;
                    }
                    if (!flag && stringBuilder2.ToString().Contains("Total CPU Usage"))
                    {
                        Console.WriteLine("CPULOAD {0}= {1:N2}%%\n", stringBuilder2, num11);
                    }
                    num11 = msvcr120d_579(devIndex, sensorIndex, stringBuilder2, num2);
                    if (stringBuilder2.ToString().Contains("Power") && !stringBuilder2.ToString().Contains("Limit"))
                    {

                        Console.WriteLine("Find power{0}= {1:N3} W\n", stringBuilder2, num11);
                    }
                    if (stringBuilder2.ToString().Contains("CPU Package Power"))
                    {
                        Console.WriteLine("CPUPWR {0}= {1:N3} W\n", stringBuilder2, num11);
                    }
                }

                stringBuilder3.Clear();
                num11 = msvcr120d_139(devIndex, sensorIndex, stringBuilder2, num2, stringBuilder3, num2);
                if (stringBuilder2.ToString().Contains("Physical Memory Available"))
                {

                    Console.WriteLine("RAMVALID {0}= {1:N3} %s\n", stringBuilder2, num11, stringBuilder3);
                }
                if (stringBuilder2.ToString().Contains("Physical Memory Used"))
                {

                    Console.WriteLine("RAM {0}= {1:N3} %s\n", stringBuilder2, num11, stringBuilder3);
                }
                if (stringBuilder2.ToString().Contains("Physical Memory Load"))
                {
                    Console.WriteLine("RAMLOAD {0}= {1:N3} %s\n", stringBuilder2, num11, stringBuilder3);
                }
                stringBuilder3.Clear();
                num11 = msvcr120d_566(devIndex, sensorIndex, stringBuilder2, num2);
                if ((stringBuilder2.ToString().Contains("Drive Temperature") || stringBuilder2.ToString().Contains("Drive Airflow Temperature")))
                {
                    //誰.Add(num7, stringBuilder.ToString());
                    //M_Data.認("HDDTEMP_b", num9, num10, num7.ToString());
                    Console.WriteLine("HDDTEMP_b harddisk {0}= {1:N2} C\n", stringBuilder2, num11);
                    num7++;
                }
                else if (stringBuilder2.ToString().Contains("CPU"))
                {
                    //M_Data.認("CPUTEMP_b", num9, num10, stringBuilder2.ToString());
                    Console.WriteLine("CPUTEMP_b {0}= {1:N2} C\n", stringBuilder2, num11);
                }
                num11 = msvcr120d_663(devIndex, sensorIndex, stringBuilder2, num2);
                if (stringBuilder2.ToString().Contains("GPU"))
                {
                    //M_Data.誈("GPUFAN", num9, num10);
                    Console.WriteLine("GPUFAN {0}= {1:N1} RPM\n", stringBuilder2, num11);
                }
                else if (stringBuilder2.ToString().Length > 0)
                {
                    //M_Data.認("FAN_b", num9, num10, num5 + "." + stringBuilder2.ToString());
                    num5++;
                    Console.WriteLine("FAN_b {0}= {1:N2} RPM\n", stringBuilder2, num11);
                }
            }
        }
    }


}