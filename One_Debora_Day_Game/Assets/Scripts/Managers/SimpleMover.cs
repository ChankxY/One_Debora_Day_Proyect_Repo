
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimpleMover : MonoBehaviour
{
    [Header("Velocidad de movimiento")]
    public float speed = 4f;

    private Vector2 move;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // PlayerInput (Send Messages) llamará este método porque la Action se llama "Move"
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector3 direction = new Vector3(move.x, 0f, move.y);
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }
}
