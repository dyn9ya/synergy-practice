using System;
using UnityEngine;

// Здоровье игрока (HP). Когда HP кончается, объект уничтожается
public class Health : MonoBehaviour
{
    public int maxHP = 100;
    public int lowHPWarning = 30; // при таком HP полоска красная

    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int, int> HealthChanged; // текущее, максимум
    public event Action Died;

    void Awake()
    {
        CurrentHP = maxHP;
    }

    void Start()
    {
        if (HealthChanged != null) HealthChanged(CurrentHP, maxHP);
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        if (HealthChanged != null) HealthChanged(CurrentHP, maxHP);
        if (CurrentHP == 0) Die();
    }

    // мгновенная смерть (взрыв бочки, падение с карты)
    public void Kill()
    {
        if (IsDead) return;
        CurrentHP = 0;
        if (HealthChanged != null) HealthChanged(CurrentHP, maxHP);
        Die();
    }

    private void Die()
    {
        IsDead = true;
        Effects.Burst(transform.position, new Color(0.9f, 0.2f, 0.2f), 60, 6f);
        if (Died != null) Died();
        Destroy(gameObject);
    }
}
