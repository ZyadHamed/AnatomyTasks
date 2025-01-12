import tkinter as tk
from tkinter import ttk
import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg
from sklearn.preprocessing import MinMaxScaler
import cv2
from PIL import Image, ImageTk
import threading

# Load the data
df = pd.read_csv("track_history.csv")

# Pitch dimensions
pitch_length = 1366
pitch_width = 720
global currFrame
currFrame = 0
original_width, original_height = 1920, 1080  # Assuming the original image is 1080p
scale_x = pitch_length / original_width
scale_y = pitch_width / original_height

# Normalize x and y columns to the pitch dimensions
scaler_x = MinMaxScaler(feature_range=(0, pitch_length))
scaler_y = MinMaxScaler(feature_range=(0, pitch_width))
df['x'] = scaler_x.fit_transform(df[['x']])
df['y'] = scaler_y.fit_transform(df[['y']])


# Function to create the pitch layout with a green background
def create_pitch():
    fig, ax = plt.subplots(figsize=(5, 3))
    ax.set_facecolor("green")
    ax.set_xlim(0, pitch_length)
    ax.set_ylim(0, pitch_width)
    ax.set_aspect(1)

    # Draw pitch boundary and center line
    plt.plot([0, 0, pitch_length, pitch_length, 0], [0, pitch_width, pitch_width, 0, 0], color="white")
    plt.plot([pitch_length / 2, pitch_length / 2], [0, pitch_width], color="white")

    return fig, ax
# Function to display the heatmap for a selected player in the bottom-right area
def show_heatmap(player_id):
    player_data = df[df['track_id'] == player_id]
    x_positions = player_data['x']
    y_positions = pitch_width - player_data['y']

    fig, ax = create_pitch()
    plt.title(f"Heat map for Player {player_id}")
    sns.kdeplot(
        x=x_positions,
        y=y_positions,
        fill=True,
        thresh=0,
        levels=100,
        cmap="Reds",
        ax=ax,
        alpha=0.5  # Make the KDE plot semi-transparent
    )

    for widget in heatmap_frame.winfo_children():
        widget.destroy()

    canvas = FigureCanvasTkAgg(fig, master=heatmap_frame)
    canvas.draw()
    canvas.get_tk_widget().pack(fill=tk.BOTH, expand=True)
# Function to update the video frame in the video_display_frame
def update_video():
    success, frame = video.read()
    if success:
        # Convert the frame to RGB format for tkinter
        global currFrame
        currFrame += 1
        result = df[(df['track_id'] == 1) & (df['frame'] == currFrame)]
        frame = cv2.cvtColor(frame, cv2.COLOR_BGR2RGB)
        #print(result)
        if not result.empty:
            center_x = result["x"] # x-coordinate of the center
            center_y = result["y"]  # y-coordinate of the center
            width = result["width"] # width of the box
            height = result["height"] # height of the box
            top_left = (int(center_x - width / 2), int(center_y - height / 1.3 + height))
            bottom_right = (int(center_x + width / 2), int(center_y + height / 2 + height))
            #cv2.rectangle(frame, top_left, bottom_right, color=(0, 255, 0), thickness=2)
        img = Image.fromarray(frame)
        img = img.resize((500, 300))  # Adjust to fit in the frame size
        imgtk = ImageTk.PhotoImage(image=img)

        # Display the image in the label
        video_label.imgtk = imgtk  # Keep a reference
        video_label.config(image=imgtk)

    # Loop the function every 20 ms to create a video effect
    video_label.after(20, update_video)


# Initialize the main GUI window
root = tk.Tk()
root.title("Player Heatmap Viewer")
root.geometry("1200x700")

# Set up frames for layout
buttons_frame = tk.Frame(root, width=200)
video_display_frame = tk.Frame(root, height=300)
heatmap_frame = tk.Frame(root, height=200)

# Configure the grid layout
root.grid_columnconfigure(0, weight=1)  # Narrow buttons column
root.grid_columnconfigure(1, weight=3)  # Wide display column
root.grid_rowconfigure(0, weight=1)
root.grid_rowconfigure(1, weight=1)

buttons_frame.grid(row=0, column=0, sticky="ns", padx=5, pady=5)
video_display_frame.grid(row=0, column=1, sticky="nsew", padx=5, pady=5)
heatmap_frame.grid(row=1, column=1, sticky="nsew", padx=5, pady=5)

# Create a label in video_display_frame to show video frames
video_label = tk.Label(video_display_frame)
video_label.pack(fill=tk.BOTH, expand=True)

# Open video capture and start the video update loop
video_path = "output.mp4"  # Replace with your video file path
video = cv2.VideoCapture(video_path)
update_video()

# Create a canvas for the buttons with a scrollbar
canvas = tk.Canvas(buttons_frame)
scrollbar = ttk.Scrollbar(buttons_frame, orient="vertical", command=canvas.yview)
scrollable_frame = ttk.Frame(canvas)

scrollable_frame.bind(
    "<Configure>",
    lambda e: canvas.configure(scrollregion=canvas.bbox("all"))
)

canvas.create_window((0, 0), window=scrollable_frame, anchor="nw")
canvas.configure(yscrollcommand=scrollbar.set)
canvas.pack(side=tk.LEFT, fill=tk.BOTH, expand=True)
scrollbar.pack(side=tk.RIGHT, fill=tk.Y)

# Add buttons for each player ID in the scrollable frame
for player_id in df['track_id'].unique():
    button = ttk.Button(scrollable_frame, text=f"Player {player_id}", command=lambda pid=player_id: show_heatmap(pid))
    button.pack(fill=tk.X, pady=2, padx=5)

# Run the GUI main loop
root.mainloop()

# Release video capture on close
video.release()
