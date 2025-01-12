using Kitware.VTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnatomyTask1.Objects
{
    public class VTKImage
    {
        vtkImageMapToWindowLevelColors windowLevelMapper = new vtkImageMapToWindowLevelColors();
        vtkImageActor imageActor = new vtkImageActor();
        private int _window;
        private int _level;
        public RenderWindowControl RenderControl { get; set; }
        vtkAlgorithmOutput data;
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
        public VTKImage(vtkAlgorithmOutput _data, int window, int level, RenderWindowControl renderer)
        {
            data = _data;
            RenderControl = renderer;
            _window = window;
            _level = level;
            windowLevelMapper.SetInputConnection(data);
        }

        public vtkImageActor createImage()
        {
            Window = _window;
            Level = _level;
            windowLevelMapper.Update();
            imageActor.GetMapper().SetInputConnection(windowLevelMapper.GetOutputPort());
            return imageActor;
        }
    }
}
