using UnityEngine;

// Управление игроком: WASD/стрелки - движение, Shift - бег, пробел - прыжок
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float runSpeed = 11f;
    public float jumpForce = 6f;
    public float fallLimitY = -10f; // если упал с карты

    private Rigidbody rb;
    private Health health;
    private Vector3 input;
    private bool jumpPressed;

    public float CurrentSpeed
    {
        get { return rb == null ? 0f : new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude; }
    }

    public bool IsGrounded
    {
        get { return Physics.Raycast(transform.position, Vector3.down, 1.1f); }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        health = GetComponent<Health>();
    }

    void Update()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1f) input.Normalize();

        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
            jumpPressed = true;

        if (transform.position.y < fallLimitY && health != null)
            health.Kill();
    }

    void FixedUpdate()
    {
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 v = input * speed;
        rb.velocity = new Vector3(v.x, rb.velocity.y, v.z);

        if (input.sqrMagnitude > 0.01f)
        {
            // поворачиваем капсулу в сторону движения
            Quaternion look = Quaternion.LookRotation(input, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, look, 12f * Time.fixedDeltaTime));
        }

        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpPressed = false;
        }
    }
}
