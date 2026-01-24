using System.IO.Ports;
using System.Net.Sockets;
using System.Windows.Controls;

namespace TuringSmartScreenNet;

public enum Command : byte
{
    Reset = 101,        // 0x65
    Clear = 102,        // 0x66
    ScreenOff = 108,    // 0x6C
    ScreenOn = 109,     // 0x6D
    SetBrightness = 110,// 0x6E
    SetOrentation = 121,
    DisplayBitmap = 197 // 0xC5
}

public enum Orientation
{
    PORTRAIT = 0,
    LANDSCAPE = 2,
    REVERSE_PORTRAIT = 1,
    REVERSE_LANDSCAPE = 3,
}
public class ScreenDriver : IDisposable
{
    private SerialPort? _serialPort;
    private const int ChunkSize = 1024;
    private readonly string _portName;

    public ScreenDriver(string portName)
    {
        _portName = portName;
    }

    public void Connect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            _serialPort.Dispose();
        }

        _serialPort = new SerialPort(_portName)
        {
            BaudRate = 115200,
            Parity = Parity.None,
            DataBits = 8,
            StopBits = StopBits.One,
            DtrEnable = false, // 保持 false 避免重启
            RtsEnable = false,
            WriteTimeout = 2000
        };

        _serialPort.Open();
        Thread.Sleep(1000); // 等待握手

        if (!_serialPort.IsOpen) throw new Exception("端口打开失败");

        _serialPort.DiscardInBuffer();
        _serialPort.DiscardOutBuffer();
    }

    public void SendCommand(Command cmd)
    {
        if (_serialPort?.IsOpen != true) return;
        byte[] packet = BuildHeader(cmd, 0, 0, 0, 0);
        _serialPort.Write(packet, 0, packet.Length);
    }

    public void SetOrientation(Orientation orientation, int width, int height)
    {
        if (_serialPort?.IsOpen != true) return;

        int x = 0,
        y = 0,
        ex = 0,
        ey = 0;
        var byteBuffer = new byte[16];
        byteBuffer[0] = (byte)(x >> 2);
        byteBuffer[1] = (byte)(((x & 3) << 6) + (y >> 4));
        byteBuffer[2] = (byte)(((y & 15) << 4) + (ex >> 6));
        byteBuffer[3] = (byte)(((ex & 63) << 2) + (ey >> 8));
        byteBuffer[4] = (byte)(ey & 255);
        byteBuffer[5] = (byte)Command.SetOrentation;
        byteBuffer[6] = (byte)(orientation + 100);
        byteBuffer[7] = (byte)(width >> 8);
        byteBuffer[8] = (byte)(width & 255);
        byteBuffer[9] = (byte)(height >> 8);
        byteBuffer[10] = (byte)(height & 255);
        _serialPort.Write(byteBuffer, 0, byteBuffer.Length);
    }

    public void SendImage((byte[] imageData, int x, int y, int width, int height) data)
    {
        if (_serialPort?.IsOpen != true) return;

        byte[] header = BuildHeader(Command.DisplayBitmap, data.x, data.y, data.width, data.height);
        _serialPort.Write(header, 0, header.Length);

        for (int i = 0; i < data.imageData.Length; i += ChunkSize)
        {
            int count = Math.Min(ChunkSize, data.imageData.Length - i);
            _serialPort.Write(data.imageData, i, count);
        }
    }

    public void SetBrightness(int level)
    {
        if (_serialPort?.IsOpen != true) return;

        var level_absolute = 255 - ((level / 100) * 255);
        if (_serialPort?.IsOpen != true) return;
        byte[] packet = BuildHeader(Command.SetBrightness, level_absolute, 0, 0, 0);
        _serialPort.Write(packet, 0, packet.Length);
    }

    public static byte[] BuildHeader(Command cmd, int x, int y, int width, int height)
    {
        int ex = x + width - 1;
        int ey = y + height - 1;
        var buffer = new byte[6];

        buffer[0] = (byte)(x >> 2);
        buffer[1] = (byte)(((x & 3) << 6) + (y >> 4));
        buffer[2] = (byte)(((y & 15) << 4) + (ex >> 6));
        buffer[3] = (byte)(((ex & 63) << 2) + (ey >> 8));
        buffer[4] = (byte)(ey & 255);
        buffer[5] = (byte)cmd;

        return buffer;
    }
    public void Dispose()
    {
        if (_serialPort?.IsOpen == true)
        {
            SendCommand(Command.ScreenOff);
            _serialPort.Close();
        }
        _serialPort?.Dispose();
    }
}