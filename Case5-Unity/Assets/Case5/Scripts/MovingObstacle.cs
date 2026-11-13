using UnityEngine;

// Подвижное препятствие. Двигается между двумя точками или крутится,
// при касании наносит игроку урон. Когда HP игрока кончается, Health уничтожает игрока.
[RequireComponent(typeof(Rigidbody))]
public class MovingObstacle : MonoBehaviour
{
    public enum MoveMode { PingPong, Rotate }

    public MoveMode mode = MoveMode.PingPong;
    public Vector3 moveOffset = new Vector3(8f, 0f, 0f); // куда ехать от стартовой точки
    public float moveSpeed = 0.5f;
    public float rotateSpeed = 90f;

    public int damage = 20;
    public float damageCooldown = 1f;   // чтобы урон не шел каждый кадр
    public float pushForce = 6f;

    private Rigidbody rb;
    private Vector3 startPos;
    private float lastDamageTime = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = transform.position;
    }

    void FixedUpdate()
    {
        if (mode == MoveMode.PingPong)
        {
            float t = Mathf.PingPong(Time.time * moveSpeed, 1f);
            rb.MovePosition(startPos + moveOffset * t);
        }
        else
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rotateSpeed * Time.fixedDeltaTime, 0f));
        }
    }

    void OnCollisionEnter(Collision collision) { TryHit(collision); }
    void OnCollisionStay(Collision collision) { TryHit(collision); }

    private void TryHit(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        if (Time.time - lastDamageTime < damageCooldown) return;

        var health = collision.collider.GetComponentInParent<Health>();
        if (health == null) return;

        lastDamageTime = Time.time;

        // отталкиваем игрока от препятствия
        var body = collision.rigidbody;
        if (body != null)
        {
            Vector3 dir = collision.transform.position - transform.position;
            dir.y = 0f;
            body.AddForce(dir.normalized * pushForce + Vector3.up * 2f, ForceMode.VelocityChange);
        }

        health.TakeDamage(damage);
    }
}
