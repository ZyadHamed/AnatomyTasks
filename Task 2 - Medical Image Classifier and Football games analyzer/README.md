# AI on Images: Medical Image Classification and Player Tracking

This project combines two tasks related to AI on images:
1. **Medical Image Classification**: Develop an AI model to classify the main organs in medical images (heart, brain, liver, limbs).
2. **Player Tracking in Football**: Use a pretrained YOLO (You Only Look Once) model to track football players and generate a movement heatmap for a selected player.

---

## Features

### 1. **Medical Image Classification**
- **Organ Classification**: Classify medical images into four categories: heart, brain, liver, and limbs.
  ![MachineLearningModel](https://github.com/user-attachments/assets/75bd0ecb-dd35-4eb8-9b6f-395a4a3730ca)

- **Deep Learning Model**: Utilizes a convolutional neural network (CNN) trained on a dataset of medical images.
- **User Interface**: Allows users to upload an image and view the predicted organ class.
  ![UI](https://github.com/user-attachments/assets/5cf6ac6c-e358-4454-9a28-d72b49740d8f)

### 2. **Football Player Tracking**
- **Player Detection**: Uses a pretrained YOLO model to detect and track football players in a video.
  ![Football tracking](https://github.com/user-attachments/assets/9fd92685-99d3-4d35-be58-96c112d9e474)

- **Heatmap Generation**: Allows the user to select a player and generates a heatmap showing their movement throughout the game.
- **Interactive Visualization**: Displays the heatmap overlay on the video for better analysis.
  ![Heatmap](https://github.com/user-attachments/assets/48dd2ab1-b270-4232-8584-e7c49d83aef7)

---

## Usage

### 1. *Medical Image Classification*
1. Launch the application.
2. Upload a medical image (heart, brain, liver, or limbs).
3. View the predicted organ class.

### 2. *Football Player Tracking*
1. Launch the application.
2. Load a football game video.
3. Select a player to track.
4. View the player's movement heatmap overlaid on the video.

---

## Technologies Used
- *Python*: Primary programming language.
- *TensorFlow/Keras*: For building and training the medical image classification model.
- *YOLO (Darknet)*: For player detection and tracking.
- *OpenCV*: For video processing and heatmap visualization.
- *Matplotlib/Seaborn*: For visualization of results.

---
## Video Demonstration
- A video demonstration for the Medical Image Classifier can be found here: https://drive.google.com/file/d/1P_fUhfbUgMpm8Tc8gsVkwx7TzNi7pWtS/view?usp=drive_link
- A video demonstration for the Football Games Analyzer can be found here: https://drive.google.com/file/d/1hx_tlL_X8C_PTwmdzM0QiMXQwRHiwnkb/view?usp=drive_link  
---
## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
