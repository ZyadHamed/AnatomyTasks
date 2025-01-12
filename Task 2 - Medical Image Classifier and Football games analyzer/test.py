import cv2

# Load the image (replace 'image.jpg' with the path to your image)
image = cv2.imread('SampleImage.png')

target_width, target_height = 1366, 720
original_width, original_height = 1920, 1080  # Assuming the original image is 1080p

# Calculate scaling factors
scale_x = target_width / original_width
scale_y = target_height / original_height

# Example YOLO-style bounding box values
center_x = 1223.846923828125 * scale_x  # x-coordinate of the center
center_y = 999.3421020507812 * scale_y  # y-coordinate of the center
width = 33.0718994140625 * scale_x    # width of the box
height = 85.6131591796875 * scale_y # height of the box

# Calculate the top-left and bottom-right corners of the bounding box
top_left = (int(center_x - width / 2), int(center_y - height / 1.3 + height))
bottom_right = (int(center_x + width / 2), int(center_y + height / 2  + height))

# Draw the rectangle on the image
cv2.rectangle(image, top_left, bottom_right, color=(0, 255, 0), thickness=2)

# Display the image with the rectangle
cv2.imshow('Corrected Bounding Box', image)
cv2.waitKey(0)
cv2.destroyAllWindows()
