using Microsoft.Win32;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Task5.ImageHandlers
{
    public class SignalManager
    {
        public static BitmapImage CropImage(BitmapImage inputImage, int centerX, int centerY,  int width, int height)
        {
            int x = centerX - width / 2;
            int y = centerY - height / 2;
            if(x <= 0 || y <= 0 || x + width >= inputImage.PixelWidth || y + height >= inputImage.PixelHeight)
            {
                return null;
            }
            BitmapImage returnedImage = new BitmapImage();
            CroppedBitmap croppedBitmap = new CroppedBitmap(inputImage, new System.Windows.Int32Rect(x, y, width, height));
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBitmap));
                encoder.Save(memory);
                memory.Position = 0;

                returnedImage.BeginInit();
                returnedImage.StreamSource = memory;
                returnedImage.CacheOption = BitmapCacheOption.OnLoad;
                returnedImage.EndInit();
                returnedImage.Freeze(); // Makes the image WPF-thread-safe
            }
            return returnedImage;

        }
        public static BitmapImage ApplyLowPassFilter(BitmapImage sourceImage, double kernelSize = 5, double sigma = 1.5)
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
                // Ensure kernel size is odd
                int kSize = (int)kernelSize;
                if (kSize % 2 == 0) kSize++;

                // Apply Gaussian blur (low-pass filter)
                Cv2.GaussianBlur(image, result, new OpenCvSharp.Size(kSize, kSize), sigma);

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
                return null;
            }
        }
        public static BitmapImage ApplyHighPassFilter(BitmapImage sourceImage)
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
                // Create Gaussian blurred version for low frequencies
                Mat blurred = new Mat();
                Cv2.GaussianBlur(image, blurred, new OpenCvSharp.Size(21, 21), 3);

                // Subtract blurred image (low frequencies) from original to get high frequencies
                Cv2.Subtract(image, blurred, result);

                // Enhance the result by adding back to original
                Cv2.AddWeighted(image, 1.5, result, 0.75, 0, result);

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
                blurred.Dispose();
                result.Dispose();

                return outputImage;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static double CalculateImageIntensity(BitmapImage image)
        {
            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            double totalIntensity = 0;
            int pixelCount = 0;

            // Calculate intensity based on the pixel format
            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixels.Length; i += 4)  // 4 bytes per pixel (BGRA)
                {
                    byte blue = pixels[i];
                    byte green = pixels[i + 1];
                    byte red = pixels[i + 2];
                    // Calculate intensity (average of RGB values)
                    totalIntensity += (red + green + blue) / 3.0;
                    pixelCount++;
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int i = 0; i < pixels.Length; i += 3)  // 3 bytes per pixel (BGR)
                {
                    byte blue = pixels[i];
                    byte green = pixels[i + 1];
                    byte red = pixels[i + 2];
                    totalIntensity += (red + green + blue) / 3.0;
                    pixelCount++;
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int i = 0; i < pixels.Length; i++)  // 1 byte per pixel
                {
                    totalIntensity += pixels[i];
                    pixelCount++;
                }
            }

            // Return average intensity
            return pixelCount > 0 ? totalIntensity / pixelCount : 0;
        }
        public static double CalculateDeviation(BitmapImage image)
        {
            double imageAverage = CalculateImageIntensity(image);
            double varianceSquaredSummation = 0;
            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            int pixelCount = 0;

            // Calculate intensity based on the pixel format
            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixels.Length; i += 4)  // 4 bytes per pixel (BGRA)
                {
                    byte blue = pixels[i];
                    byte green = pixels[i + 1];
                    byte red = pixels[i + 2];
                    // Calculate intensity (average of RGB values)
                    varianceSquaredSummation += Math.Pow(((red + green + blue) / 3.0) - imageAverage, 2);
                    pixelCount++;
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int i = 0; i < pixels.Length; i += 3)  // 3 bytes per pixel (BGR)
                {
                    byte blue = pixels[i];
                    byte green = pixels[i + 1];
                    byte red = pixels[i + 2];
                    varianceSquaredSummation += Math.Pow(((red + green + blue) / 3.0) - imageAverage, 2);
                    pixelCount++;
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int i = 0; i < pixels.Length; i++)  // 1 byte per pixel
                {
                    varianceSquaredSummation += Math.Pow(pixels[i] - imageAverage, 2);
                    pixelCount++;
                }
            }
            return Math.Sqrt(varianceSquaredSummation / pixelCount);
        }
        public static void SaveImage(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder(); // or JpegBitmapEncoder for JPEG
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
        public static double CalculateSNR(BitmapImage signal, BitmapImage noise)
        {
            return CalculateImageIntensity(signal) / (CalculateDeviation(noise));
        }

        public static double CalculateCNR(BitmapImage signal1, BitmapImage signal2,  BitmapImage noise)
        {
            return Math.Abs(CalculateImageIntensity(signal1) - CalculateImageIntensity(signal2)) / CalculateDeviation(noise);
        }
    }
}
