using UnityEngine;

public class RotationState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int CurrentRotationState;
    public float originalX = 0;
    public float originalY = 0;
    public float originalZ = 0;
    public int rotateAxis = 0;
    void Start()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        CurrentRotationState = Random.Range(0, 3);
    }

    // Update is called once per frame
    void Update()
    {
        switch (CurrentRotationState)
        {
            case 0:
                this.transform.localRotation = Quaternion.Euler(originalX, originalY, originalZ);
                break;
            case 1:
                if(rotateAxis == 0)
                    this.transform.localRotation = Quaternion.Euler(originalX + 90, originalY, originalZ);
                else if(rotateAxis == 1)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY + 90, originalZ);
                else if(rotateAxis == 2)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY, originalZ + 90);
                break;
            case 2:
                if (rotateAxis == 0)
                    this.transform.localRotation = Quaternion.Euler(originalX + 180, originalY, originalZ);
                else if (rotateAxis == 1)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY + 180, originalZ);
                else if (rotateAxis == 2)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY, originalZ + 180); 
                break;
            case 3:
                if (rotateAxis == 0)
                    this.transform.localRotation = Quaternion.Euler(originalX + 270, originalY, originalZ);
                else if (rotateAxis == 1)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY + 270, originalZ);
                else if (rotateAxis == 2)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY, originalZ + 270); 
                break;
        }
        
        if(transform.position.z > 3.2)//Enabling gravity if above ground
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezePositionY;
        }
    }
}
