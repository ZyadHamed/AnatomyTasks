using Kitware.VTK;
using System;

public class MedicalImageViewer
{
    private vtkRenderWindow renderWindow;
    private vtkImageViewer2 imageViewer;
    

    public MedicalImageViewer()
    {
        try
        {
            // Initialize the VTK image viewer
            renderWindow = vtkRenderWindow.New();
            imageViewer = vtkImageViewer2.New();

            // Set the render window for the image viewer
            imageViewer.SetRenderWindow(renderWindow);

            // Load your medical image (DICOM or another format)
            vtkDICOMImageReader dicomReader = vtkDICOMImageReader.New();
            dicomReader.SetDirectoryName(@"D:\Cairo University\AnatomyTask1\AnatomyTask1\Subject (1)\98.12.2\");
            dicomReader.Update();

            // Check if any DICOM files were loaded
            if (dicomReader.GetOutput() == null)
            {
                throw new Exception("No DICOM files found or the data is corrupted.");
            }

            // Set the input data for the viewer
            imageViewer.SetInputConnection(dicomReader.GetOutputPort());
            imageViewer.SetSlice(0);  // Start with the first slice
            imageViewer.Render();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error loading DICOM files: " + ex.Message);
        }
    }


    public void Show()
    {
        renderWindow.Render();
    }
}
