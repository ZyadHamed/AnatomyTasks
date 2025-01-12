import numpy as np
import pandas as pd
from PIL import Image
import os

import torch
from torch.utils.data import Dataset, DataLoader, ConcatDataset,random_split
from torchvision import transforms, datasets
from torchvision.models import EfficientNet_B3_Weights, efficientnet_b3
import torch.nn as nn
import torch.nn.functional as F
import torch.optim as optim
from tqdm import tqdm
class CustomImageDataset(Dataset):
    def __init__(self, img_dir, label, transform=None):
        self.img_dir = img_dir
        self.transform = transform
        self.img_files = [f for f in os.listdir(img_dir) if os.path.isfile(os.path.join(img_dir, f))]
        self.label = label
    def __len__(self):
        return len(self.img_files)

    def __getitem__(self, idx):
        img_path = os.path.join(self.img_dir, self.img_files[idx])
        image = Image.open(img_path).convert("RGB")
        if self.transform:
            image = self.transform(image)
        return image, self.label
brain_dir = "/kaggle/input/brain-tumor-mri-dataset/Training/notumor"
lung_dir1 = "/kaggle/input/iqothnccd-lung-cancer-dataset/The IQ-OTHNCCD lung cancer dataset/The IQ-OTHNCCD lung cancer dataset/Bengin cases"
lung_dir2 = "/kaggle/input/iqothnccd-lung-cancer-dataset/The IQ-OTHNCCD lung cancer dataset/The IQ-OTHNCCD lung cancer dataset/Malignant cases"
lung_dir3 = "/kaggle/input/iqothnccd-lung-cancer-dataset/The IQ-OTHNCCD lung cancer dataset/The IQ-OTHNCCD lung cancer dataset/Normal cases"
knee_dir1 = "/kaggle/input/digital-knee-xray/MedicalExpert-I/0Normal"
knee_dir2 = "/kaggle/input/digital-knee-xray/MedicalExpert-I/1Doubtful"
knee_dir3 = "/kaggle/input/digital-knee-xray/MedicalExpert-I/2Mild"
knee_dir4 = "/kaggle/input/digital-knee-xray/MedicalExpert-I/3Moderate"
knee_dir5 = "/kaggle/input/digital-knee-xray/MedicalExpert-I/4Severe"
hand_dir = "/kaggle/input/hand-xray/test/hand"

test_transform = transforms.Compose([
    transforms.Resize((300, 300)),
    transforms.ToTensor(),          # Convert images to PyTorch tensors
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])  # Standard normalization
])

brain_ds = CustomImageDataset(brain_dir, 0, test_transform)

lung_ds1 = CustomImageDataset(lung_dir1, 1, test_transform)
lung_ds2 = CustomImageDataset(lung_dir2, 1, test_transform)
lung_ds3 = CustomImageDataset(lung_dir3, 1, test_transform)
lung_ds = ConcatDataset([lung_ds1, lung_ds2, lung_ds3])

knee_ds1 = CustomImageDataset(knee_dir1, 2, test_transform)
knee_ds2 = CustomImageDataset(knee_dir2, 2, test_transform)
knee_ds3 = CustomImageDataset(knee_dir3, 2, test_transform)
knee_ds4 = CustomImageDataset(knee_dir4, 2, test_transform)
knee_ds5 = CustomImageDataset(knee_dir5, 2, test_transform)
knee_ds = ConcatDataset([knee_ds1, knee_ds2, knee_ds3, knee_ds4, knee_ds5])

hand_ds = CustomImageDataset(hand_dir, 3, test_transform)

dataset = ConcatDataset([brain_ds, lung_ds, knee_ds, hand_ds])
train_size = int(0.8 * len(dataset))  # 80% for training
test_size = len(dataset) - train_size  # 20% for testing

# Split the dataset
train_dataset, test_dataset = random_split(dataset, [train_size, test_size])
train_loader = DataLoader(train_dataset, batch_size=32, shuffle=True)
test_loader = DataLoader(test_dataset, batch_size=32, shuffle=False)
# Parameters
num_epochs = 3
batch_size = 32
learning_rate = 0.001
num_classes = 4  # Adjust based on your dataset
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

model = efficientnet_b3(EfficientNet_B3_Weights.DEFAULT)
model.classifier[1] = nn.Linear(in_features=1536, out_features=num_classes, bias=True)
model = model.to(device)
criterion = nn.CrossEntropyLoss()
optimizer = optim.Adam(model.parameters(), lr=learning_rate)
for epoch in range(num_epochs):
    model.train()
    running_loss = 0.0
    for images, labels in tqdm(train_loader, desc=f"Epoch {epoch + 1}/{num_epochs}", unit="batch"):
        images, labels = images.to(device), labels.to(device)
        
        # Forward pass
        outputs = model(images)
        loss = criterion(outputs, labels)
        
        # Backward pass and optimization
        optimizer.zero_grad()
        loss.backward()
        optimizer.step()
        
        running_loss += loss.item()
    
    print(f"Epoch [{epoch + 1}/{num_epochs}], Loss: {running_loss / len(train_loader)}")
    model.eval()
    correct = 0
    total = 0
    val_loss = 0.0
    with torch.no_grad():
        for images, labels in test_loader:
            images, labels = images.to(device), labels.to(device)
            outputs = model(images)
            loss = criterion(outputs, labels)
            val_loss += loss.item()

            _, predicted = torch.max(outputs.data, 1)
            total += labels.size(0)
            correct += (predicted == labels).sum().item()
            print(predicted)

    # Calculate accuracy and average validation loss
    accuracy = 100 * correct / total
    avg_val_loss = val_loss / len(test_loader)
    print(f"Validation Loss: {avg_val_loss:.4f}, Accuracy: {accuracy:.2f}%")
torch.save(model, "model.pt")
