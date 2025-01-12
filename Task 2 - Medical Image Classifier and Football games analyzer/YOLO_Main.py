from collections import defaultdict
import cv2
import numpy as np
import pandas as pd  # Import pandas to handle CSV export
from ultralytics import YOLO

# Load the YOLO model
model = YOLO("yolov8m.pt")

# Open the video file
video_path = "video.mp4"
cap = cv2.VideoCapture(video_path)
cv2.namedWindow("Frame", cv2.WINDOW_GUI_EXPANDED)  # Use WINDOW_NORMAL if WINDOW_GUI_EXPANDED doesn't work


# Check if the video opened successfully
if not cap.isOpened():
    print("Error opening video file")
    exit()

# Retrieve video frame width, height, and FPS
frame_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
frame_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
print("Width: ", frame_width, " Height: ", frame_height)
fps = cap.get(cv2.CAP_PROP_FPS)

# Initialize VideoWriter with the correct frame size
fourcc = cv2.VideoWriter_fourcc(*'mp4v')
out = cv2.VideoWriter('output.mp4', fourcc, fps, (frame_width, frame_height))

# Store the track history with frame numbers
track_history = defaultdict(lambda: [])
# Initialize frame counter
frame_number = 0

# Loop through the video frames
while cap.isOpened():
    # Read a frame from the video
    success, frame = cap.read()

    if success:
        # Run YOLO tracking on the frame, persisting tracks between frames
        #frame = cv2.convertScaleAbs(frame, alpha=1.8, beta=-60)
        results = model.track(frame, persist=True, conf=0.1)

        # Get the boxes and track IDs
        #(result (one frame)) -> [box1, box2, box3...] -> [[x, y, w, h], [x, y, w, h], [x, u, w, h]]
        # (result (one frame)) -> [box1, box2, box3...] -> [id1, id2, id3, .....]
        boxes = results[0].boxes.xywh.cpu()
        track_ids = results[0].boxes.id.int().cpu().tolist()

        # Visualize the results on the frame
        annotated_frame = results[0].plot()

        # Plot the tracks and update track history
        for box, track_id in zip(boxes, track_ids):
            x, y, w, h = box
            # Store the x, y center point along with frame number
            # [Playerid1, Playerid2, Playerid3...] -> [[{"frame": frame, "x": x, "y": y, "width": w, "height": h}, {"frame": frame, "x": x, "y": y, "width": w, "height": h}], [{"frame": frame, "x": x, "y": y, "width": w, "height": h}, {"frame": frame, "x": x, "y": y, "width": w, "height": h}]]
            track_history[track_id].append({"frame": frame_number, "x": float(x), "y": float(y), "width": float(w), "height": float(h)})
        # Write the annotated frame to the output video
        out.write(annotated_frame)

        # Display the annotated frame
        cv2.imshow("Frame", annotated_frame)

        # Increment frame counter
        frame_number += 1

        # Break the loop if 'q' is pressed
        if cv2.waitKey(1) & 0xFF == ord("q"):
            break
    else:
        # Break the loop if the end of the video is reached
        break

# Convert track_history to a flat list for easy DataFrame conversion
"""
Before 
{
    "id1": [{"frame": 1, "x": 5, "y": 6},
            {"frame": 2, "x": 6, "y": 9},
            {"frame": 3, "x": 5, "y": 6}],
    "id2": [{"frame": 1, "x": 5, "y": 6},
            {"frame": 2, "x": 6, "y": 9},
            {"frame": 3, "x": 5, "y": 6}]
}
After:
[
{"trackid": "id1", "frame": 1, "x": 5, "y": 6},
{"trackid": "id1", "frame": 2, "x": 6, "y": 9},
{"trackid": "id1", "frame": 3, "x": 5, "y": 6},
{"trackid": "id2", "frame": 1, "x": 5, "y": 6},
{"trackid": "id2", "frame": 2, "x": 6, "y": 9},
{"trackid": "id2", "frame": 3, "x": 5, "y": 6}

]

3 Crepe, each with tomato, sauce, meat -> tomato, sauce, meat, tomato, sauce, meat, tomato, sauce, meat
"""
flat_track_history = [
    {"track_id": track_id, "frame": item["frame"], "x": item["x"], "y": item["y"], "width": item["width"], "height": item["height"]}
    for track_id, positions in track_history.items()
    for item in positions
]

# Save track history to CSV
df = pd.DataFrame(flat_track_history)
df.to_csv("track_history.csv", index=False)

# Release the video capture and output objects and close display window
cap.release()
out.release()
cv2.destroyAllWindows()
