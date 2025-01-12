using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitware.VTK;

namespace AnatomyTask1.Objects
{
    public class VolumeFactory
    {
        vtkAlgorithmOutput data;
        public VolumeFactory(vtkAlgorithmOutput _data) 
        {
            data = _data;
        }
        vtkColorTransferFunction createColorFunction()
        {
            vtkColorTransferFunction colorFunction = new vtkColorTransferFunction();
            colorFunction.SetColorSpaceToDiverging();
            colorFunction.AddRGBPoint(0, 0.0, 0.0, 0.0);   // Black for low intensity
            colorFunction.AddRGBPoint(-300, 0.772, 0.3, 0.3);
            colorFunction.AddRGBPoint(500, 1.0, 0.5, 0.3); // Red-orange at mid-range intensity
            colorFunction.AddRGBPoint(1000, 1.0, 1.0, 1.0); // White for high intensity
            return colorFunction;
        }

        vtkPiecewiseFunction createOpacityFunction()
        {
            vtkPiecewiseFunction opacityFunction = new vtkPiecewiseFunction();
            opacityFunction.AddPoint(0, 0.0);  // Completely transparent at low values
            opacityFunction.AddPoint(250, 0.05);
            opacityFunction.AddPoint(500, 0.1);  // Slightly transparent at mid-range values
            opacityFunction.AddPoint(1000, 0.9);  // Nearly opaque at high values
            opacityFunction.AddPoint(1500, 1.0);  // Fully opaque at high values
            return opacityFunction;
        }

        public vtkVolume createVolume()
        {

            //Construct a VTK volume mapper from the data
            vtkSmartVolumeMapper volumeMapper = new vtkSmartVolumeMapper();
            volumeMapper.SetInputConnection(data);
            vtkVolumeProperty volumeProperty = new vtkVolumeProperty();
            volumeProperty.SetInterpolationTypeToLinear();
            volumeProperty.SetScalarOpacity(createOpacityFunction());
            volumeProperty.SetColor(createColorFunction());

            vtkVolume volume = new vtkVolume();
            volume.SetMapper(volumeMapper);
            volume.SetProperty(volumeProperty);
            return volume;
        }

    }
}
