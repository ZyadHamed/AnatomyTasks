using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Task5.ImageHandlers
{
    public class ImageNoiser
    {
        private static Random random = new Random();

        // Box-Muller transform method
        public static double NextGaussian(double mean, double stdDev)
        {
            // Box-Muller transform, a method for generating random numbers that follow a normal (Gaussian) distribution. 
            double u1 = 1.0 - random.NextDouble(); // Uniform(0,1) random doubles
            double u2 = 1.0 - random.NextDouble();

            // Random normal(0,1)
            double randomNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            // Transform to desired mean and standard deviation
            return mean + (stdDev * randomNormal);
        }
        public static BitmapImage ApplyGaussianNoise(BitmapImage image, double standardDeviation)
        {
            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // Need to cast to int first to handle overflow
                    int blue = pixels[i] + (int)NextGaussian(0, standardDeviation);
                    int green = pixels[i + 1] + (int)NextGaussian(0, standardDeviation);
                    int red = pixels[i + 2] + (int)NextGaussian(0, standardDeviation);

                    pixels[i] = (byte)Math.Max(0, Math.Min(255, blue));
                    pixels[i + 1] = (byte)Math.Max(0, Math.Min(255, green));
                    pixels[i + 2] = (byte)Math.Max(0, Math.Min(255, red));
                    // Alpha channel remains unchanged
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int i = 0; i < pixels.Length; i += 3)
                {
                    int blue = pixels[i] + (int)NextGaussian(0, standardDeviation);
                    int green = pixels[i + 1] + (int)NextGaussian(0, standardDeviation);
                    int red = pixels[i + 2] + (int)NextGaussian(0, standardDeviation);

                    pixels[i] = (byte)Math.Max(0, Math.Min(255, blue));
                    pixels[i + 1] = (byte)Math.Max(0, Math.Min(255, green));
                    pixels[i + 2] = (byte)Math.Max(0, Math.Min(255, red));
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    int gray = pixels[i] + (int)NextGaussian(0, standardDeviation);
                    pixels[i] = (byte)Math.Max(0, Math.Min(255, gray));
                }
            }

            // Convert back to BitmapImage
            BitmapSource bitmapSource = BitmapSource.Create(
                image.PixelWidth,
                image.PixelHeight,
                image.DpiX,
                image.DpiY,
                image.Format,
                null,
                pixels,
                stride);

            // Convert BitmapSource to BitmapImage
            BitmapImage result = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memory);
                memory.Position = 0;

                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = memory;
                result.EndInit();
                result.Freeze(); // Makes the image WPF-thread-safe
            }

            return result;
        }
        public static BitmapImage ApplySaltAndPepperNoise(BitmapImage image, double probability)
        {
            Random random = new Random();

            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    if (random.NextDouble() < probability)
                    {
                        byte noiseValue = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                        pixels[i] = noiseValue;     // Blue
                        pixels[i + 1] = noiseValue; // Green
                        pixels[i + 2] = noiseValue; // Red
                                                    // Alpha remains unchanged
                    }
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int i = 0; i < pixels.Length; i += 3)
                {
                    if (random.NextDouble() < probability)
                    {
                        byte noiseValue = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                        pixels[i] = noiseValue;     // Blue
                        pixels[i + 1] = noiseValue; // Green
                        pixels[i + 2] = noiseValue; // Red
                    }
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (random.NextDouble() < probability)
                    {
                        pixels[i] = random.NextDouble() < 0.5 ? (byte)0 : (byte)255;
                    }
                }
            }

            // Convert back to BitmapImage
            BitmapSource bitmapSource = BitmapSource.Create(
                image.PixelWidth,
                image.PixelHeight,
                image.DpiX,
                image.DpiY,
                image.Format,
                null,
                pixels,
                stride);

            // Convert BitmapSource to BitmapImage
            BitmapImage result = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memory);
                memory.Position = 0;

                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = memory;
                result.EndInit();
                result.Freeze(); // Makes the image WPF-thread-safe
            }

            return result;
        }

        public static BitmapImage ApplyPoissonNoise(BitmapImage image, double intensity = 1.0)
        {
            Random random = new Random();

            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            byte[] resultPixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int i = 0; i < pixels.Length; i += 4)
                {
                    // Generate Poisson noise for each channel
                    resultPixels[i] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i], intensity, random)));
                    resultPixels[i + 1] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i + 1], intensity, random)));
                    resultPixels[i + 2] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i + 2], intensity, random)));
                    resultPixels[i + 3] = pixels[i + 3]; // Keep alpha channel unchanged
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int i = 0; i < pixels.Length; i += 3)
                {
                    resultPixels[i] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i], intensity, random)));
                    resultPixels[i + 1] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i + 1], intensity, random)));
                    resultPixels[i + 2] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i + 2], intensity, random)));
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int i = 0; i < pixels.Length; i++)
                {
                    resultPixels[i] = (byte)Math.Max(0, Math.Min(255,
                        GeneratePoissonNoise(pixels[i], intensity, random)));
                }
            }

            // Convert back to BitmapImage
            BitmapSource bitmapSource = BitmapSource.Create(
                image.PixelWidth,
                image.PixelHeight,
                image.DpiX,
                image.DpiY,
                image.Format,
                null,
                resultPixels,
                stride);

            BitmapImage result = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
                encoder.Save(memory);
                memory.Position = 0;

                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = memory;
                result.EndInit();
                result.Freeze();
            }

            return result;
        }

        private static double GeneratePoissonNoise(double lambda, double intensity, Random random)
        {
            // Scale lambda by intensity
            lambda = Math.Max(0.01, lambda * intensity / 255.0);

            if (lambda < 30.0)
            {
                // Use direct method for small lambda
                double L = Math.Exp(-lambda);
                double p = 1.0;
                int k = 0;

                do
                {
                    k++;
                    p *= random.NextDouble();
                }
                while (p > L);

                return ((k - 1) * 255.0 / intensity);
            }
            else
            {
                // Use normal approximation for large lambda
                double u1 = random.NextDouble();
                double u2 = random.NextDouble();

                // Box-Muller transform
                double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

                // Transform to Poisson
                return (lambda + Math.Sqrt(lambda) * z) * 255.0 / intensity;
            }
        }

    }
}
