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

namespace AnatomyTask1
{
    /// <summary>
    /// Interaction logic for TilesWindow.xaml
    /// </summary>
    public partial class TilesWindow : Window
    {
        public TilesWindow(List<WriteableBitmap> images)
        {
            InitializeComponent();
            DisplayImages(images);
        }
        public void DisplayImages(List<WriteableBitmap> images)
        {
            // Calculate the optimal number of columns for a square-like grid
            //int columns = (int)Math.Ceiling(Math.Sqrt(images.Count));
               int columns = 4;

            // Clear existing content
            imagesGrid.Children.Clear();
            imagesGrid.Columns = columns;

            foreach (var image in images)
            {
                // Create image control
                var imageControl = new Image
                {
                    Source = image,
                    Stretch = Stretch.Uniform,
                    Margin = new Thickness(5)
                };


                imagesGrid.Children.Add(imageControl);
            }
        }
    }
}
