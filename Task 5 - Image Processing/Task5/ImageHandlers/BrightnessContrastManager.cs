using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Runtime.InteropServices;

namespace Task5.ImageHandlers
{
    public class BrightnessContrastManager
    {
        public static BitmapImage AdjustBrightnessContrast(BitmapImage sourceImage,
    double brightness = 0,    // -255 to 255
    double contrast = 1.0)    // 0.0 to 3.0
        {
            // Convert BitmapImage to Mat
            string tempPath = System.IO.Path.GetTempFileName() + ".png";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(sourceImage));
                encoder.Save(fileStream);
            }

            // Read with OpenCV
            Mat image = Cv2.ImRead(tempPath, ImreadModes.Grayscale);
            Mat result = new Mat();

            try
            {
                // Adjust brightness and contrast
                image.ConvertTo(result, -1, contrast, brightness);

                // Convert back to BitmapImage
                BitmapImage outputImage = new BitmapImage();
                Cv2.ImWrite(tempPath, result);

                outputImage.BeginInit();
                outputImage.CacheOption = BitmapCacheOption.OnLoad;
                outputImage.UriSource = new Uri(tempPath);
                outputImage.EndInit();
                outputImage.Freeze();

                // Cleanup
                File.Delete(tempPath);
                image.Dispose();
                result.Dispose();

                return outputImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adjusting brightness and contrast: {ex.Message}");
                return null;
            }
        }

        public static BitmapImage ApplyCLAHE(BitmapImage sourceImage,
            double clipLimit = 2.0,
            int tileGridSize = 8)
        {
            string tempPath = System.IO.Path.GetTempFileName() + ".png";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(sourceImage));
                encoder.Save(fileStream);
            }

            try
            {
                Mat image = Cv2.ImRead(tempPath, ImreadModes.Grayscale);
                Mat result = new Mat();

                var clahe = Cv2.CreateCLAHE(clipLimit, new OpenCvSharp.Size(tileGridSize, tileGridSize));
                clahe.Apply(image, result);
                clahe.Dispose();

                // Convert to BitmapImage
                BitmapImage outputImage = new BitmapImage();
                Cv2.ImWrite(tempPath, result);

                outputImage.BeginInit();
                outputImage.CacheOption = BitmapCacheOption.OnLoad;
                outputImage.UriSource = new Uri(tempPath);
                outputImage.EndInit();
                outputImage.Freeze();

                // Cleanup
                File.Delete(tempPath);
                image.Dispose();
                result.Dispose();

                return outputImage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static BitmapImage ApplyHistogramEqualization(BitmapImage sourceImage)
        {
            // Convert BitmapImage to Mat
            string tempPath = System.IO.Path.GetTempFileName() + ".png";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(sourceImage));
                encoder.Save(fileStream);
            }
                // Read the image
                Mat image = Cv2.ImRead(tempPath, ImreadModes.Grayscale);
                Mat result = new Mat();
                Cv2.EqualizeHist(image, result);

                // Convert back to BitmapImage
                BitmapImage outputImage = new BitmapImage();
                Cv2.ImWrite(tempPath, result);

                outputImage.BeginInit();
                outputImage.CacheOption = BitmapCacheOption.OnLoad;
                outputImage.UriSource = new Uri(tempPath);
                outputImage.EndInit();
                outputImage.Freeze();

                // Cleanup
                File.Delete(tempPath);
                image.Dispose();
                result.Dispose();

                return outputImage;
        }
        public static BitmapImage ApplyGammaCorrection(BitmapImage sourceImage, double gamma = 3.0)
        {
            string tempPath = System.IO.Path.GetTempFileName() + ".png";
            using (var fileStream = new FileStream(tempPath, FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(sourceImage));
                encoder.Save(fileStream);
            }

            try
            {
                Mat image = Cv2.ImRead(tempPath, ImreadModes.Color);
                Mat result = new Mat();

                // Create gamma lookup table
                Mat lookupTable = new Mat(1, 256, MatType.CV_8U);
                byte[] lookupValues = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    lookupValues[i] = (byte)(Math.Pow(i / 255.0, gamma) * 255.0);
                }

                // Use Marshal.Copy instead of SetArray
                Marshal.Copy(lookupValues, 0, lookupTable.Data, lookupValues.Length);

                // Apply gamma correction using lookup table
                Cv2.LUT(image, lookupTable, result);

                // Convert back to BitmapImage
                BitmapImage outputImage = new BitmapImage();
                Cv2.ImWrite(tempPath, result);

                outputImage.BeginInit();
                outputImage.CacheOption = BitmapCacheOption.OnLoad;
                outputImage.UriSource = new Uri(tempPath);
                outputImage.EndInit();
                outputImage.Freeze();

                // Cleanup
                File.Delete(tempPath);
                image.Dispose();
                result.Dispose();
                lookupTable.Dispose();

                return outputImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error applying gamma correction: {ex.Message}");
                return null;
            }
        }
    }
}
