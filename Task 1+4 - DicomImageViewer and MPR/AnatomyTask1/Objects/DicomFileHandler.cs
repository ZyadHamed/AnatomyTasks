using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FellowOakDicom;
using FellowOakDicom.Imaging;
using FellowOakDicom.Imaging.Codec;
using System.Drawing.Imaging;

namespace AnatomyTask1.Objects
{
    public class DicomFileHandler
    {
        string filePath;
        DicomFile currentFile;
        public DicomFileHandler(string fileUrl)
        {
            filePath = fileUrl;
            WPFImageManager manager = new WPFImageManager();

        }

        public void UpdateTag(DicomTag tag, string value)
        {
            DicomFile file = DicomFile.Open(filePath);
            if (file.Dataset.Contains(tag))
            {
                file.Dataset.AddOrUpdate(tag, value);
                currentFile = file.Clone();
            }
        }

        public void SwapTwoTagValues(DicomTag tag1, DicomTag tag2)
        {
            DicomFile file = DicomFile.Open(filePath);
            string tag1Value = file.Dataset.GetString(tag1);
            string tag2Value = file.Dataset.GetString(tag2);
            UpdateTag(tag2, tag1Value);
            UpdateTag(tag1, tag2Value);
            SaveFile();
        }


        //file path looks like this: \kaggle\dataset\1.dcm
        public void SaveFile()
        {
            FileInfo info = new FileInfo(filePath);
            string directory = info.Directory.FullName;
            currentFile.Save(directory + "\\temp.dcm");
            info.Delete();
            File.Move(directory + "\\temp.dcm", filePath);
        }

        //Brightness
        public int GetLevel()
        {
            DicomFile file = DicomFile.Open(filePath);
            if (file.Dataset.Contains(DicomTag.Parse("0028,1050")))
            {
                return int.Parse(file.Dataset.GetString(DicomTag.Parse("0028,1050")));
            }
            return 1;
        }

        //Contrast
        public int GetWindow()
        {
            DicomFile file = DicomFile.Open(filePath);
            if (file.Dataset.Contains(DicomTag.Parse("0028,1051")))
            {
                return int.Parse(file.Dataset.GetString(DicomTag.Parse("0028,1051")));
            }
            return 1;
        }


        public void AnonymizeData(string prefix)
        {
            // Patient Information
            UpdateTag(DicomTag.PatientName, prefix);
            UpdateTag(DicomTag.PatientID, prefix);

            //UpdateTag(DicomTag.PatientBirthDate, prefix);
            //UpdateTag(DicomTag.PatientSex, prefix);

            // Study Information
            //UpdateTag(DicomTag.StudyInstanceUID, prefix); // Replace with prefix instead of a new UID
            //UpdateTag(DicomTag.StudyID, prefix);
            //UpdateTag(DicomTag.StudyDate, prefix);
            //UpdateTag(DicomTag.StudyTime, prefix);

            //// Series Information
            //UpdateTag(DicomTag.SeriesInstanceUID, prefix); // Replace with prefix instead of a new UID
            //UpdateTag(DicomTag.SeriesNumber, prefix);
            //UpdateTag(DicomTag.SeriesDate, prefix);
            //UpdateTag(DicomTag.SeriesTime, prefix);

            //// Accession Information
            //UpdateTag(DicomTag.AccessionNumber, prefix);

            // Physician Information
            //UpdateTag(DicomTag.ReferringPhysicianName, prefix);
            //UpdateTag(DicomTag.PerformingPhysicianName, prefix);
            //UpdateTag(DicomTag.OperatorsName, prefix);

            //// Equipment Information
            //UpdateTag(DicomTag.DeviceSerialNumber, prefix);
            //UpdateTag(DicomTag.ProtocolName, prefix);

            //// Institution Information
            //UpdateTag(DicomTag.InstitutionName, prefix);
            //UpdateTag(DicomTag.InstitutionAddress, prefix);
            //UpdateTag(DicomTag.InstitutionalDepartmentName, prefix);

            //// Other Identifiers
            //UpdateTag(DicomTag.PatientComments, prefix);
            //UpdateTag(DicomTag.PatientAddress, prefix);
            //UpdateTag(DicomTag.PatientTelephoneNumbers, prefix);

            SaveFile();
        }

        public List<DicomTagDTO> GetAllProperties()
        {
            DicomFile file = DicomFile.Open(filePath);
            List<DicomTagDTO> data = new List<DicomTagDTO>();
            foreach (DicomItem item in file.Dataset.ToList())
            {
                data.Add(new DicomTagDTO()
                {
                    TagID = item.Tag.ToString(),
                    VR = item.ValueRepresentation.ToString(),
                    TagName = item.Tag.DictionaryEntry.Name,
                    Value = file.Dataset.GetString(item.Tag)
                });
            }
            return data;
        }

        public Bitmap ConvertWritableBitmapToBitmap(WriteableBitmap writableBitmap)
        {
            // Step 1: Save the WritableBitmap to a MemoryStream as PNG
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(writableBitmap));

            using (MemoryStream stream = new MemoryStream())
            {
                encoder.Save(stream); // Save the WritableBitmap to the stream as PNG
                stream.Position = 0;  // Reset stream position

                // Step 2: Load the stream into a System.Drawing.Bitmap
                Bitmap bitmap = new Bitmap(stream);
                return bitmap;
            }
        }
        public List<WriteableBitmap> GetAllImages()
        {
            List<WriteableBitmap> returnedList = new List<WriteableBitmap>();

            DicomFile file = DicomFile.Open(filePath);
            DicomImage dicomimage = new DicomImage(filePath);
            if (dicomimage.NumberOfFrames >= 1)
            {
                for (int i = 0; i < dicomimage.NumberOfFrames; i++)
                {
                    var dataset = file.Dataset;
                    int width = dataset.GetSingleValue<int>(DicomTag.Columns);
                    int height = dataset.GetSingleValue<int>(DicomTag.Rows);
                    int bitsAllocated = dataset.GetSingleValue<int>(DicomTag.BitsAllocated);
                    int bitsStored = dataset.GetSingleValue<int>(DicomTag.BitsStored);
                    int samplesPerPixel = dataset.GetSingleValue<int>(DicomTag.SamplesPerPixel);
                    var photometricInterpretation = dataset.GetSingleValue<string>(DicomTag.PhotometricInterpretation);

                    // Create DICOM image
                    var dicomImage = new DicomImage(dataset);

                    // Get pixel data
                    var pixelData = DicomPixelData.Create(dataset);

                    // Determine pixel format
                    System.Windows.Media.PixelFormat pixelFormat;
                    switch (photometricInterpretation)
                    {
                        case "MONOCHROME1":
                        case "MONOCHROME2":
                            pixelFormat = bitsAllocated switch
                            {
                                8 => PixelFormats.Gray8,
                                16 => PixelFormats.Gray16,
                                _ => PixelFormats.Gray32Float
                            };
                            break;
                        case "RGB":
                            pixelFormat = PixelFormats.Rgb24;
                            break;
                        case "PALETTE COLOR":
                            pixelFormat = PixelFormats.Rgb24;
                            break;
                        default:
                            pixelFormat = PixelFormats.Bgr32;
                            break;
                    }

                    // Create WriteableBitmap
                    var writeableBitmap = new WriteableBitmap(
                        width,
                        height,
                        96,
                        96,
                        pixelFormat,
                        null);

                    // Get the first frame's pixel data
                    byte[] frameData = pixelData.GetFrame(i).Data;

                    // Calculate stride
                    int stride = (width * pixelFormat.BitsPerPixel + 7) / 8;

                    // Write pixels
                    writeableBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, width, height),
                        frameData,
                        stride,
                        0);

                    writeableBitmap = ChangeImageBrightness(writeableBitmap, 50);
                     

                    returnedList.Add(writeableBitmap);


                }
            }
            return returnedList;
        }

        public WriteableBitmap GetImage()
        {
            DicomFile file = DicomFile.Open(filePath);
            DicomImage dicomimage = new DicomImage(filePath);
            if (dicomimage.NumberOfFrames >= 1)
            {
                    var dataset = file.Dataset;
                    int width = dataset.GetSingleValue<int>(DicomTag.Columns);
                    int height = dataset.GetSingleValue<int>(DicomTag.Rows);
                    int bitsAllocated = dataset.GetSingleValue<int>(DicomTag.BitsAllocated);
                    int bitsStored = dataset.GetSingleValue<int>(DicomTag.BitsStored);
                    int samplesPerPixel = dataset.GetSingleValue<int>(DicomTag.SamplesPerPixel);
                    var photometricInterpretation = dataset.GetSingleValue<string>(DicomTag.PhotometricInterpretation);

                    // Create DICOM image
                    var dicomImage = new DicomImage(dataset);

                    // Get pixel data
                    var pixelData = DicomPixelData.Create(dataset);

                    // Determine pixel format
                    System.Windows.Media.PixelFormat pixelFormat;
                    switch (photometricInterpretation)
                    {
                        case "MONOCHROME1":
                        case "MONOCHROME2":
                            pixelFormat = bitsAllocated switch
                            {
                                8 => PixelFormats.Gray8,
                                16 => PixelFormats.Gray16,
                                _ => PixelFormats.Gray32Float
                            };
                            break;
                        case "RGB":
                            pixelFormat = PixelFormats.Rgb24;
                            break;
                        case "PALETTE COLOR":
                            pixelFormat = PixelFormats.Rgb24;
                            break;
                        default:
                            pixelFormat = PixelFormats.Bgr32;
                            break;
                    }

                    // Create WriteableBitmap
                    var writeableBitmap = new WriteableBitmap(
                        width,
                        height,
                        96,
                        96,
                        pixelFormat,
                        null);

                    // Get the first frame's pixel data
                    byte[] frameData = pixelData.GetFrame(0).Data;

                    // Calculate stride
                    int stride = (width * pixelFormat.BitsPerPixel + 7) / 8;

                    // Write pixels
                    writeableBitmap.WritePixels(
                        new System.Windows.Int32Rect(0, 0, width, height),
                        frameData,
                        stride,
                        0);

                    writeableBitmap = ChangeImageBrightness(writeableBitmap, 50);
                    return writeableBitmap;
            }
            return null;
        }


        WriteableBitmap ChangeImageBrightness(WriteableBitmap image, int brightness)
        {
            // Make the ColorMatrix.
            float b = brightness;
            ColorMatrix cm = new ColorMatrix(new float[][]
                {
            new float[] {b, 0, 0, 0, 0},
            new float[] {0, b, 0, 0, 0},
            new float[] {0, 0, b, 0, 0},
            new float[] {0, 0, 0, 1, 0},
            new float[] {0, 0, 0, 0, 1},
                });
            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorMatrix(cm);

            // Draw the image onto the new bitmap while applying
            // the new ColorMatrix.
            Point[] points =
            {
        new Point(0, 0),
        new Point((int)image.Width, 0),
        new Point(0, (int)image.Height),
    };
            Rectangle rect = new Rectangle(0, 0, (int)image.Width, (int)image.Height);

            // Make the result bitmap.
            Bitmap bm = new Bitmap((int)image.Width, (int)image.Height);
            using (Graphics gr = Graphics.FromImage(bm))
            {
                gr.DrawImage(BitmapFromWriteableBitmap(image), points, rect,
                    GraphicsUnit.Pixel, attributes);
            }

            // Return the result.
            return WriteableBitmapFromBitmap(bm);
        }

        Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            System.Drawing.Bitmap bmp;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new System.Drawing.Bitmap(outStream);
            }
            return bmp;
        }

        WriteableBitmap WriteableBitmapFromBitmap(Bitmap bmp)
        {
            System.Windows.Media.Imaging.BitmapSource bitmapSource =
  System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
  bmp.GetHbitmap(), IntPtr.Zero, System.Windows.Int32Rect.Empty,
  System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            System.Windows.Media.Imaging.WriteableBitmap writeableBitmap = new System.Windows.Media.Imaging.WriteableBitmap(bitmapSource);
            return writeableBitmap;
        }
    }
}
