using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace AnatomyTask1.Objects
{
    public interface IVTKSection
    {
        protected bool isSectionPlaying { get; set; }
        protected int maxSlice { get; set; }
        protected vtkImageReslice sectionReslice { get; set; }
        protected vtkImageData data { get; set; }
        public vtkImageActor createImage(vtkImageData data);

    }
}
