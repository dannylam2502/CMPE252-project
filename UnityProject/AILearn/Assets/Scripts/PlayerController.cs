using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Vector3 _startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevents unwanted rotations due to physics
        _startPos = transform.position;
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
        rb.linearVelocity = moveDirection * moveSpeed + new Vector3(0, rb.linearVelocity.y, 0);
    }

    public void OnHitByEnemy(GameObject enemy)
    {
        Debug.LogError("Player got hit by " + enemy.name);
        transform.position = _startPos;
    }
}
