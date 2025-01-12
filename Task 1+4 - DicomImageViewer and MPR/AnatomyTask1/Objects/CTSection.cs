using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace AnatomyTask1.Objects
{
    /*
     * Hierarchy of rendering sections
     * 
     * WindowsFormsHost <- RenderWindowControl <- RenderWindow <- Renderer
     * Rendrer <- VTKImageActor <- vtkImageData [Loaded from DICOM Images]
     * 
     * RenderWindow <- vtkRenderWindowInteractor <- vtkInteractorStyle (vtkInteractorStyleImage)
     */
    public class CTSection
    {
        vtkAlgorithmOutput data;
        vtkImageReslice sectionReslice;
        public int maxSlice;
        int[] imageDims;
        int spatialCoordinatesIndexer;
        vtkActor lastClickPointActor;
        vtkImageActor imageActor;
        vtkImageActorPointPlacer pointPlacer;
        vtkImageMapToWindowLevelColors windowLevelMapper = new vtkImageMapToWindowLevelColors();
        private int _window; 
        private int _level;

        public int Window 
        { 
            get
            {
                return _window;
            }
            set
            {
                _window = value;
                windowLevelMapper.SetWindow(_window);
                windowLevelMapper.Update();
                //RenderControl.RenderWindow.Render();
            }
        }

        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                windowLevelMapper.SetLevel(_level);
                windowLevelMapper.Update();
                //RenderControl.RenderWindow.Render();
            }
        }

        public RenderWindowControl RenderControl { get; set; }

        public CTSection(vtkAlgorithmOutput _data, int _spatialCoordinatesIndexer, int window, int level, RenderWindowControl renderer)
        {
            data = _data;
            spatialCoordinatesIndexer = _spatialCoordinatesIndexer;
            sectionReslice = new vtkImageReslice();
            maxSlice = 373;
            RenderControl = renderer;
            Window = window;
            Level = level;
            InitSectionReslice();
        }

        public bool EndSliceReached()
        {
            return GetCurrentSlice() >= maxSlice - 1;
        }

        public bool FirstSliceReached()
        {
            return GetCurrentSlice() <= 0;
        }

        public int GetCurrentSlice()
        {
            return (int)sectionReslice.GetResliceAxesOrigin()[spatialCoordinatesIndexer];
        }

        void InitSectionReslice()
        {
            sectionReslice.SetInputConnection(data);
            if (spatialCoordinatesIndexer == 0)
            {
                sectionReslice.SetResliceAxesDirectionCosines(0, -1, 0, 
                                                              0, 0, -1, 
                                                              -1, 0, 0);  // YZ Plane
            }
            else if (spatialCoordinatesIndexer == 1)
            {
                sectionReslice.SetResliceAxesDirectionCosines(-1, 0, 0,
                                                              0, 0, -1, 
                                                              0, 1, 0); //XZ Plane
            }
            else if (spatialCoordinatesIndexer == 2)
            {
                sectionReslice.SetResliceAxesDirectionCosines(-1, 0, 0, 
                                                              0, 1, 0, 
                                                              0, 0, 1);   // XY Plane
            }
            else
            {
                throw new ArgumentException("The provided spatialCoordinateIndexer must be between 0 and 2");
            }
            sectionReslice.SetOutputDimensionality(2);     // 2D Image
            sectionReslice.SetResliceAxesOrigin(0, 0, 0);  // Slice in the origin
            sectionReslice.Update();
        }

        public vtkImageActor createImage()
        {
            imageDims = sectionReslice.GetOutput().GetDimensions();
            imageActor = new vtkImageActor();
            
            windowLevelMapper.SetInputConnection(sectionReslice.GetOutputPort());
            windowLevelMapper.Update();
            imageActor.GetMapper().SetInputConnection(windowLevelMapper.GetOutputPort());
            double[] range = sectionReslice.GetOutput().GetScalarRange();
            return imageActor;
        }

        public void IncrementSlice()
        {
            double currIndex = GetCurrentSlice();
            SetSlice(currIndex + 1);
        }

        public void DecrementSlice()
        {
            double currIndex = GetCurrentSlice();
            SetSlice(currIndex - 1);
        }

        public void SetSlice(double index)
        {
            if (spatialCoordinatesIndexer == 0)
            {
                sectionReslice.SetResliceAxesOrigin(index, 0, 0);
            }
            else if (spatialCoordinatesIndexer == 1)
            {
                sectionReslice.SetResliceAxesOrigin(0, index, 0);
            }
            else if (spatialCoordinatesIndexer == 2)
            {
                sectionReslice.SetResliceAxesOrigin(0, 0, index);
            }
            else
            {
                throw new ArgumentException("The provided spatialCoordinateIndexer must be between 0 and 2");
            }

            sectionReslice.Update();
            windowLevelMapper.Update();
            imageDims = sectionReslice.GetOutput().GetDimensions();
            RenderControl.RenderWindow.Render();
        }
        public void AddPointToImage(double x, double y, double z, vtkRenderer renderer)
        {
            // Create the point coordinates in 3D world space
            vtkPoints points = vtkPoints.New();
            points.InsertNextPoint(x, y, z);  // Use the world position from the picker

            // Create a polydata to store the points
            vtkPolyData pointPolyData = vtkPolyData.New();
            pointPolyData.SetPoints(points);

            // Create a glyph (e.g., a small sphere) to represent the point
            vtkSphereSource sphereSource = vtkSphereSource.New();
            sphereSource.SetRadius(5.0);  // Adjust the radius to make the point visible

            // Create a mapper for the glyph
            vtkGlyph3D glyph = vtkGlyph3D.New();
            glyph.SetInputData(pointPolyData);
            glyph.SetSourceConnection(sphereSource.GetOutputPort());

            if (lastClickPointActor != null)
                renderer.RemoveActor(lastClickPointActor);

            // Create an actor for the glyph (use vtkActor instead of vtkActor2D)
            lastClickPointActor = vtkActor.New();
            vtkPolyDataMapper mapper = vtkPolyDataMapper.New();
            mapper.SetInputConnection(glyph.GetOutputPort());
            lastClickPointActor.SetMapper(mapper);

            // Set the point's color (e.g., red)
            lastClickPointActor.GetProperty().SetColor(1.0, 0.0, 0.0); // RGB values for red

            // Add the actor to the renderer
            renderer.AddActor(lastClickPointActor);

            // Render to update the scene
            renderer.GetRenderWindow().Render();
        }

        public void RemovePoint(vtkRenderer renderer)
        {
            renderer.RemoveActor(lastClickPointActor);
        }

    }
}
