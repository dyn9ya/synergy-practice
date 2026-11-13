using UnityEngine;

// Частицы пыли под ногами игрока. Чем быстрее бежит, тем больше частиц,
// когда стоит или в прыжке - частиц нет
public class PlayerTrail : MonoBehaviour
{
    public PlayerController player;
    public ParticleSystem dust;
    public float particlesPerSpeed = 6f;

    void Awake()
    {
        if (player == null) player = GetComponentInParent<PlayerController>();
        if (dust == null) dust = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if (player == null || dust == null) return;
        var emission = dust.emission;
        float speed = player.CurrentSpeed;
        emission.rateOverTime = (player.IsGrounded && speed > 0.5f) ? speed * particlesPerSpeed : 0f;
    }
}
