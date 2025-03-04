using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool collision = false;
    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents unwanted rotations due to physics
    }

    void Update()
    {
        HandleMovementInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void HandleMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal"); // A (-1) & D (1)
        float moveZ = Input.GetAxis("Vertical");   // W (1) & S (-1)

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
    }

    void MovePlayer()
    {
        if (!collision)
        {
            rb.linearVelocity = moveDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
        }
        else
        {
            rb.linearVelocity = -(moveDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0));
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "wall")
        {
            collision = true;
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        collision = false;
    }
}
