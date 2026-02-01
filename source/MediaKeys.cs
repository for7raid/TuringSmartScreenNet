using System.Runtime.InteropServices;
namespace TuringSmartScreenNet
{

    public static class MediaKeys
    {
        private const int INPUT_KEYBOARD = 1;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const int KEYEVENTF_KEYUP = 0x0002;

        private const short VK_MEDIA_PLAY_PAUSE = 0xB3;
        private const short VK_MEDIA_NEXT_TRACK = 0xB0;
        private const short VK_MEDIA_PREV_TRACK = 0xB1;
        private const short VK_VOLUME_UP = 0xAF;
        private const short VK_VOLUME_DOWN = 0xAE;
        private const short VK_VOLUME_MUTE = 0xAD;

        // If 32 bit, FieldOffset = 4; else FieldOffset = 8
        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public int type;
            [FieldOffset(8)]
            public MOUSEINPUT mi;
            [FieldOffset(8)]
            public KEYBDINPUT ki;
            [FieldOffset(8)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private static void Send(short key)
        {
            INPUT[] inputs = new INPUT[2];

            inputs[0] = new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = key,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYDOWN,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }

            };

            inputs[1] = new INPUT
            {
                type = INPUT_KEYBOARD,
                ki = new KEYBDINPUT
                {
                    wVk = key,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }

            };

            uint sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

            if (sent == 0)
            {
                // Получаем ошибку WinAPI
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine("SendInput failed with error: " + error);
            }
            else
            {
                Console.WriteLine("SendInput succeeded");
            }
        }

        // ===== публичные методы =====

        public static void PlayPause() => Send(VK_MEDIA_PLAY_PAUSE);
        public static void Next() => Send(VK_MEDIA_NEXT_TRACK);
        public static void Previous() => Send(VK_MEDIA_PREV_TRACK);
        public static void VolumeUp() => Send(VK_VOLUME_UP);
        public static void VolumeDown() => Send(VK_VOLUME_DOWN);
        public static void Mute() => Send(VK_VOLUME_MUTE);
    }

}
