


using System;
using System.Diagnostics;
using System.IO;
using System.Security.RightsManagement;
using System.Text;
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
                102,
                105,
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

        public static (byte[] data, int x, int y, int width, int height) SaveWpfElementAsBitmap(FrameworkElement element, Visual relativeTo)
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
            GeneralTransform transform = element.TransformToAncestor(relativeTo);
            Point position1 = transform.Transform(new Point(0, 0));

            Point screenPosition = element.PointToScreen(new Point(0d, 0d));
            Point screenPositionRelative = relativeTo.PointToScreen(new Point(0d, 0d));
            Point position = new Point(screenPosition.X - screenPositionRelative.X, screenPosition.Y - screenPositionRelative.Y);

            // Create a RenderTargetBitmap
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(
                width,
                height,
                102,
                105,
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


            byte[] resultBuffer = new byte[width * height * 2];
            int rowBytes = wb.BackBufferStride;
            int totalPixels = wb.BackBufferStride * wb.PixelHeight;

            unsafe
            {
                byte* baseSrcPtr = (byte*)wb.BackBuffer;

                fixed (byte* dstPtr = resultBuffer)
                {
                    byte* d = dstPtr;

                    for (int i = 0; i < totalPixels; i += 4)
                    {
                        byte* s = baseSrcPtr + i;

                        byte b = s[0];
                        byte g = s[1];
                        byte r = s[2];

                        // RGB888 -> RGB565 转换
                        ushort r5 = (ushort)((r >> 3) & 0x1F);
                        ushort g6 = (ushort)((g >> 2) & 0x3F);
                        ushort b5 = (ushort)((b >> 3) & 0x1F);
                        ushort rgb565 = (ushort)((r5 << 11) | (g6 << 5) | b5);

                        d[0] = (byte)(rgb565 & 0xFF);
                        d[1] = (byte)((rgb565 >> 8) & 0xFF);

                        d += 2;
                    }

                }
            }

            wb.Unlock();

            return (resultBuffer, (int)position.X, (int)position.Y, width, height);

        }

        public static (byte[] data, int x, int y, int width, int height) Crop(byte[] data, int x, int y, int width, int height)
        {
            int screenWidth = 320,
                bytesPerPixel = 2,
                rowBytes = screenWidth * bytesPerPixel;

            byte[] resultBuffer = new byte[width * height * 2];
            unsafe
            {
                fixed (byte* dstPtr = resultBuffer)
                {
                    for (int i = 0; i < height; i++)
                    {

                        var start = ((i + y) * screenWidth + x) * bytesPerPixel;
                        var destStart = (nint)dstPtr + (nint)(i * width * bytesPerPixel);
                        System.Runtime.InteropServices.Marshal.Copy(data, start, destStart, width * 2);


                    }
                }
            }

            return (resultBuffer, x, y, width, height);
        }

        public static List<(byte[] data, int x, int y, int width, int height)> GetDiffs(byte[] prev, byte[] current)
        {
            const int width = 320;
            const int height = 480;
            const int dilationRadius = 5;

            var diff = new bool[width * height];

            for (int i = 0; i < prev.Length; i += 2)
            {
                diff[i / 2] = prev[i] != current[i] || prev[i + 1] != current[i + 1];
            }

            var dilated = Dilate(diff, width, height, dilationRadius);
            var components = ExtractComponents(dilated, width, height);

            return components.Select(c => Crop(current, c.x, c.y, c.w, c.h)).ToList();
        }

        static bool[] Dilate(bool[] src, int width, int height, int radius)
        {
            var dst = new bool[src.Length];

            int Index(int x, int y) => y * width + x;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (!src[Index(x, y)])
                        continue;

                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        int ny = y + dy;
                        if (ny < 0 || ny >= height)
                            continue;

                        for (int dx = -radius; dx <= radius; dx++)
                        {
                            int nx = x + dx;
                            if (nx < 0 || nx >= width)
                                continue;

                            dst[Index(nx, ny)] = true;
                        }
                    }
                }
            }

            return dst;
        }
        static List<(int x, int y, int w, int h)> ExtractComponents(bool[] mask, int width, int height)
        {
            var visited = new bool[mask.Length];
            var rects = new List<(int x, int y, int w, int h)>();

            int Index(int x, int y) => y * width + x;

            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int i = Index(x, y);
                    if (!mask[i] || visited[i])
                        continue;

                    var queue = new Queue<(int x, int y)>();
                    queue.Enqueue((x, y));
                    visited[i] = true;

                    int minX = x, maxX = x;
                    int minY = y, maxY = y;

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();

                        for (int k = 0; k < 8; k++)
                        {
                            int nx = cx + dx[k];
                            int ny = cy + dy[k];

                            if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                                continue;

                            int ni = Index(nx, ny);
                            if (visited[ni] || !mask[ni])
                                continue;

                            visited[ni] = true;
                            queue.Enqueue((nx, ny));

                            minX = Math.Min(minX, nx);
                            maxX = Math.Max(maxX, nx);
                            minY = Math.Min(minY, ny);
                            maxY = Math.Max(maxY, ny);
                        }
                    }

                    rects.Add((
                        minX,
                        minY,
                        maxX - minX + 1,
                        maxY - minY + 1));
                }
            }

            return rects;
        }

    }
}
