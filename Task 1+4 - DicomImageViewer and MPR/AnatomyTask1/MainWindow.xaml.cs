using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using Kitware.VTK;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;
using AnatomyTask1.Objects;
using FellowOakDicom.Media;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
namespace AnatomyTask1
{
    /*
    * Hierarchy of rendering sections
    *
    * WindowsFormsHost <- RenderWindowControl <- RenderWindow <- Renderer
    * Rendrer <- VTKImageActor <- vtkImageData [Loaded from DICOM Images]
    *
    * RenderWindow <- vtkRenderWindowInteractor <- vtkInteractorStyle (vtkInteractorStyleImage)
    */

    public partial class MainWindow : Window
    {
        #region GlobalVariableDecleration

        RenderWindowControl controlSagittal = new RenderWindowControl();
        RenderWindowControl controlCoronal = new RenderWindowControl();
        RenderWindowControl controlAxial = new RenderWindowControl();
        RenderWindowControl control3D = new RenderWindowControl();
        RenderWindowControl controlImage = new RenderWindowControl();

        CTSection axialSection;
        CTSection sagittalSection;
        CTSection coronalSection;
        VolumeFactory volumeFactory;

        vtkRenderWindowInteractor interactorAxial;
        vtkRenderWindowInteractor interactorCoronal;
        vtkRenderWindowInteractor interactorSagittal;
        vtkRenderWindowInteractor interactorImage;

        vtkRenderer rendererAxial;
        vtkRenderer rendererCoronal;
        vtkRenderer rendererSagittal;
        vtkRenderer rendererImage;

        bool isSagittalPlaying = false;
        bool isCoronalPlaying = false;
        bool isAxialPlaying = false;

        bool isImageInitialized = false;
        vtkImageActor currentActor;
        VTKImage image;

        string dicomDirectory = "";
        string fileDirectory = "";
        DicomFileHandler fileHandler;
        int window;
        int level;
        #endregion
        public MainWindow()
        {
            InitializeComponent();
            //window = fileHandler.GetWindow();
            //level = fileHandler.GetLevel();
            //Create the renderer and add the image actor

        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            hostImage.Child = controlImage;
            rendererImage = new vtkRenderer();
            rendererImage.SetBackground(0.1, 0.1, 0.1);
            controlImage.RenderWindow.AddRenderer(rendererImage);

            //Set up the interactor (Responsible for brightness and contrast and zoom)
            interactorImage = new vtkRenderWindowInteractor();
            vtkInteractorStyleImage imageStyle = new vtkInteractorStyleImage();
            interactorImage.SetInteractorStyle(imageStyle);

            //Render everything
            controlImage.RenderWindow.SetInteractor(interactorImage);
            controlImage.RenderWindow.Render();
            interactorImage.Initialize();
        }
        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
        }

        void InitAxialRendering()
        {
            // Set up renderers for each slice view
            rendererAxial = new vtkRenderer();
            controlAxial.RenderWindow.AddRenderer(rendererAxial);
            rendererAxial.AddActor(axialSection.createImage());
            rendererAxial.SetBackground(0.1, 0.1, 0.1);

            interactorAxial = new vtkRenderWindowInteractor();
            interactorAxial.LeftButtonPressEvt += InteractorAxial_LeftButtonDoubleClickEvt;
            vtkInteractorStyleImage imageStyle = new vtkInteractorStyleImage();
            interactorAxial.SetInteractorStyle(imageStyle);

            controlAxial.RenderWindow.SetInteractor(interactorAxial);
            controlAxial.RenderWindow.Render();
            interactorAxial.Initialize();
        }
        void InitCoronalRendering()
        {
            rendererCoronal = new vtkRenderer();
            controlCoronal.RenderWindow.AddRenderer(rendererCoronal);
            rendererCoronal.AddActor(coronalSection.createImage());
            rendererCoronal.SetBackground(0.1, 0.1, 0.1);

            interactorCoronal = new vtkRenderWindowInteractor();
            interactorCoronal.LeftButtonPressEvt += InteractorCoronal_LeftButtonDoubleClickEvt;
            vtkInteractorStyleImage imageStyle = new vtkInteractorStyleImage();
            interactorCoronal.SetInteractorStyle(imageStyle);

            controlCoronal.RenderWindow.SetInteractor(interactorCoronal);
            controlCoronal.RenderWindow.Render();
            interactorCoronal.Initialize();
        }

        void InitSagittalRendering()
        {
            rendererSagittal = new vtkRenderer();
            controlSagittal.RenderWindow.AddRenderer(rendererSagittal);
            rendererSagittal.AddActor(sagittalSection.createImage());
            rendererSagittal.SetBackground(0.1, 0.1, 0.1);

            interactorSagittal = new vtkRenderWindowInteractor();
            interactorSagittal.LeftButtonPressEvt += InteractorSagittal_LeftButtonDoubleClickEvt;
            vtkInteractorStyleImage imageStyle = new vtkInteractorStyleImage();
            interactorSagittal.SetInteractorStyle(imageStyle);

            controlSagittal.RenderWindow.SetInteractor(interactorSagittal);
            controlSagittal.RenderWindow.Render();
            interactorSagittal.Initialize();
        }

        void RenderSelectedImage()
        {

            //Retrives the brightness and contrast of the file
            DicomFileHandler handler = new DicomFileHandler(fileDirectory);
            window = handler.GetWindow();
            level = handler.GetLevel();

            //Read the image in the vtk file
            vtkDICOMImageReader reader = new vtkDICOMImageReader();
            reader.SetFileName(fileDirectory);
            reader.Update();
            image = new VTKImage(reader.GetOutputPort(), window, level, controlImage);
            vtkImageActor actor = image.createImage();
            if(currentActor != null)
            {
                rendererImage.RemoveActor(currentActor);
            }
            currentActor = actor;
            rendererImage.AddActor(currentActor);
            rendererImage.ResetCamera();
            controlImage.RenderWindow.Render();

            windowSlider.Value = window;
            levelSlider.Value = level;
        }


        void CloseImageRendering()
        {

        }

        #region AxialEvents

        private void txtCurrSlideAxial_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int slice;
                if (int.TryParse(txtCurrSlideAxial.Text, out slice))
                {
                    if (slice > 0 && slice < axialSection.maxSlice)
                    {
                        axialSection.SetSlice(slice);
                    }
                }
            }
        }


        private void InteractorAxial_LeftButtonDoubleClickEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            //[x, y]
            int[] clickPos = interactorAxial.GetEventPosition();

            // Create a picker
            vtkCellPicker picker = vtkCellPicker.New();
            interactorAxial.SetPicker(picker);

            // Perform the picking (this converts 2D click into a 3D world position)
            picker.Pick(clickPos[0], clickPos[1], 0, rendererAxial);

            // Get the picked position in world coordinates
            double[] worldPos = picker.GetPickPosition();
            double x = worldPos[0] + 373;
            if (x > 0 && x < 373 && worldPos[1] > 0 && worldPos[1] < 373)
            {
                axialSection.AddPointToImage(worldPos[0], worldPos[1], 0, rendererAxial);

                int sagittalSlice = 373 - (int)Math.Floor(x);
                sagittalSection.SetSlice(sagittalSlice);
                txtCurrSlideSagittal.Text = sagittalSlice.ToString();
                sagittalSection.RemovePoint(rendererSagittal);
                sagittalSection.AddPointToImage(-worldPos[1], -axialSection.GetCurrentSlice(), 0, rendererSagittal);

                int coronalSlice = (int)Math.Floor(worldPos[1]);
                coronalSection.SetSlice(coronalSlice);
                txtCurrSlideCoronal.Text = coronalSlice.ToString();
                coronalSection.RemovePoint(rendererCoronal);
                coronalSection.AddPointToImage(worldPos[0], -axialSection.GetCurrentSlice(), 0, rendererCoronal);

            }

        }

        private void btnNextSlideAxial_Click(object sender, RoutedEventArgs e)
        {
            if (axialSection.EndSliceReached())
                MessageBox.Show("You have reached the end of possible slices!");
            else
            {
                axialSection.IncrementSlice();
                txtCurrSlideAxial.Text = axialSection.GetCurrentSlice().ToString();
            }
        }

        private async void btnPlayAxial_Click(object sender, RoutedEventArgs e)
        {
            if (isAxialPlaying == false)
            {
                btnPlayAxial.Content = "❚❚";
                isAxialPlaying = true;
                int currSlide = axialSection.GetCurrentSlice();
                if (axialSection.EndSliceReached())
                {
                    axialSection.SetSlice(0);
                    txtCurrSlideAxial.Text = axialSection.GetCurrentSlice().ToString();
                    currSlide = 0;
                }
                for (; currSlide < axialSection.maxSlice; currSlide++)
                {
                    if (isAxialPlaying)
                    {
                        axialSection.IncrementSlice();
                        txtCurrSlideAxial.Text = axialSection.GetCurrentSlice().ToString();
                        await Task.Delay(50);
                        if (axialSection.EndSliceReached())
                        {
                            btnPlayAxial.Content = "▶️";
                            MessageBox.Show("Play done!");
                            isAxialPlaying = false;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                isAxialPlaying = false;
                btnPlayAxial.Content = "▶️";
            }
        }

        private void btnPrevSlideAxial_Click(object sender, RoutedEventArgs e)
        {
            if (axialSection.FirstSliceReached())
                MessageBox.Show("You have reached the beginning of possible slices!");
            else
            {
                axialSection.DecrementSlice();
                txtCurrSlideAxial.Text = axialSection.GetCurrentSlice().ToString();
            }
        }

        #endregion

        #region SagittalEvents

        private void txtCurrSlideSagittal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int slice;
                if (int.TryParse(txtCurrSlideSagittal.Text, out slice))
                {
                    if (slice > 0 && slice < sagittalSection.maxSlice)
                    {
                        sagittalSection.SetSlice(slice);
                    }
                }
            }
        }
        private void InteractorSagittal_LeftButtonDoubleClickEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            //[x, z]
            int[] clickPos = interactorSagittal.GetEventPosition();

            // Create a picker
            vtkCellPicker picker = vtkCellPicker.New();
            interactorSagittal.SetPicker(picker);

            // Perform the picking (this converts 2D click into a 3D world position)
            picker.Pick(clickPos[0], clickPos[1], 0, rendererSagittal);

            // Get the picked position in world coordinates
            double[] worldPos = picker.GetPickPosition();
            double x = 373 + worldPos[0];
            double y = worldPos[1] + 315;
            if (x > 0 && x < 373 && y > 0 && y < 315)
            {

                sagittalSection.AddPointToImage(worldPos[0], worldPos[1], 0, rendererSagittal);

                int coronalSlice = (int)Math.Floor(Math.Abs(worldPos[0]));
                coronalSection.SetSlice(coronalSlice);
                txtCurrSlideCoronal.Text = coronalSlice.ToString();
                coronalSection.RemovePoint(rendererCoronal);
                coronalSection.AddPointToImage(-sagittalSection.GetCurrentSlice(), worldPos[1], 0, rendererCoronal);

                int axialSlice = (int)Math.Floor(Math.Abs(worldPos[1]));
                axialSection.SetSlice(axialSlice);
                txtCurrSlideAxial.Text = axialSlice.ToString();
                axialSection.RemovePoint(rendererAxial);
                axialSection.AddPointToImage(-sagittalSection.GetCurrentSlice(),- worldPos[0] , 0, rendererAxial);
            }

        }

        private void btnNextSlideSagittal_Click(object sender, RoutedEventArgs e)
        {
            if (sagittalSection.EndSliceReached())
                MessageBox.Show("You have reached the end of possible slices!");
            else
            {
                sagittalSection.IncrementSlice();
                txtCurrSlideSagittal.Text = sagittalSection.GetCurrentSlice().ToString();
            }
        }

        private async void btnPlaySagittal_Click(object sender, RoutedEventArgs e)
        {
            if (isSagittalPlaying == false)
            {
                isSagittalPlaying = true;
                btnPlaySagittal.Content = "❚❚";
                int currSlide = sagittalSection.GetCurrentSlice();
                if (currSlide >= 373)
                {
                    sagittalSection.SetSlice(0);
                    txtCurrSlideSagittal.Text = sagittalSection.GetCurrentSlice().ToString();
                    currSlide = 0;
                }
                for (; currSlide <= 373; currSlide++)
                {
                    if (isSagittalPlaying)
                    {
                        sagittalSection.SetSlice(currSlide);
                        txtCurrSlideSagittal.Text = sagittalSection.GetCurrentSlice().ToString();
                        await Task.Delay(50);
                        if (sagittalSection.EndSliceReached())
                        {
                            MessageBox.Show("Play done!");
                            btnPlaySagittal.Content = "▶️";
                            isSagittalPlaying = false;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                isSagittalPlaying = false;
                btnPlaySagittal.Content = "▶️";
            }
        }
        private void btnPrevSlideSagittal_Click(object sender, RoutedEventArgs e)
        {
            if (sagittalSection.FirstSliceReached())
                MessageBox.Show("You have reached the beginning of possible slices!");
            else
            {
                sagittalSection.DecrementSlice();
                txtCurrSlideSagittal.Text = sagittalSection.GetCurrentSlice().ToString();
            }
        }


        #endregion

        #region CoronalEvents

        private void txtCurrSlideCoronal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int slice;
                if (int.TryParse(txtCurrSlideCoronal.Text, out slice))
                {
                    if (slice > 0 && slice < coronalSection.maxSlice)
                    {
                        coronalSection.SetSlice(slice);
                    }
                }
            }
        }


        private void InteractorCoronal_LeftButtonDoubleClickEvt(vtkObject sender, vtkObjectEventArgs e)
        {
            //[x, z]
            int[] clickPos = interactorCoronal.GetEventPosition();

            // Create a picker
            vtkCellPicker picker = vtkCellPicker.New();
            interactorCoronal.SetPicker(picker);

            // Perform the picking (this converts 2D click into a 3D world position)
            picker.Pick(clickPos[0], clickPos[1], 0, rendererCoronal);

            // Get the picked position in world coordinates
            double[] worldPos = picker.GetPickPosition();
            double x = worldPos[0] + 373;
            double y = worldPos[1] + 315;
            if (x > 0 && x < 373 && y > 0 && y < 315)
            {
                coronalSection.AddPointToImage(worldPos[0], worldPos[1], 0, rendererCoronal);

                int sagittalSlice = (int)(373 - x);
                sagittalSection.SetSlice(sagittalSlice);
                txtCurrSlideSagittal.Text = sagittalSlice.ToString();
                sagittalSection.RemovePoint(rendererSagittal);
                sagittalSection.AddPointToImage(-coronalSection.GetCurrentSlice(), y - 315, 0, rendererSagittal);

                int axialSlice = (int)((315 - y) / 315 * 373);
                axialSection.SetSlice(axialSlice);
                txtCurrSlideAxial.Text = axialSlice.ToString();
                axialSection.RemovePoint(rendererAxial);
                axialSection.AddPointToImage(x - 373, coronalSection.GetCurrentSlice(), 0, rendererAxial);
            }

        }

        private void btnNextSlideCoronal_Click(object sender, RoutedEventArgs e)
        {
            if (coronalSection.EndSliceReached())
            {
                MessageBox.Show("You reached the end of possible slices!");
            }
            else
            {
                coronalSection.IncrementSlice();
                txtCurrSlideCoronal.Text = coronalSection.GetCurrentSlice().ToString();
            }
        }

        private async void btnPlayCoronal_Click(object sender, RoutedEventArgs e)
        {
            if (isCoronalPlaying == false)
            {
                isCoronalPlaying = true;
                btnPlayCoronal.Content = "❚❚";
                int currSlide = coronalSection.GetCurrentSlice();
                if (currSlide >= coronalSection.maxSlice)
                {
                    coronalSection.SetSlice(0);
                    txtCurrSlideCoronal.Text = coronalSection.GetCurrentSlice().ToString();
                    currSlide = 0;
                }
                for (; currSlide < coronalSection.maxSlice; currSlide++)
                {
                    if (isCoronalPlaying)
                    {
                        coronalSection.IncrementSlice();
                        txtCurrSlideCoronal.Text = coronalSection.GetCurrentSlice().ToString();
                        await Task.Delay(50);
                        if (coronalSection.EndSliceReached())
                        {
                            MessageBox.Show("Play done!");
                            btnPlayCoronal.Content = "▶️";
                            isCoronalPlaying = false;
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                isCoronalPlaying = false;
                btnPlayCoronal.Content = "▶️";
            }

        }

        private void btnPrevSlideCoronal_Click(object sender, RoutedEventArgs e)
        {
            if (coronalSection.FirstSliceReached())
                MessageBox.Show("You have reached the beginning of possible slices!");
            else
            {
                coronalSection.DecrementSlice();
                txtCurrSlideCoronal.Text = coronalSection.GetCurrentSlice().ToString();
            }
        }

        #endregion

        #region ImageEvents

        private void menuItemOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Open DICOM File";
            openFileDialog.Filter = "DICOM Files (*.dcm)|*.dcm";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == true) 
            {
                fileDirectory = openFileDialog.FileName;
                RenderSelectedImage();
                isImageInitialized = true;
                DicomFileHandler handler = new DicomFileHandler(fileDirectory);
                List<WriteableBitmap> images = handler.GetAllImages();
                int i = 0;
                foreach(WriteableBitmap image in images)
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));

                    // Save the image
                    using (var fileStream = new FileStream("image" + i.ToString() + ".png", FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                    i++;
                }
            }
        }

        private void menuItemOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select a Folder",
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true
            };

            // Set the file name to an empty string to select folders
            dialog.FileName = "Select a folder";

            if (dialog.ShowDialog() == true)
            {
                dicomDirectory = Path.GetDirectoryName(dialog.FileName);
            }
            else
            {
                return;
            }
            openFolderGrid.Visibility = Visibility.Visible;
            openImageGrid.Visibility = Visibility.Hidden;
            //Assign Each VTK RenderWindowControl to its corrosponding host in the WPF app.
            hostSagittal.Child = controlSagittal;
            hostCoronal.Child = controlCoronal;
            hostAxial.Child = controlAxial;
            host3D.Child = control3D;

            string firstFilePath = Directory.GetFiles(dicomDirectory)[0];
            DicomFileHandler handler = new DicomFileHandler(firstFilePath);
            window = handler.GetWindow();
            level = handler.GetLevel();
            // Load DICOM files from directory

            vtkDICOMImageReader reader = new vtkDICOMImageReader();
            reader.SetDirectoryName(dicomDirectory);
            reader.Update();
            vtkAlgorithmOutput data = reader.GetOutputPort();

            sagittalSection = new CTSection(data, 0, window, level, controlSagittal);
            coronalSection = new CTSection(data, 1, window, level, controlCoronal);
            axialSection = new CTSection(data, 2, window, level, controlAxial);
            volumeFactory = new VolumeFactory(data);

            lbMaxSlideSagittal.Text = "/ " + sagittalSection.maxSlice.ToString();
            lbMaxSlideCoronal.Text = "/ " + coronalSection.maxSlice.ToString();
            lbMaxSlideAxial.Text = "/ " + axialSection.maxSlice.ToString();

            vtkRenderer renderer = new vtkRenderer();
            control3D.RenderWindow.AddRenderer(renderer);
            renderer.AddVolume(volumeFactory.createVolume());
            renderer.SetBackground(0.1, 0.2, 0.3); // Set a background color (optional)
            vtkRenderWindowInteractor interactor = new vtkRenderWindowInteractor();
            control3D.RenderWindow.SetInteractor(interactor);
            // Start the interaction and rendering loop
            control3D.RenderWindow.Render();

            InitAxialRendering();

            InitCoronalRendering();

            InitSagittalRendering();
        }

        private void openImageGrid_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void menuItemShowProperties_Click(object sender, RoutedEventArgs e)
        {
            if(fileDirectory == "")
            {
                MessageBox.Show("Please open a dicom file first!");
            }
            else
            {
                PropertiesWindow propertiesWindow = new PropertiesWindow(fileDirectory);
                propertiesWindow.Show();
            }
        }
        #endregion

        private void menuItemAnonymizeData_Click(object sender, RoutedEventArgs e)
        {
            if (fileDirectory == "")
            {
                MessageBox.Show("Please open a dicom file first!");
            }
            else
            {
                TextInputWindow textInputWindow = new TextInputWindow();
                if (textInputWindow.ShowDialog() == true)
                {
                    DicomFileHandler fileHandler = new DicomFileHandler(fileDirectory);
                    fileHandler.AnonymizeData(textInputWindow.prefix);
                }
            }

        }

        private void windowSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbWindowValue.Text = "Window: " + windowSlider.Value.ToString();
            image.Window = (int)windowSlider.Value;
            controlImage.RenderWindow.Render();
        }

        private void levelSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbLevelValue.Text = "Level: " + levelSlider.Value.ToString();
            image.Level = (int)levelSlider.Value;
            controlImage.RenderWindow.Render();
        }

        private void menuItemShowTiles_Click(object sender, RoutedEventArgs e)
        {
            if(dicomDirectory != "")
            {
                List<WriteableBitmap> images = new List<WriteableBitmap>();
                string[] directories = Directory.GetFiles(dicomDirectory).OrderBy(x => int.Parse(Path.GetFileNameWithoutExtension(x))).ToArray<string>();
                foreach (string directory in directories)
                {
                    DicomFileHandler handler = new DicomFileHandler(directory);
                    images.Add(handler.GetImage());
                }
                TilesWindow window = new TilesWindow(images);
                window.Show();
            }
        }
    }
}
