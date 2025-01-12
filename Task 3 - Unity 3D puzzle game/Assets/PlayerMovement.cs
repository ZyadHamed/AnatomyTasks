using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController player;
    public float speed = 12f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        float gravity = -9.8f;
        Vector3 movementVector = transform.right * x + transform.forward * z + transform.up * gravity;
        player.Move(movementVector * speed * Time.deltaTime);
    }
}
