using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Task5.ImageHandlers;

namespace Task5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BitmapImage inputImage;
        BitmapImage outputPort1Image;
        BitmapImage outputPort2Image;

        BitmapImage ROI1;
        BitmapImage ROI2;
        BitmapImage Noise;

        Rectangle roi1Rectangle;
        Rectangle roi2Rectangle;
        Rectangle noiseRectangle;

        int selectedROISize = 16;
        bool selectSNRROISignal = false;
        bool selectSNRROINoise = false;

        bool selectCNRROI1Signal = false;
        bool selectCNRROI2Signal = false;
        bool selectCNRROINoise = false;

        double noisingValue = 0;

        string selectedOutputPort;
        string selectedInputPort;

        public MainWindow()
        {
            InitializeComponent();
        }

        void ClearROI()
        {
            imgInputOverlayCanvas.Children.Clear();
            imgOutput1OverlayCanvas.Children.Clear();
            imgOutput2OverlayCanvas.Children.Clear();
        }

        private void btnChooseImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp|All files (*.*)|*.*";
            dialog.FilterIndex = 1;
            if (dialog.ShowDialog() == true)
            {
                string imgPath = dialog.FileName;
                inputImage = new BitmapImage(new Uri(imgPath));
                imgInput.Source = inputImage;
                ClearROI();

            }
        }

        private void btnSelectROISNR_Click(object sender, RoutedEventArgs e)
        {
            if (selectSNRROISignal == false)
            {
                selectSNRROISignal = true;
                btnSelectROISNR.Content = "Cancel";
            }
            else
            {
                selectSNRROISignal = false;
                btnSelectROISNR.Content = "Select ROI";
            }
        }

        private void btnSelectNoiseSNR_Click(object sender, RoutedEventArgs e)
        {
            if (selectSNRROINoise == false)
            {
                selectSNRROINoise = true;
                btnSelectNoiseSNR.Content = "Cancel";
            }
            else
            {
                selectSNRROINoise = false;
                btnSelectNoiseSNR.Content = "Select Noise";
            }
        }

        private void btnCalculateSNR_Click(object sender, RoutedEventArgs e)
        {
            if(ROI1 != null && Noise != null)
            {
                double SNR = SignalManager.CalculateSNR(ROI1, Noise);
                MessageBox.Show($"SNR: {SNR}");
            }
        }

        void PaintROISNR(Point clickPosition, Canvas overlayCanvas, Image imgControl, Brush color, int ROIType)
        {
            double widthRatio = overlayCanvas.ActualWidth / imgControl.Source.Width;
            double heightRatio = overlayCanvas.ActualHeight / imgControl.Source.Height;

            double ratio = Math.Min(widthRatio, heightRatio);
            // Calculate the rectangle position
            double x = (clickPosition.X - (selectedROISize / 2) * ratio);
            double y = (clickPosition.Y - (selectedROISize / 2) * ratio);

            if(ROIType == 0)
            {
                if (roi1Rectangle != null)
                {
                    overlayCanvas.Children.Remove(roi1Rectangle);
                }

                // Create a rectangle to show the ROI
                roi1Rectangle = new Rectangle
                {
                    Width = selectedROISize * ratio,
                    Height = selectedROISize * ratio,
                    Stroke = color,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                // Position the rectangle
                Canvas.SetLeft(roi1Rectangle, x);
                Canvas.SetTop(roi1Rectangle, y);

                // Clear previous rectangles if needed
                overlayCanvas.Children.Add(roi1Rectangle);
            }
            else if (ROIType == 1)
            {
                if (roi2Rectangle != null)
                {
                    overlayCanvas.Children.Remove(roi2Rectangle);
                }

                // Create a rectangle to show the ROI
                roi2Rectangle = new Rectangle
                {
                    Width = selectedROISize * ratio,
                    Height = selectedROISize * ratio,
                    Stroke = color,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                // Position the rectangle
                Canvas.SetLeft(roi2Rectangle, x);
                Canvas.SetTop(roi2Rectangle, y);

                // Clear previous rectangles if needed
                overlayCanvas.Children.Add(roi2Rectangle);
            }
            else if (ROIType == 2)
            {
                if (noiseRectangle != null)
                {
                    overlayCanvas.Children.Remove(noiseRectangle);
                }

                // Create a rectangle to show the ROI
                noiseRectangle = new Rectangle
                {
                    Width = selectedROISize * ratio,
                    Height = selectedROISize * ratio,
                    Stroke = color,
                    StrokeThickness = 1,
                    Fill = Brushes.Transparent
                };

                // Position the rectangle
                Canvas.SetLeft(noiseRectangle, x);
                Canvas.SetTop(noiseRectangle, y);

                // Clear previous rectangles if needed
                overlayCanvas.Children.Add(noiseRectangle);
            }

        }

        BitmapImage cropROI(Point clickPosition,  Image imgControl, BitmapImage image, Canvas overlayCanvas)
        {
            double scaledImageWidth = image.PixelWidth * (imgControl.ActualHeight / image.PixelHeight);
            double emptySpaceWidth = (overlayCanvas.ActualWidth - scaledImageWidth) / 2;
            double scaledImageHeight = image.PixelHeight * (imgControl.ActualHeight / image.PixelHeight);
            double emptySpaceHeight = (overlayCanvas.ActualHeight - scaledImageHeight) / 2;
            BitmapImage returnedImage = SignalManager.CropImage(image,
                (int)((clickPosition.X - emptySpaceWidth) / (overlayCanvas.ActualWidth - emptySpaceWidth * 2) * image.PixelWidth),
                (int)((clickPosition.Y - emptySpaceHeight) / (overlayCanvas.ActualHeight - emptySpaceHeight * 2) * image.PixelHeight),
                selectedROISize, selectedROISize);
            return returnedImage;
        }

        private void imgInput_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(selectSNRROISignal == true && selectSNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgInputOverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgInput, inputImage, imgInputOverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROISignal = false;
                    btnSelectROISNR.Content = "Select ROI";
                    PaintROISNR(clickPosition, imgInputOverlayCanvas, imgInput, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectSNRROISignal == false && selectSNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgInputOverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgInput, inputImage, imgInputOverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROINoise = false;
                    btnSelectNoiseSNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgInputOverlayCanvas, imgInput, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if(selectCNRROI1Signal == true && selectCNRROI2Signal == false && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgInputOverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgInput, inputImage, imgInputOverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI1Signal = false;
                    btnSelectROI1CNR.Content = "Select ROI 1";
                    PaintROISNR(clickPosition, imgInputOverlayCanvas, imgInput, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == true && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgInputOverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgInput, inputImage, imgInputOverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI2Signal = false;
                    btnSelectROI2CNR.Content = "Select ROI 2";
                    PaintROISNR(clickPosition, imgInputOverlayCanvas, imgInput, Brushes.Blue, 1);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI2 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == false && selectCNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgInputOverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgInput, inputImage, imgInputOverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROINoise = false;
                    btnSelectNoiseCNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgInputOverlayCanvas, imgInput, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
        }

        private void imgOutput2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectSNRROISignal == true && selectSNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput2OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput2, outputPort2Image, imgOutput2OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROISignal = false;
                    btnSelectROISNR.Content = "Select ROI";
                    PaintROISNR(clickPosition, imgOutput2OverlayCanvas, imgOutput2, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectSNRROISignal == false && selectSNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgOutput2OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput2, outputPort2Image, imgOutput2OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROINoise = false;
                    btnSelectNoiseSNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgOutput2OverlayCanvas, imgOutput2, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == true && selectCNRROI2Signal == false && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput2OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput2, outputPort2Image, imgOutput2OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI1Signal = false;
                    btnSelectROI1CNR.Content = "Select ROI 1";
                    PaintROISNR(clickPosition, imgOutput2OverlayCanvas, imgOutput2, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == true && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput2OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput2, outputPort2Image, imgOutput2OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI2Signal = false;
                    btnSelectROI2CNR.Content = "Select ROI 2";
                    PaintROISNR(clickPosition, imgOutput2OverlayCanvas, imgOutput2, Brushes.Blue, 1);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI2 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == false && selectCNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgOutput2OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput2, outputPort2Image, imgOutput2OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROINoise = false;
                    btnSelectNoiseCNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgOutput2OverlayCanvas, imgOutput2, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
        }

        private void imgOutput1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (selectSNRROISignal == true && selectSNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput1OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput1, outputPort1Image, imgOutput1OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROISignal = false;
                    btnSelectROISNR.Content = "Select ROI";
                    PaintROISNR(clickPosition, imgOutput1OverlayCanvas, imgOutput1, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectSNRROISignal == false && selectSNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgOutput1OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput1, outputPort1Image, imgOutput1OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectSNRROINoise = false;
                    btnSelectNoiseSNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgOutput1OverlayCanvas, imgOutput1, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == true && selectCNRROI2Signal == false && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput1OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput1, outputPort1Image, imgOutput1OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI1Signal = false;
                    btnSelectROI1CNR.Content = "Select ROI 1";
                    PaintROISNR(clickPosition, imgOutput1OverlayCanvas, imgOutput1, Brushes.Green, 0);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI1 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == true && selectCNRROINoise == false)
            {
                Point clickPosition = e.GetPosition(imgOutput1OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput1, outputPort1Image, imgOutput1OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROI2Signal = false;
                    btnSelectROI2CNR.Content = "Select ROI 2";
                    PaintROISNR(clickPosition, imgOutput1OverlayCanvas, imgOutput1, Brushes.Blue, 1);
                    SignalManager.SaveImage(image, "cropped.png");
                    ROI2 = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
            else if (selectCNRROI1Signal == false && selectCNRROI2Signal == false && selectCNRROINoise == true)
            {
                Point clickPosition = e.GetPosition(imgOutput1OverlayCanvas);
                BitmapImage image = cropROI(clickPosition, imgOutput1, outputPort1Image, imgOutput1OverlayCanvas);
                if (image == null)
                {
                    MessageBox.Show("ROI is outside image bounds!");
                }
                else
                {
                    selectCNRROINoise = false;
                    btnSelectNoiseCNR.Content = "Select Noise";
                    PaintROISNR(clickPosition, imgOutput1OverlayCanvas, imgOutput1, Brushes.Red, 2);
                    SignalManager.SaveImage(image, "cropped.png");
                    Noise = image;
                    double intensity = SignalManager.CalculateImageIntensity(image);
                    MessageBox.Show(intensity.ToString());
                }
            }
        }

        private void cmbROISize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if(((ComboBoxItem)cmbROISize.SelectedItem).Content != null)
            {
                string newValue = ((ComboBoxItem)cmbROISize.SelectedItem).Content.ToString();
                selectedROISize = int.Parse(newValue.Split("x")[0]);
            }


        }

        private void btnSelectROI1CNR_Click(object sender, RoutedEventArgs e)
        {
            if(selectCNRROI1Signal == false)
            {
                selectCNRROI1Signal = true;
                btnSelectROI1CNR.Content = "Cancel";
            }
            else
            {
                selectCNRROI1Signal = false;
                btnSelectROI1CNR.Content = "Select ROI 1";
            }

        }

        private void btnSelectROI2CNR_Click(object sender, RoutedEventArgs e)
        {
            if (selectCNRROI2Signal == false)
            {
                selectCNRROI2Signal = true;
                btnSelectROI2CNR.Content = "Cancel";
            }
            else
            {
                selectCNRROI2Signal = false;
                btnSelectROI2CNR.Content = "Select ROI 2";
            }
        }

        private void btnSelectNoiseCNR_Click(object sender, RoutedEventArgs e)
        {
            if (selectCNRROINoise == false)
            {
                selectCNRROINoise = true;
                btnSelectNoiseCNR.Content = "Cancel";
            }
            else
            {
                selectCNRROINoise = false;
                btnSelectNoiseCNR.Content = "Select Noise";
            }
        }

        private void btnCalculateCNR_Click(object sender, RoutedEventArgs e)
        {
            if (ROI1 != null && ROI2 != null && Noise != null)
            {
                double CNR = SignalManager.CalculateCNR(ROI1, ROI2, Noise);
                MessageBox.Show($"CNR: {CNR}");
            }
        }

        private void btnApplyGaussianNoise_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage noisedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                noisedImage = ImageNoiser.ApplyGaussianNoise(inputImage, noisingValue);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                noisedImage = ImageNoiser.ApplyGaussianNoise(outputPort1Image, noisingValue);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                noisedImage = ImageNoiser.ApplyGaussianNoise(outputPort2Image, noisingValue);
            }

            if(selectedOutputPort == "Input Port")
            {
                inputImage = noisedImage;
                imgInput.Source = inputImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = noisedImage;
                imgOutput1.Source = outputPort1Image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = noisedImage;
                imgOutput2.Source = outputPort2Image;
            }
        }

        private void btnApplySaltPepperNoise_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage noisedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                noisedImage = ImageNoiser.ApplySaltAndPepperNoise(inputImage, noisingValue / 1000);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                noisedImage = ImageNoiser.ApplySaltAndPepperNoise(outputPort1Image, noisingValue / 1000);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                noisedImage = ImageNoiser.ApplySaltAndPepperNoise(outputPort2Image, noisingValue / 1000);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = noisedImage;
                imgInput.Source = inputImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = noisedImage;
                imgOutput1.Source = outputPort1Image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = noisedImage;
                imgOutput2.Source = outputPort2Image;
            }
        }

        private void btnApplyPoissonNoise_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage noisedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                noisedImage = ImageNoiser.ApplyPoissonNoise(inputImage, noisingValue / 500);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                noisedImage = ImageNoiser.ApplyPoissonNoise(outputPort1Image, noisingValue / 500);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                noisedImage = ImageNoiser.ApplyPoissonNoise(outputPort2Image, noisingValue / 500);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = noisedImage;
                imgInput.Source = inputImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = noisedImage;
                imgOutput1.Source = outputPort1Image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = noisedImage;
                imgOutput2.Source = outputPort2Image;
            }
        }

        private void cmbImageInputPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)cmbImageInputPort.SelectedItem).Content != null)
            {
                string newValue = ((ComboBoxItem)cmbImageInputPort.SelectedItem).Content.ToString();
                selectedInputPort = newValue;
            }
        }

        private void cmbImageOutputPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ComboBoxItem)cmbImageOutputPort.SelectedItem).Content != null)
            {
                string newValue = ((ComboBoxItem)cmbImageOutputPort.SelectedItem).Content.ToString();
                selectedOutputPort = newValue;
            }
        }

        private void sliderNoiseDegree_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(sliderNoiseDegree.Value != null)
            {
                noisingValue = sliderNoiseDegree.Value;
            }
        }

        private void btnApplyMeanDenoising_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage denoisedImage = new BitmapImage(); 
            if (selectedInputPort == "Input Port")
            {
                denoisedImage = ImageDenoiser.ApplyMeanDenoising(inputImage, 3);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                denoisedImage = ImageDenoiser.ApplyMeanDenoising(outputPort1Image, 3);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                denoisedImage = ImageDenoiser.ApplyMeanDenoising(outputPort2Image, 3);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = denoisedImage;
                imgInput.Source = denoisedImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = denoisedImage;
                imgOutput1.Source = denoisedImage;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = denoisedImage;
                imgOutput2.Source = denoisedImage;
            }
        }

        private void btnApplySpatialFrequencyDomainDenoising_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage denoisedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                denoisedImage = ImageDenoiser.ApplyWienerDenoising(inputImage, 3);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                denoisedImage = ImageDenoiser.ApplyWienerDenoising(outputPort1Image, 3);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                denoisedImage = ImageDenoiser.ApplyWienerDenoising(outputPort2Image, 3);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = denoisedImage;
                imgInput.Source = denoisedImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = denoisedImage;
                imgOutput1.Source = denoisedImage;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = denoisedImage;
                imgOutput2.Source = denoisedImage;
            }
        }

        private void btnApplyWeightedMedianDenoising_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage denoisedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                denoisedImage = ImageDenoiser.ApplyWeightedMedianDenoising(inputImage, 3);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                denoisedImage = ImageDenoiser.ApplyWeightedMedianDenoising(outputPort1Image, 3);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                denoisedImage = ImageDenoiser.ApplyWeightedMedianDenoising(outputPort2Image, 3);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = denoisedImage;
                imgInput.Source = inputImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = denoisedImage;
                imgOutput1.Source = outputPort1Image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = denoisedImage;
                imgOutput2.Source = outputPort2Image;
            }
        }

        private void imgInput_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HistogramWindow histogramWindow = new HistogramWindow(inputImage);
            histogramWindow.DisplayHistogram();
            histogramWindow.Show();
        }

        private void imgOutput2_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HistogramWindow histogramWindow = new HistogramWindow(outputPort2Image);
            histogramWindow.DisplayHistogram();
            histogramWindow.Show();
        }

        private void imgOutput1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            HistogramWindow histogramWindow = new HistogramWindow(outputPort1Image);
            histogramWindow.DisplayHistogram();
            histogramWindow.Show();
        }

        private void btnApplyLowPassFilter_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage lowPassedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                lowPassedImage = SignalManager.ApplyLowPassFilter(inputImage);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                lowPassedImage = SignalManager.ApplyLowPassFilter(outputPort1Image);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                lowPassedImage = SignalManager.ApplyLowPassFilter(outputPort2Image);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = lowPassedImage;
                imgInput.Source = lowPassedImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = lowPassedImage;
                imgOutput1.Source = lowPassedImage;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = lowPassedImage;
                imgOutput2.Source = lowPassedImage;
            }
        }

        private void btnApplyHighPassFilter_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage highPassedImage = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                highPassedImage = SignalManager.ApplyHighPassFilter(inputImage);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                highPassedImage = SignalManager.ApplyHighPassFilter(outputPort1Image);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                highPassedImage = SignalManager.ApplyHighPassFilter(outputPort2Image);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = highPassedImage;
                imgInput.Source = highPassedImage;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = highPassedImage;
                imgOutput1.Source = highPassedImage;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = highPassedImage;
                imgOutput2.Source = highPassedImage;
            }
        }

        private void btnClearROIs_Click(object sender, RoutedEventArgs e)
        {
            ClearROI();
        }

        private void sliderContrast_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BitmapImage image = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(inputImage, sliderBrightness.Value, sliderContrast.Value);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(outputPort1Image, sliderBrightness.Value, sliderContrast.Value);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(outputPort2Image, sliderBrightness.Value, sliderContrast.Value);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = image;
                imgInput.Source = image;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = image;
                imgOutput1.Source = image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = image;
                imgOutput2.Source = image;
            }
        }

        private void sliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BitmapImage image = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(inputImage, sliderBrightness.Value, sliderContrast.Value);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(outputPort1Image, sliderBrightness.Value, sliderContrast.Value);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                image = BrightnessContrastManager.AdjustBrightnessContrast(outputPort2Image, sliderBrightness.Value, sliderContrast.Value);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = image;
                imgInput.Source = image;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = image;
                imgOutput1.Source = image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = image;
                imgOutput2.Source = image;
            }
        }

        private void btnApplyCLACHE_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                image = BrightnessContrastManager.ApplyCLAHE(inputImage);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                image = BrightnessContrastManager.ApplyCLAHE(outputPort1Image);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                image = BrightnessContrastManager.ApplyCLAHE(outputPort2Image);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = image;
                imgInput.Source = image;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = image;
                imgOutput1.Source = image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = image;
                imgOutput2.Source = image;
            }
        }

        private void btnApplyHistogramEqualization_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                image = BrightnessContrastManager.ApplyHistogramEqualization(inputImage);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                image = BrightnessContrastManager.ApplyHistogramEqualization(outputPort1Image);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                image = BrightnessContrastManager.ApplyHistogramEqualization(outputPort2Image);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = image;
                imgInput.Source = image;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = image;
                imgOutput1.Source = image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = image;
                imgOutput2.Source = image;
            }
        }

        private void btnApplyGammaCorrection_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage image = new BitmapImage();
            if (selectedInputPort == "Input Port")
            {
                image = BrightnessContrastManager.ApplyGammaCorrection(inputImage);
            }

            else if (selectedInputPort == "Output Port 1")
            {
                image = BrightnessContrastManager.ApplyGammaCorrection(outputPort1Image);
            }
            else if (selectedInputPort == "Output Port 2")
            {
                image = BrightnessContrastManager.ApplyGammaCorrection(outputPort2Image);
            }

            if (selectedOutputPort == "Input Port")
            {
                inputImage = image;
                imgInput.Source = image;
            }
            else if (selectedOutputPort == "Output Port 1")
            {
                outputPort1Image = image;
                imgOutput1.Source = image;
            }
            else if (selectedOutputPort == "Output Port 2")
            {
                outputPort2Image = image;
                imgOutput2.Source = image;
            }
        }

        private void btnOpenZoomWindow_Click(object sender, RoutedEventArgs e)
        {
            
            if (selectedInputPort == "Input Port")
            {
                ZoomWindow zoomWindow = new ZoomWindow(inputImage);
                zoomWindow.Show();
            }

            else if (selectedInputPort == "Output Port 1")
            {
                ZoomWindow zoomWindow = new ZoomWindow(outputPort1Image);
                zoomWindow.Show();
            }
            else if (selectedInputPort == "Output Port 2")
            {
                ZoomWindow zoomWindow = new ZoomWindow(outputPort2Image);
                zoomWindow.Show();
            }
        }
    }
}