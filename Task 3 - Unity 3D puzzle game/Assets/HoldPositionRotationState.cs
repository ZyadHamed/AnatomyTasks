using UnityEngine;

public class HoldPositionRotationState : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int CurrentRotationState = 0;
    public float originalX = 0;
    public float originalY = 0;
    public float originalZ = 0;
    public int rotateAxis = 0;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(transform.childCount != 0)
        {
            originalX = transform.GetChild(0).gameObject.GetComponent<RotationState>().originalX;
            originalY = transform.GetChild(0).gameObject.GetComponent<RotationState>().originalY;
            originalZ = transform.GetChild(0).gameObject.GetComponent<RotationState>().originalZ;
        }

        switch (CurrentRotationState)
        {
            case 0:
                this.transform.localRotation = Quaternion.Euler(originalX, originalY, originalZ);
                break;
            case 1:
                if (rotateAxis == 0)
                    this.transform.localRotation = Quaternion.Euler(originalX + 90, originalY, originalZ);
                else if (rotateAxis == 1)
                    this.transform.localRotation = Quaternion.Euler(originalX, originalY + 90, originalZ);
                else if (rotateAxis == 2)
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


    }
}
