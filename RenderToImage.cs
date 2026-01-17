using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace TuringSmartScreenNet
{
    internal class RenderToImage
    {
        public static void SaveWpfElementAsImage(FrameworkElement element, string filePath)
        {
            // Ensure the element has its layout calculated if it's not already displayed
            if (element.ActualWidth == 0 || element.ActualHeight == 0)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                element.Arrange(new Rect(element.DesiredSize));
            }

            // Define the dimensions of the bitmap and DPI
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            double dpi = 96; // Standard WPF DPI

            // Create a RenderTargetBitmap
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                width,
                height,
                dpi,
                dpi,
                PixelFormats.Pbgra32); // Use Pbgra32 for transparency support

            // Render the visual into the bitmap
            renderTargetBitmap.Render(element);

            // Use a BitmapEncoder to save the image to a specific format
            // This example uses PngBitmapEncoder
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            // Save the encoded image to a file stream
            try
            {
                using (Stream stream = File.Create(filePath))
                {
                    pngBitmapEncoder.Save(stream);
                }
            }
            catch (IOException ex)
            {
                // Handle file writing errors
                MessageBox.Show($"Error saving file: {ex.Message}");
            }
        }

        public static byte[] SaveWpfElementAsBitmap(FrameworkElement element)
        {
            // Ensure the element has its layout calculated if it's not already displayed
            if (element.ActualWidth == 0 || element.ActualHeight == 0)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                element.Arrange(new Rect(element.DesiredSize));
            }

            // Define the dimensions of the bitmap and DPI
            int width = (int)element.ActualWidth;
            int height = (int)element.ActualHeight;
            double dpi = 96; // Standard WPF DPI

            // Create a RenderTargetBitmap
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                width,
                height,
                dpi,
                dpi,
                PixelFormats.Pbgra32); // Use Pbgra32 for transparency support

            // Render the visual into the bitmap
            renderTargetBitmap.Render(element);

            WriteableBitmap wb = new WriteableBitmap(renderTargetBitmap);
            wb.Lock();
            renderTargetBitmap.CopyPixels(
                    new Int32Rect(0, 0, renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight),
                    wb.BackBuffer,
                    wb.BackBufferStride * wb.PixelHeight,
                    wb.BackBufferStride);

            wb.AddDirtyRect(new Int32Rect(0, 0, wb.PixelWidth, wb.PixelHeight));
            IntPtr buffer = wb.BackBuffer;
            int stride = wb.BackBufferStride;
            byte[] pixels = new byte[stride * height];
            System.Runtime.InteropServices.Marshal.Copy(buffer, pixels, 0, pixels.Length);
            wb.Unlock();

            var array = new List<byte>();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];

                // RGB888 -> RGB565 转换
                ushort r5 = (ushort)((r >> 3) & 0x1F);
                ushort g6 = (ushort)((g >> 2) & 0x3F);
                ushort b5 = (ushort)((b >> 3) & 0x1F);
                ushort rgb565 = (ushort)((r5 << 11) | (g6 << 5) | b5);

                // 写入结果 (小端序: 低字节在前)
                array.Add((byte)(rgb565 & 0xFF));
                array.Add((byte)((rgb565 >> 8) & 0xFF));
            }

            return array.ToArray();

        }
    }
}
