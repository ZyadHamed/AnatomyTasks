# Dicom Image Viewer with Multiplanar Reconstruction (MPR)

This project combines two essential tasks for working with DICOM datasets: a **DICOM Image Viewer** and a **Multiplanar Reconstruction (MPR)** tool. The application allows users to view and interact with DICOM files, including viewing metadata, anonymizing files, and visualizing slices in various planes (axial, sagittal, and coronal).

---

## Features

### 1. **DICOM Image Viewer**
- **View DICOM Tags**: Display all DICOM metadata tags for a selected file.
- **Anonymize DICOM Files**: Remove or anonymize sensitive patient information from DICOM files.
- **Cine Play**: Play through slices in a cine loop for dynamic visualization.
- **Grid View**: Display all slices in a grid layout for quick overview.

### 2. **Multiplanar Reconstruction (MPR)**
- **Generate Sagittal and Coronal Slices**: Reconstruct sagittal and coronal views from a volume of axial slices.
- **Interactive Viewports**: Pinpoint a point in any viewport (axial, sagittal, or coronal), and the corresponding points will be highlighted in the other two viewports.
- **Multiplanar Visualization**: Simultaneously view axial, sagittal, and coronal slices for comprehensive analysis.

---
## Usage

1. *Load a DICOM File*: Use the file dialog to load a DICOM file or a folder containing a series of DICOM slices.
2. *View DICOM Tags*: Navigate to the metadata viewer to inspect all DICOM tags.
3. *Anonymize Files*: Use the anonymization tool to remove sensitive information.
4. *Cine Play/Grid View*: Switch between cine play and grid view to visualize slices.
5. *MPR Functionality*: Use the MPR tool to generate sagittal and coronal slices. Click on any viewport to see the corresponding points in the other planes.

---

## Technologies Used
- *C#*: Primary programming language.
- *Activiz.NET*: For reading and manipulating DICOM files.
- *WPF*: For the graphical user interface (GUI).
