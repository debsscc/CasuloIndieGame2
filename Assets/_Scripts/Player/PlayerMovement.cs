using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        direction = input.normalized;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    public void SetSpeed(float newSpeed) => speed = newSpeed;

    public void SetDirection(Vector2 newDirection) => direction = newDirection.normalized;
}