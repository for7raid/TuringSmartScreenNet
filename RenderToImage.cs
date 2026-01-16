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
    }
}
