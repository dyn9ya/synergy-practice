using UnityEngine;

// Монетка: крутится, при касании игрока попадает в кошелек и исчезает
public class Coin : MonoBehaviour
{
    public int value = 1;
    public float rotateSpeed = 120f;
    public float bobHeight = 0.15f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        // чтобы монеты качались не синхронно
        bobOffset = Random.Range(0f, 6f);
    }

    private float bobOffset;

    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
        transform.position = startPos + Vector3.up * Mathf.Sin(Time.time * 3f + bobOffset) * bobHeight;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Wallet.Instance != null) Wallet.Instance.Add(value);
        Effects.Burst(transform.position, new Color(1f, 0.85f, 0.2f), 20, 3f);
        Destroy(gameObject);
    }
}
