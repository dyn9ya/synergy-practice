using UnityEngine;

// Бочка со взрывчаткой. Если игрок врезается в нее с силой больше порога, бочка взрывается
// и игрок уничтожается. Если подойти медленно, бочку можно просто толкать.
[RequireComponent(typeof(Rigidbody))]
public class ExplosiveBarrel : MonoBehaviour
{
    public float minImpactSpeed = 8f;     // порог силы удара (относительная скорость)
    public float explosionRadius = 4f;
    public float explosionForce = 700f;

    private bool exploded;

    void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;
        if (!collision.collider.CompareTag("Player")) return;

        float impact = collision.relativeVelocity.magnitude;
        if (impact < minImpactSpeed) return;

        Explode(collision.collider.GetComponentInParent<Health>());
    }

    public void Explode(Health victim)
    {
        if (exploded) return;
        exploded = true;

        Effects.Burst(transform.position, new Color(1f, 0.5f, 0.1f), 80, 9f);
        Effects.Burst(transform.position, new Color(0.25f, 0.25f, 0.25f), 40, 4f);

        // раскидываем все физические объекты рядом
        foreach (var col in Physics.OverlapSphere(transform.position, explosionRadius))
        {
            var body = col.attachedRigidbody;
            if (body != null && body.gameObject != gameObject)
                body.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f);
        }

        if (victim != null) victim.Kill();
        Destroy(gameObject);
    }
}
