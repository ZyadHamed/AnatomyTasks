using AnatomyTask1.Objects;
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
    /// Interaction logic for PropertiesWindow.xaml
    /// </summary>
    public partial class PropertiesWindow : Window
    {
        DicomFileHandler fileHandler;
        List<DicomTagDTO> data;
        public PropertiesWindow(string filePath)
        {
            InitializeComponent();
            fileHandler = new DicomFileHandler(filePath);
            data = fileHandler.GetAllProperties();
            var organizedData = DicomCategorizer.CategorizeTags(data);
            DicomDataGrid.ItemsSource = data;
            PopulateDicomTags(organizedData);
        }

        public void PopulateDicomTags(Dictionary<string, List<DicomTagDTO>> categorizedTags)
        {
            // Clear any existing children
            categorizationPanel.Children.Clear();

            foreach (var category in categorizedTags)
            {
                // Add category label
                var categoryLabel = new Label
                {
                    Content = category.Key,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(5)
                };
                categorizationPanel.Children.Add(categoryLabel);

                // Add tags under the category
                foreach (var tag in category.Value)
                {
                    var tagText = new TextBlock
                    {
                        Text = $"{tag.TagName}: {tag.Value}",
                        Margin = new Thickness(10, 2, 10, 2)
                    };
                    categorizationPanel.Children.Add(tagText);
                }
            }
        }

        private void DicomDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Value") // Ensure it's the Value column
            {
                // Get the edited DicomTag
                var editedTag = e.Row.Item as DicomTagDTO;

                // Extract the new value from the editing element (TextBox)
                if (e.EditingElement is TextBox textBox)
                {
                    string newValue = textBox.Text;
                    fileHandler.UpdateTag(FellowOakDicom.DicomTag.Parse(editedTag.TagID), newValue);
                    fileHandler.SaveFile();
                }
            }
        }

        private void btnAnonymize_Click(object sender, RoutedEventArgs e)
        {
            TextInputWindow textInputWindow = new TextInputWindow();
            if (textInputWindow.ShowDialog() == true)
            {
                string prefix = textInputWindow.prefix;
                textInputWindow.Close();
                fileHandler.AnonymizeData(prefix);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                if(txtSearch.Text == "")
                {
                    DicomDataGrid.ItemsSource = data;
                }
                else
                {
                    List<DicomTagDTO> filteredData = data.Where(tag => tag.TagName.Contains(txtSearch.Text)).ToList();
                    DicomDataGrid.ItemsSource = filteredData;
                }
                

            }
        }
    }
}
