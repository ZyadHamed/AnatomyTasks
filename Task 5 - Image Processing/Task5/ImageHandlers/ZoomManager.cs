using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace Task5.ImageHandlers
{
    public class ZoomManager
    {
            // Nearest Neighbor Interpolation
            public static BitmapImage ScaleNearestNeighbor(BitmapImage source, double scaleX, double scaleY)
            {
                int sourceWidth = source.PixelWidth;
                int sourceHeight = source.PixelHeight;
                int targetWidth = (int)(sourceWidth * scaleX);
                int targetHeight = (int)(sourceHeight * scaleY);

                int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
                int targetStride = (targetWidth * source.Format.BitsPerPixel + 7) / 8;

                byte[] sourcePixels = new byte[sourceHeight * sourceStride];
                byte[] targetPixels = new byte[targetHeight * targetStride];

                source.CopyPixels(sourcePixels, sourceStride, 0);

                Parallel.For(0, targetHeight, y =>
                {
                    int sourceY = (int)(y / scaleY);
                    sourceY = Math.Min(sourceY, sourceHeight - 1);

                    for (int x = 0; x < targetWidth; x++)
                    {
                        int sourceX = (int)(x / scaleX);
                        sourceX = Math.Min(sourceX, sourceWidth - 1);

                        int sourceIndex = (sourceY * sourceStride) + (sourceX * 4);
                        int targetIndex = (y * targetStride) + (x * 4);

                        // Copy BGRA values
                        for (int i = 0; i < 4; i++)
                        {
                            targetPixels[targetIndex + i] = sourcePixels[sourceIndex + i];
                        }
                    }
                });

                return CreateBitmapImage(targetPixels, targetWidth, targetHeight, targetStride);
            }

            // Bilinear Interpolation
            public static BitmapImage ScaleBilinear(BitmapImage source, double scaleX, double scaleY)
            {
                int sourceWidth = source.PixelWidth;
                int sourceHeight = source.PixelHeight;
                int targetWidth = (int)(sourceWidth * scaleX);
                int targetHeight = (int)(sourceHeight * scaleY);

                int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
                int targetStride = (targetWidth * source.Format.BitsPerPixel + 7) / 8;

                byte[] sourcePixels = new byte[sourceHeight * sourceStride];
                byte[] targetPixels = new byte[targetHeight * targetStride];

                source.CopyPixels(sourcePixels, sourceStride, 0);

                Parallel.For(0, targetHeight, y =>
                {
                    double sourceY = y / scaleY;
                    int y1 = (int)sourceY;
                    int y2 = Math.Min(y1 + 1, sourceHeight - 1);
                    double dy = sourceY - y1;

                    for (int x = 0; x < targetWidth; x++)
                    {
                        double sourceX = x / scaleX;
                        int x1 = (int)sourceX;
                        int x2 = Math.Min(x1 + 1, sourceWidth - 1);
                        double dx = sourceX - x1;

                        int targetIndex = (y * targetStride) + (x * 4);

                        for (int c = 0; c < 4; c++)
                        {
                            double p1 = sourcePixels[(y1 * sourceStride) + (x1 * 4) + c];
                            double p2 = sourcePixels[(y1 * sourceStride) + (x2 * 4) + c];
                            double p3 = sourcePixels[(y2 * sourceStride) + (x1 * 4) + c];
                            double p4 = sourcePixels[(y2 * sourceStride) + (x2 * 4) + c];

                            // Bilinear interpolation
                            double value = p1 * (1 - dx) * (1 - dy) +
                                         p2 * dx * (1 - dy) +
                                         p3 * (1 - dx) * dy +
                                         p4 * dx * dy;

                            targetPixels[targetIndex + c] = (byte)Math.Max(0, Math.Min(255, value));
                        }
                    }
                });

                return CreateBitmapImage(targetPixels, targetWidth, targetHeight, targetStride);
            }

            // Linear Interpolation
            public static BitmapImage ScaleLinear(BitmapImage source, double scaleX, double scaleY)
            {
                int sourceWidth = source.PixelWidth;
                int sourceHeight = source.PixelHeight;
                int targetWidth = (int)(sourceWidth * scaleX);
                int targetHeight = (int)(sourceHeight * scaleY);

                int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
                int targetStride = (targetWidth * source.Format.BitsPerPixel + 7) / 8;

                byte[] sourcePixels = new byte[sourceHeight * sourceStride];
                byte[] targetPixels = new byte[targetHeight * targetStride];

                source.CopyPixels(sourcePixels, sourceStride, 0);

                Parallel.For(0, targetHeight, y =>
                {
                    double sourceY = y / scaleY;
                    int y1 = (int)sourceY;
                    int y2 = Math.Min(y1 + 1, sourceHeight - 1);
                    double dy = sourceY - y1;

                    for (int x = 0; x < targetWidth; x++)
                    {
                        double sourceX = x / scaleX;
                        int x1 = (int)sourceX;
                        double dx = sourceX - x1;

                        int targetIndex = (y * targetStride) + (x * 4);

                        for (int c = 0; c < 4; c++)
                        {
                            double p1 = sourcePixels[(y1 * sourceStride) + (x1 * 4) + c];
                            double p2 = sourcePixels[(y2 * sourceStride) + (x1 * 4) + c];

                            // Linear interpolation
                            double value = p1 * (1 - dy) + p2 * dy;

                            targetPixels[targetIndex + c] = (byte)Math.Max(0, Math.Min(255, value));
                        }
                    }
                });

                return CreateBitmapImage(targetPixels, targetWidth, targetHeight, targetStride);
            }

            // Bicubic Interpolation
            public static BitmapImage ScaleBicubic(BitmapImage source, double scaleX, double scaleY)
            {
                int sourceWidth = source.PixelWidth;
                int sourceHeight = source.PixelHeight;
                int targetWidth = (int)(sourceWidth * scaleX);
                int targetHeight = (int)(sourceHeight * scaleY);

                int sourceStride = (sourceWidth * source.Format.BitsPerPixel + 7) / 8;
                int targetStride = (targetWidth * source.Format.BitsPerPixel + 7) / 8;

                byte[] sourcePixels = new byte[sourceHeight * sourceStride];
                byte[] targetPixels = new byte[targetHeight * targetStride];

                source.CopyPixels(sourcePixels, sourceStride, 0);

                Parallel.For(0, targetHeight, y =>
                {
                    double sourceY = y / scaleY;
                    int y0 = Math.Max((int)sourceY - 1, 0);
                    int y1 = (int)sourceY;
                    int y2 = Math.Min(y1 + 1, sourceHeight - 1);
                    int y3 = Math.Min(y1 + 2, sourceHeight - 1);
                    double dy = sourceY - y1;

                    for (int x = 0; x < targetWidth; x++)
                    {
                        double sourceX = x / scaleX;
                        int x0 = Math.Max((int)sourceX - 1, 0);
                        int x1 = (int)sourceX;
                        int x2 = Math.Min(x1 + 1, sourceWidth - 1);
                        int x3 = Math.Min(x1 + 2, sourceWidth - 1);
                        double dx = sourceX - x1;

                        int targetIndex = (y * targetStride) + (x * 4);

                        for (int c = 0; c < 4; c++)
                        {
                            double[] points = new double[4];
                            for (int i = 0; i < 4; i++)
                            {
                                int yi = i == 0 ? y0 : (i == 1 ? y1 : (i == 2 ? y2 : y3));
                                points[i] = CubicInterpolate(
                                    sourcePixels[(yi * sourceStride) + (x0 * 4) + c],
                                    sourcePixels[(yi * sourceStride) + (x1 * 4) + c],
                                    sourcePixels[(yi * sourceStride) + (x2 * 4) + c],
                                    sourcePixels[(yi * sourceStride) + (x3 * 4) + c],
                                    dx);
                            }

                            double value = CubicInterpolate(points[0], points[1], points[2], points[3], dy);
                            targetPixels[targetIndex + c] = (byte)Math.Max(0, Math.Min(255, value));
                        }
                    }
                });

                return CreateBitmapImage(targetPixels, targetWidth, targetHeight, targetStride);
            }

            private static double CubicInterpolate(double v0, double v1, double v2, double v3, double t)
            {
                double a0 = v3 - v2 - v0 + v1;
                double a1 = v0 - v1 - a0;
                double a2 = v2 - v0;
                double a3 = v1;

                return a0 * t * t * t + a1 * t * t + a2 * t + a3;
            }

            private static BitmapImage CreateBitmapImage(byte[] pixels, int width, int height, int stride)
            {
                WriteableBitmap writeableBmp = new WriteableBitmap(width, height, 96, 96,
                    PixelFormats.Bgra32, null);
                writeableBmp.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

                BitmapImage result = new BitmapImage();
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(writeableBmp));
                    encoder.Save(stream);
                    stream.Position = 0;

                    result.BeginInit();
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = stream;
                    result.EndInit();
                    result.Freeze();
                }

                return result;
            }
    }
}
