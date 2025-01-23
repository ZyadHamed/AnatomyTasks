![Zoom](https://github.com/user-attachments/assets/7804b668-3931-477e-b37b-3876c8c320b6)This project is an advanced image viewer that allows users to open and view 2D images, apply various image processing techniques, and visualize the results in multiple viewports. The application supports a wide range of functionalities, including resolution adjustments, noise addition and removal, filtering, contrast enhancement, and more.

---

## Features

### 1. **Image Viewing and Editing**
- **Input and Output Viewports**: Open and view a 2D image in the input viewport. Apply changes and view the results in two available output viewports.
- **Chained Processing**: Apply one change on the input image, show the result in `Output1`, then apply another change on `Output1` and show the results in `Output2`.
- **Histogram Visualization**: Display the histogram of any image (input or output) by double-clicking or through a designated action.
  ![Histogram](https://github.com/user-attachments/assets/942a5376-2ad1-4194-b6a5-9920ad87b1a3)

### 2. **Image Processing Functionalities**
- **Resolution Adjustment**:
  - Zoom in/out with different factors.
  - Interpolation methods: Nearest-neighbor, linear, bilinear, and cubic interpolation.
    ![Zoom](https://github.com/user-attachments/assets/ec133d37-b234-491f-9e5d-9508a82e5ec5)

- **SNR (Signal-to-Noise Ratio)**:
  - Measure SNR or CNR by placing two ROIs and calculating average intensities.
    ![SNR](https://github.com/user-attachments/assets/45eb5cf9-dfd2-4b2c-af1a-0450cc3a284e)
    ![CNR](https://github.com/user-attachments/assets/aeced624-da38-456d-b24b-2795cc72fd6e)

  - Apply 3 types of noise (e.g., Gaussian, Salt & Pepper, Speckle).
    ![GaussianNoise](https://github.com/user-attachments/assets/487b3683-986c-4019-8821-a497d64bf0ad)
    ![SaltPepperNoise](https://github.com/user-attachments/assets/e91b4ed2-2ba5-4dbf-98b5-de2a526c552e)
  - Apply 3 denoising techniques (e.g., Gaussian blur, Median filter, Bilateral filter).
    ![Denoising](https://github.com/user-attachments/assets/7724125b-9740-4191-b603-928c81e05901)

- **Filtering**:
  - Apply Lowpass and Highpass filters.
    ![LowPassHighPassFilters](https://github.com/user-attachments/assets/c1f24ad2-09bc-46c2-8daa-3b61781574db)

- **CNR (Contrast-to-Noise Ratio)**:
  - Adjust brightness and contrast.
  - Apply 3 contrast adjustment techniques (e.g., Histogram Equalization, CLAHE, Custom method).
    ![CLACHE](https://github.com/user-attachments/assets/b4aad01a-f9d7-485c-ba84-8abb3bdbf848)
    ![Gamma Correction](https://github.com/user-attachments/assets/adcf4f94-aa7a-4dfc-a441-aa22ea44d488)


---

## Usage

1. **Open an Image**: Use the file dialog to open a 2D grayscale image.
2. **Apply Changes**:
   - Apply image processing techniques from the available options.
   - View results in `Output1` and `Output2` viewports.
3. **View Histogram**: Right-click on any image to display its histogram.
4. **Zoom and Interpolate**: Use zoom controls and select interpolation methods for detailed viewing.
5. **Measure SNR/CNR**: Place ROIs on the image to measure SNR or CNR.
6. **Add Noise and Denoise**: Experiment with different noise types and denoising techniques.
7. **Adjust Contrast**: Enhance image contrast using histogram equalization, CLAHE, or custom methods.

---

## Technologies Used
- **C#**: Primary programming language.
- **OpenCVSharp**: For image processing and visualization.
- **WPF**: The GUI Framework
---

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
