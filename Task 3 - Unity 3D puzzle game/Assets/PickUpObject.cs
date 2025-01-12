using UnityEngine;
using UnityEngine.UI;

public class PickupObject : MonoBehaviour
{
    public Transform holdPosition;      // Set this to the empty GameObject (hold position) in front of the player
    public float pickupRange = 4f;      // Maximum distance to pick up an object
    public Rigidbody pickedObject;     // The object currently being picked up
    public Camera camera;
    public GameObject helperText;

    private void Start()
    {
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (pickedObject == null)
            {
                // Try to pick up an object
                TryPickupObject();
            }
            else
            {
                // Drop the object if already holding one
                DropObject();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if(pickedObject != null)
            {
                if (pickedObject.gameObject.GetComponent<RotationState>().CurrentRotationState >= 3)
                {
                    holdPosition.gameObject.GetComponent<HoldPositionRotationState>().CurrentRotationState = 0;
                    pickedObject.gameObject.GetComponent<RotationState>().CurrentRotationState = 0;
                }
                else
                {
                    holdPosition.gameObject.GetComponent<HoldPositionRotationState>().CurrentRotationState += 1;
                    pickedObject.gameObject.GetComponent<RotationState>().CurrentRotationState++;
                }
            }
        }

        // If holding an object, update its position
        if (pickedObject != null)
        {
            pickedObject.transform.position = holdPosition.position;
            pickedObject.transform.rotation = holdPosition.rotation;
        }
    }

    private void TryPickupObject()
    {
        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); // Ray from the center of the screen
        RaycastHit hit;
        // Cast the ray from the center of the screen, aligning with the crosshair
        if (Physics.Raycast(ray, out hit, pickupRange))
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Pick up the object: make it kinematic and attach it to the hold position
                pickedObject = rb;
                pickedObject.useGravity = false;
                pickedObject.isKinematic = true;
                pickedObject.transform.position = holdPosition.position;
                pickedObject.transform.SetParent(holdPosition);
            }
        }
    }


    private void DropObject()
    {
        if (pickedObject != null)
        {
            // Re-enable physics and detach the object
            pickedObject.useGravity = true;
            pickedObject.isKinematic = false;
            pickedObject.transform.SetParent(null);
            pickedObject = null;
        }
    }
}
