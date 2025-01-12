using UnityEngine;
using UnityEngine.UI;
public class MouseLook : MonoBehaviour
{
    public float mouseSensitivityX = 100f;
    public float mouseSensitivityY = 100f;
    public Transform HoldPosition;
    float xRotation = 0f;
    public Transform playerBody;
    public GameObject helperText;
    public GameObject pickedBone;
    public Camera Camera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
        Vector3 holdOffset = transform.forward * 1f + transform.up * (xRotation / 90f);
        HoldPosition.position = transform.position + holdOffset;
        Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Ray from the center of the screen
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 4))
        {
            GameObject rayHit = hit.collider.gameObject;
            pickedBone = rayHit;
            if(rayHit != null && rayHit.CompareTag("Bone"))
            {
                helperText.SetActive(true);
            }
        }
        else
        {
            helperText.SetActive(false);
        }
    }
}
