using System;
using System.Collections.Generic;
using System.Linq;
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
using Task5.ImageHandlers;

namespace Task5
{
    /// <summary>
    /// Interaction logic for ZoomWindow.xaml
    /// </summary>
    public partial class ZoomWindow : Window
    {
        public ZoomWindow(BitmapImage image)
        {
            InitializeComponent();
            LoadImage(image);
        }
        private BitmapImage originalImage;
        private double currentScale = 1.0;
        private ScaleType currentScaleType = ScaleType.Bilinear;

        public enum ScaleType
        {
            NearestNeighbor,
            Linear,
            Bilinear,
            Bicubic
        }

        private void MainImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (originalImage == null) return;

            // Get mouse position relative to the image
            Point mousePos = e.GetPosition(MainImage);

            // Calculate new scale
            if (e.Delta > 0)
                currentScale *= 1.2; // Zoom in
            else
                currentScale /= 1.2; // Zoom out

            // Limit the scale
            currentScale = Math.Max(0.1, Math.Min(10.0, currentScale));

            // Apply scaling based on selected method
            BitmapImage scaledImage = null;
            switch (currentScaleType)
            {
                case ScaleType.NearestNeighbor:
                    scaledImage = ZoomManager.ScaleNearestNeighbor(originalImage, currentScale, currentScale);
                    break;
                case ScaleType.Linear:
                    scaledImage = ZoomManager.ScaleLinear(originalImage, currentScale, currentScale);
                    break;
                case ScaleType.Bilinear:
                    scaledImage = ZoomManager.ScaleBilinear(originalImage, currentScale, currentScale);
                    break;
                case ScaleType.Bicubic:
                    scaledImage = ZoomManager.ScaleBicubic(originalImage, currentScale, currentScale);
                    break;
            }

            if (scaledImage != null)
            {
                MainImage.Source = scaledImage;

                // Adjust scroll position to keep mouse point fixed
                double scrollX = mousePos.X * currentScale - ImageScrollViewer.ViewportWidth / 2;
                double scrollY = mousePos.Y * currentScale - ImageScrollViewer.ViewportHeight / 2;

                ImageScrollViewer.ScrollToHorizontalOffset(scrollX);
                ImageScrollViewer.ScrollToVerticalOffset(scrollY);
            }
        }

        // Method to load image
        public void LoadImage(BitmapImage image)
        {
            originalImage = image;
            MainImage.Source = originalImage;
            currentScale = 1.0;
        }

        // Method to change scaling type
        public void ChangeScalingMethod(ScaleType newType)
        {
            currentScaleType = newType;
            if (currentScale != 1.0 && originalImage != null)
            {
                // Reapply current scale with new method
                BitmapImage scaledImage = null;
                switch (currentScaleType)
                {
                    case ScaleType.NearestNeighbor:
                        scaledImage = ZoomManager.ScaleNearestNeighbor(originalImage, currentScale, currentScale);
                        break;
                    case ScaleType.Linear:
                        scaledImage = ZoomManager.ScaleLinear(originalImage, currentScale, currentScale);
                        break;
                    case ScaleType.Bilinear:
                        scaledImage = ZoomManager.ScaleBilinear(originalImage, currentScale, currentScale);
                        break;
                    case ScaleType.Bicubic:
                        scaledImage = ZoomManager.ScaleBicubic(originalImage, currentScale, currentScale);
                        break;
                }

                if (scaledImage != null)
                {
                    MainImage.Source = scaledImage;
                }
            }
        }
        private void btnZoomLinearInterpolation_Click(object sender, RoutedEventArgs e)
        {
            ChangeScalingMethod(ScaleType.Linear);
        }

        private void btnZoomBilinearInterpolation_Click(object sender, RoutedEventArgs e)
        {
            ChangeScalingMethod(ScaleType.Bilinear);
        }

        private void btnZoomNearstNeighnourInterpolation_Click(object sender, RoutedEventArgs e)
        {
            ChangeScalingMethod(ScaleType.NearestNeighbor);
        }

        private void btnZoomCubicInterpolation_Click(object sender, RoutedEventArgs e)
        {
            ChangeScalingMethod(ScaleType.Bicubic);
        }
    }
}
