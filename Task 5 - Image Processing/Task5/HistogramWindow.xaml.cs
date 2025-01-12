using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OpenCvSharp;
using Point = OpenCvSharp.Point;

namespace Task5
{
    /// <summary>
    /// Interaction logic for HistogramWindow.xaml
    /// </summary>
    public partial class HistogramWindow : System.Windows.Window
    {
        Mat image;
        public HistogramWindow(BitmapImage bitmapImage)
        {
            InitializeComponent();
            // Save BitmapImage to temporary file
            string tempPath = System.IO.Path.GetTempFileName() + ".png";
            using (var fileStream = new System.IO.FileStream(tempPath, System.IO.FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(fileStream);
            }

            // Read with OpenCV
            image = Cv2.ImRead(tempPath, ImreadModes.Grayscale);
            HistogramCanvas.Width = 800;      // Increase from 512
            HistogramCanvas.Height = 600;     // Increase from 300
            this.Width = 850;                 // Make window slightly larger than canvas
            this.Height = 650;
        }

        public void DisplayHistogram()
        {
            if (image == null || image.Empty()) return;

            try
            {
                // Clear previous histogram
                HistogramCanvas.Children.Clear();

                // Calculate histogram
                Mat hist = new Mat();
                Mat[] images = { image };
                int[] channels = { 0 }; // Grayscale channel
                Mat mask = new Mat();
                int[] histSize = { 256 };
                Rangef[] ranges = { new Rangef(0, 256) };

                // Calculate histogram
                Cv2.CalcHist(
                    images,
                    channels,
                    mask,
                    hist,
                    1,
                    histSize,
                    ranges);

                // Find the maximum value for scaling
                double minVal, maxVal;
                Point minLoc, maxLoc;
                Cv2.MinMaxLoc(hist, out minVal, out maxVal, out minLoc, out maxLoc);

                // Scale factor for display
                double scale = (HistogramCanvas.Height - 20) / maxVal; // Leave some margin

                // Draw background
                var background = new Rectangle
                {
                    Width = HistogramCanvas.Width,
                    Height = HistogramCanvas.Height,
                    Fill = new SolidColorBrush(Colors.White)
                };
                HistogramCanvas.Children.Add(background);

                // Draw histogram bars
                double barWidth = (HistogramCanvas.Width - 20) / 256.0; // Leave some margin

                for (int i = 0; i < 256; i++)
                {
                    float binVal = hist.Get<float>(i);
                    double barHeight = binVal * scale;

                    var bar = new Rectangle
                    {
                        Width = Math.Max(1, barWidth - 1), // Ensure minimum width of 1
                        Height = Math.Max(1, barHeight), // Ensure minimum height of 1
                        Fill = new SolidColorBrush(Colors.Black)
                    };

                    Canvas.SetLeft(bar, 10 + (i * barWidth)); // Start 10 pixels from left
                    Canvas.SetTop(bar, HistogramCanvas.Height - barHeight - 10); // 10 pixels from bottom

                    HistogramCanvas.Children.Add(bar);
                }

                // Draw axes
                var xAxis = new Line
                {
                    X1 = 10,
                    Y1 = HistogramCanvas.Height - 10,
                    X2 = HistogramCanvas.Width - 10,
                    Y2 = HistogramCanvas.Height - 10,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };

                var yAxis = new Line
                {
                    X1 = 10,
                    Y1 = 10,
                    X2 = 10,
                    Y2 = HistogramCanvas.Height - 10,
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };

                HistogramCanvas.Children.Add(xAxis);
                HistogramCanvas.Children.Add(yAxis);

                // Add X-axis labels
                for (int i = 0; i <= 255; i += 50)
                {
                    var xLabel = new TextBlock
                    {
                        Text = i.ToString(),
                        FontSize = 10
                    };
                    Canvas.SetLeft(xLabel, 10 + (i * barWidth) - 10);
                    Canvas.SetTop(xLabel, HistogramCanvas.Height - 25);
                    HistogramCanvas.Children.Add(xLabel);
                }

                // Add Y-axis labels
                for (int i = 0; i <= 4; i++)
                {
                    var yLabel = new TextBlock
                    {
                        Text = Math.Round(maxVal * i / 4).ToString(),
                        FontSize = 10
                    };
                    Canvas.SetLeft(yLabel, 5);
                    Canvas.SetTop(yLabel, HistogramCanvas.Height - 20 - (i * (HistogramCanvas.Height - 40) / 4));
                    HistogramCanvas.Children.Add(yLabel);
                }

                // Add axis titles
                var xAxisTitle = new TextBlock
                {
                    Text = "Pixel Intensity",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(xAxisTitle, HistogramCanvas.Width / 2 - 40);
                Canvas.SetTop(xAxisTitle, HistogramCanvas.Height - 5);
                HistogramCanvas.Children.Add(xAxisTitle);

                var yAxisTitle = new TextBlock
                {
                    Text = "Pixel Count",
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    RenderTransform = new RotateTransform(-90)
                };
                Canvas.SetLeft(yAxisTitle, -HistogramCanvas.Height / 2 + 40);
                Canvas.SetTop(yAxisTitle, 20);
                HistogramCanvas.Children.Add(yAxisTitle);

                // Add title
                var title = new TextBlock
                {
                    Text = "Grayscale Histogram",
                    FontSize = 14,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(title, HistogramCanvas.Width / 2 - 60);
                Canvas.SetTop(title, 10);
                HistogramCanvas.Children.Add(title);

                // Cleanup
                hist.Dispose();
                mask.Dispose();
            }
            catch (Exception ex)
            {
                var errorText = new TextBlock
                {
                    Text = "Error: " + ex.Message,
                    Foreground = new SolidColorBrush(Colors.Red)
                };
                Canvas.SetLeft(errorText, 10);
                Canvas.SetTop(errorText, 10);
                HistogramCanvas.Children.Add(errorText);
            }
        }
    }
}
