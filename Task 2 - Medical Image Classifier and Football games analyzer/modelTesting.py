import tkinter as tk
from tkinter import filedialog
from PIL import Image, ImageTk
import torch
from torchvision import transforms
import torch.nn.functional as F

# Load your model
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
model = torch.load("model.pt", map_location=device)
model.eval()

classification_Dict = {0: "Brain", 1: "Lung", 2: "Knee", 3:"Hand"}

# Define the image transformation
transform = transforms.Compose([
    transforms.Resize((300, 300)),
    transforms.ToTensor(),
    transforms.Normalize([0.485, 0.456, 0.406], [0.229, 0.224, 0.225])
])


# Function to classify the image
def classify_image(image_path):
    image = Image.open(image_path).convert("RGB")
    image = transform(image).unsqueeze(0)  # Add batch dimension
    image = image.to(device)

    with torch.no_grad():
        output = model(image)
        print(F.softmax(output, dim=1))
        confidence = torch.max(F.softmax(output, dim=1))
        _, predicted = torch.max(output, 1)

    return predicted.item(), confidence.item()


# Function to load and display the image
def load_image():
    file_path = filedialog.askopenfilename(filetypes=[("Image files", "*.jpg;*.jpeg;*.png")])
    if file_path:
        # Display the selected image
        image = Image.open(file_path)
        image = image.resize((300, 300))
        img_tk = ImageTk.PhotoImage(image)
        image_label.config(image=img_tk)
        image_label.image = img_tk  # Keep a reference to prevent garbage collection

        # Classify the image and update result
        result, confidence = classify_image(file_path)
        result = classification_Dict[result]
        result_label.config(text=f"Predicted Class: {result}")
        confidence_label.config(text=f"Confidence: {confidence * 100}%")

# Initialize the GUI
root = tk.Tk()
root.title("Image Classification")

# Create and place the 'Select File' button
select_button = tk.Button(root, text="Select Image", command=load_image)
select_button.pack(pady=10)

# Label to display the selected image
image_label = tk.Label(root)
image_label.pack()

# Label to display the classification result
result_label = tk.Label(root, text="Predicted Class: ", font=("Arial", 14))
result_label.pack(pady=10)

confidence_label = tk.Label(root, text="Confidence: ", font=("Arial", 14))
confidence_label.pack(pady=10)

# Run the GUI
root.mainloop()
