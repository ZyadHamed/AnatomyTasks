using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Task5.ImageHandlers;
using System.IO;

namespace Task5.ImageHandlers
{
    public class ImageDenoiser
    {
        public static BitmapImage ApplyMeanDenoising(BitmapImage image, int windowSize = 3)
        {
            // Ensure window size is odd
            if (windowSize % 2 == 0)
                windowSize++;

            int radius = windowSize / 2;

            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            byte[] resultPixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int y = 0; y < image.PixelHeight; y++)
                {
                    for (int x = 0; x < image.PixelWidth; x++)
                    {
                        int pixelOffset = (y * stride) + (x * 4);
                        double sumB = 0, sumG = 0, sumR = 0;
                        int count = 0;

                        // Process neighborhood
                        for (int wy = -radius; wy <= radius; wy++)
                        {
                            for (int wx = -radius; wx <= radius; wx++)
                            {
                                int newX = x + wx;
                                int newY = y + wy;

                                // Check boundaries
                                if (newX >= 0 && newX < image.PixelWidth &&
                                    newY >= 0 && newY < image.PixelHeight)
                                {
                                    int neighborOffset = (newY * stride) + (newX * 4);
                                    sumB += pixels[neighborOffset];
                                    sumG += pixels[neighborOffset + 1];
                                    sumR += pixels[neighborOffset + 2];
                                    count++;
                                }
                            }
                        }

                        // Calculate average
                        resultPixels[pixelOffset] = (byte)(sumB / count);     // Blue
                        resultPixels[pixelOffset + 1] = (byte)(sumG / count); // Green
                        resultPixels[pixelOffset + 2] = (byte)(sumR / count); // Red
                        resultPixels[pixelOffset + 3] = pixels[pixelOffset + 3]; // Copy alpha
                    }
                }
            }
            else if (image.Format == PixelFormats.Bgr24)
            {
                for (int y = 0; y < image.PixelHeight; y++)
                {
                    for (int x = 0; x < image.PixelWidth; x++)
                    {
                        int pixelOffset = (y * stride) + (x * 3);
                        double sumB = 0, sumG = 0, sumR = 0;
                        int count = 0;

                        for (int wy = -radius; wy <= radius; wy++)
                        {
                            for (int wx = -radius; wx <= radius; wx++)
                            {
                                int newX = x + wx;
                                int newY = y + wy;

                                if (newX >= 0 && newX < image.PixelWidth &&
                                    newY >= 0 && newY < image.PixelHeight)
                                {
                                    int neighborOffset = (newY * stride) + (newX * 3);
                                    sumB += pixels[neighborOffset];
                                    sumG += pixels[neighborOffset + 1];
                                    sumR += pixels[neighborOffset + 2];
                                    count++;
                                }
                            }
                        }

                        resultPixels[pixelOffset] = (byte)(sumB / count);
                        resultPixels[pixelOffset + 1] = (byte)(sumG / count);
                        resultPixels[pixelOffset + 2] = (byte)(sumR / count);
                    }
                }
            }
            else if (image.Format == PixelFormats.Gray8)
            {
                for (int y = 0; y < image.PixelHeight; y++)
                {
                    for (int x = 0; x < image.PixelWidth; x++)
                    {
                        int pixelOffset = (y * stride) + x;
                        double sum = 0;
                        int count = 0;

                        for (int wy = -radius; wy <= radius; wy++)
                        {
                            for (int wx = -radius; wx <= radius; wx++)
                            {
                                int newX = x + wx;
                                int newY = y + wy;

                                if (newX >= 0 && newX < image.PixelWidth &&
                                    newY >= 0 && newY < image.PixelHeight)
                                {
                                    int neighborOffset = (newY * stride) + newX;
                                    sum += pixels[neighborOffset];
                                    count++;
                                }
                            }
                        }

                        resultPixels[pixelOffset] = (byte)(sum / count);
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
        public static BitmapImage ApplyWeightedMedianDenoising(BitmapImage image, int windowSize = 3)
        {
            // Ensure window size is odd
            if (windowSize % 2 == 0)
                windowSize++;

            int radius = windowSize / 2;

            // Create weight matrix (higher weights for center pixels)
            double[,] weights = GenerateWeightMatrix(windowSize);

            // Get the image format details
            int stride = (image.PixelWidth * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[image.PixelHeight * stride];
            byte[] resultPixels = new byte[image.PixelHeight * stride];

            // Copy the pixels into the array
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                for (int y = 0; y < image.PixelHeight; y++)
                {
                    for (int x = 0; x < image.PixelWidth; x++)
                    {
                        int pixelOffset = (y * stride) + (x * 4);

                        List<WeightedPixel> blueValues = new List<WeightedPixel>();
                        List<WeightedPixel> greenValues = new List<WeightedPixel>();
                        List<WeightedPixel> redValues = new List<WeightedPixel>();

                        // Process neighborhood
                        for (int wy = -radius; wy <= radius; wy++)
                        {
                            for (int wx = -radius; wx <= radius; wx++)
                            {
                                int newX = x + wx;
                                int newY = y + wy;

                                // Check boundaries
                                if (newX >= 0 && newX < image.PixelWidth &&
                                    newY >= 0 && newY < image.PixelHeight)
                                {
                                    int neighborOffset = (newY * stride) + (newX * 4);
                                    double weight = weights[wy + radius, wx + radius];

                                    blueValues.Add(new WeightedPixel(pixels[neighborOffset], weight));
                                    greenValues.Add(new WeightedPixel(pixels[neighborOffset + 1], weight));
                                    redValues.Add(new WeightedPixel(pixels[neighborOffset + 2], weight));
                                }
                            }
                        }

                        // Calculate weighted median for each channel
                        resultPixels[pixelOffset] = GetWeightedMedian(blueValues);
                        resultPixels[pixelOffset + 1] = GetWeightedMedian(greenValues);
                        resultPixels[pixelOffset + 2] = GetWeightedMedian(redValues);
                        resultPixels[pixelOffset + 3] = pixels[pixelOffset + 3]; // Copy alpha
                    }
                }
            }
            // Similar blocks for BGR24 and Gray8 formats...

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

        private class WeightedPixel
        {
            public byte Value { get; set; }
            public double Weight { get; set; }

            public WeightedPixel(byte value, double weight)
            {
                Value = value;
                Weight = weight;
            }
        }

        private static double[,] GenerateWeightMatrix(int size)
        {
            double[,] weights = new double[size, size];
            int radius = size / 2;
            double sigma = radius / 2.0;

            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    // Gaussian weight
                    double weight = Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
                    weights[y + radius, x + radius] = weight;
                }
            }

            return weights;
        }

        private static byte GetWeightedMedian(List<WeightedPixel> weightedValues)
        {
            // Sort by value
            weightedValues.Sort((a, b) => a.Value.CompareTo(b.Value));

            // Calculate total weight
            double totalWeight = weightedValues.Sum(wp => wp.Weight);
            double medianWeight = totalWeight / 2;

            // Find weighted median
            double currentWeight = 0;
            foreach (var wp in weightedValues)
            {
                currentWeight += wp.Weight;
                if (currentWeight >= medianWeight)
                    return wp.Value;
            }

            return weightedValues.Last().Value;
        }

        public static BitmapImage ApplyWaveletDenoising(BitmapImage image, int levels = 3, double threshold = 30.0)
        {
            // Get image data
            int width = image.PixelWidth;
            int height = image.PixelHeight;
            int stride = (width * image.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[height * stride];
            image.CopyPixels(pixels, stride, 0);

            if (image.Format == PixelFormats.Bgra32 || image.Format == PixelFormats.Bgr32)
            {
                // Process each channel separately
                double[,] blueChannel = new double[height, width];
                double[,] greenChannel = new double[height, width];
                double[,] redChannel = new double[height, width];

                // Separate channels
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = (y * stride) + (x * 4);
                        blueChannel[y, x] = pixels[i];
                        greenChannel[y, x] = pixels[i + 1];
                        redChannel[y, x] = pixels[i + 2];
                    }
                }

                // Apply wavelet denoising to each channel
                blueChannel = WaveletDenoise(blueChannel, levels, threshold);
                greenChannel = WaveletDenoise(greenChannel, levels, threshold);
                redChannel = WaveletDenoise(redChannel, levels, threshold);

                // Combine channels
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = (y * stride) + (x * 4);
                        pixels[i] = (byte)Math.Max(0, Math.Min(255, blueChannel[y, x]));
                        pixels[i + 1] = (byte)Math.Max(0, Math.Min(255, greenChannel[y, x]));
                        pixels[i + 2] = (byte)Math.Max(0, Math.Min(255, redChannel[y, x]));
                        // Alpha channel remains unchanged
                    }
                }
            }

            // Convert back to BitmapImage
            BitmapSource bitmapSource = BitmapSource.Create(
                width,
                height,
                image.DpiX,
                image.DpiY,
                image.Format,
                null,
                pixels,
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

        private static double[,] WaveletDenoise(double[,] data, int levels, double threshold)
        {
            int height = data.GetLength(0);
            int width = data.GetLength(1);
            double[,] coefficients = new double[height, width];
            Array.Copy(data, coefficients, data.Length);

            // Forward wavelet transform
            for (int level = 0; level < levels; level++)
            {
                int levelHeight = height >> level;
                int levelWidth = width >> level;

                // Horizontal transform
                for (int y = 0; y < levelHeight; y++)
                {
                    for (int x = 0; x < levelWidth; x += 2)
                    {
                        double a = coefficients[y, x];
                        double b = coefficients[y, x + 1];
                        coefficients[y, x / 2] = (a + b) / Math.Sqrt(2);
                        coefficients[y, levelWidth / 2 + x / 2] = (a - b) / Math.Sqrt(2);
                    }
                }

                // Vertical transform
                for (int x = 0; x < levelWidth; x++)
                {
                    for (int y = 0; y < levelHeight; y += 2)
                    {
                        double a = coefficients[y, x];
                        double b = coefficients[y + 1, x];
                        coefficients[y / 2, x] = (a + b) / Math.Sqrt(2);
                        coefficients[levelHeight / 2 + y / 2, x] = (a - b) / Math.Sqrt(2);
                    }
                }
            }

            // Apply thresholding to wavelet coefficients
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Skip the approximation coefficients (lowest frequency)
                    if (y < height >> levels && x < width >> levels)
                        continue;

                    // Soft thresholding
                    double value = coefficients[y, x];
                    double absValue = Math.Abs(value);
                    if (absValue <= threshold)
                        coefficients[y, x] = 0;
                    else
                        coefficients[y, x] = Math.Sign(value) * (absValue - threshold);
                }
            }

            // Inverse wavelet transform
            for (int level = levels - 1; level >= 0; level--)
            {
                int levelHeight = height >> level;
                int levelWidth = width >> level;

                // Vertical inverse transform
                for (int x = 0; x < levelWidth; x++)
                {
                    for (int y = 0; y < levelHeight / 2; y++)
                    {
                        double a = coefficients[y, x];
                        double b = coefficients[levelHeight / 2 + y, x];
                        coefficients[2 * y, x] = (a + b) / Math.Sqrt(2);
                        coefficients[2 * y + 1, x] = (a - b) / Math.Sqrt(2);
                    }
                }

                // Horizontal inverse transform
                for (int y = 0; y < levelHeight; y++)
                {
                    for (int x = 0; x < levelWidth / 2; x++)
                    {
                        double a = coefficients[y, x];
                        double b = coefficients[y, levelWidth / 2 + x];
                        coefficients[y, 2 * x] = (a + b) / Math.Sqrt(2);
                        coefficients[y, 2 * x + 1] = (a - b) / Math.Sqrt(2);
                    }
                }
            }

            return coefficients;
        }
        public static BitmapImage ApplyWienerDenoising(BitmapImage sourceImage, int windowSize = 3, double noiseVariance = 10)
        {
            // Get source pixels
            int width = sourceImage.PixelWidth;
            int height = sourceImage.PixelHeight;
            int stride = (width * sourceImage.Format.BitsPerPixel + 7) / 8;
            byte[] pixels = new byte[height * stride];
            sourceImage.CopyPixels(pixels, stride, 0);

            // Create result array
            byte[] result = new byte[height * stride];
            int padding = windowSize / 2;

            // Process each channel in the image
            Parallel.For(0, height, y =>
            {
                for (int x = 0; x < width; x++)
                {
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int pixelOffset = (y * stride) + (x * 4) + channel;

                        // Calculate local mean and variance
                        double localMean = 0;
                        double localVariance = 0;
                        int count = 0;

                        // Gather neighborhood pixels
                        for (int wy = -padding; wy <= padding; wy++)
                        {
                            int ny = y + wy;
                            if (ny < 0 || ny >= height) continue;

                            for (int wx = -padding; wx <= padding; wx++)
                            {
                                int nx = x + wx;
                                if (nx < 0 || nx >= width) continue;

                                int neighborOffset = (ny * stride) + (nx * 4) + channel;
                                localMean += pixels[neighborOffset];
                                count++;
                            }
                        }

                        localMean /= count;

                        // Calculate variance
                        for (int wy = -padding; wy <= padding; wy++)
                        {
                            int ny = y + wy;
                            if (ny < 0 || ny >= height) continue;

                            for (int wx = -padding; wx <= padding; wx++)
                            {
                                int nx = x + wx;
                                if (nx < 0 || nx >= width) continue;

                                int neighborOffset = (ny * stride) + (nx * 4) + channel;
                                double diff = pixels[neighborOffset] - localMean;
                                localVariance += diff * diff;
                            }
                        }

                        localVariance /= count;

                        // Apply Wiener filter
                        double maxVariance = Math.Max(0, localVariance - noiseVariance);
                        double h = maxVariance / (maxVariance + noiseVariance);
                        double filteredValue = localMean + h * (pixels[pixelOffset] - localMean);

                        // Store result
                        result[pixelOffset] = (byte)Math.Max(0, Math.Min(255, filteredValue));
                    }

                    // Copy alpha channel
                    int alphaOffset = (y * stride) + (x * 4) + 3;
                    result[alphaOffset] = pixels[alphaOffset];
                }
            });

            // Create result BitmapImage
            WriteableBitmap writeableBmp = new WriteableBitmap(width, height, 96, 96,
                PixelFormats.Bgra32, null);
            writeableBmp.WritePixels(new Int32Rect(0, 0, width, height), result, stride, 0);

            // Convert to BitmapImage
            BitmapImage outputImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writeableBmp));
                encoder.Save(stream);
                stream.Position = 0;

                outputImage.BeginInit();
                outputImage.CacheOption = BitmapCacheOption.OnLoad;
                outputImage.StreamSource = stream;
                outputImage.EndInit();
                outputImage.Freeze();
            }

            return outputImage;
        }
    }
}
