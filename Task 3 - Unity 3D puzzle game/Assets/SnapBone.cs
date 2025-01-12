using UnityEngine;

public class SnapBone : MonoBehaviour
{
    public Transform snapObject;
    public float snapThreshold = 1;
    public float requiredRotationState = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(this.transform.position, snapObject.position) < snapThreshold
            && GetComponent<RotationState>().CurrentRotationState == requiredRotationState)
        {
            this.transform.SetParent(null);
            this.transform.position = snapObject.position;
            this.transform.rotation = snapObject.rotation;
            this.transform.SetParent(snapObject);
            snapObject.gameObject.SetActive(true);
            Destroy(this.gameObject);
        }
    }
}
