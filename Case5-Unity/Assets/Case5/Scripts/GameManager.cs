using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Интерфейс (монеты, HP) и конец игры. R - начать заново
public class GameManager : MonoBehaviour
{
    public Health player;
    public Text coinsText;
    public Text hpText;
    public Text gameOverText;
    public Material particleMaterial;

    private bool gameOver;

    void Awake()
    {
        Effects.ParticleMaterial = particleMaterial;
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (Wallet.Instance != null) Wallet.Instance.CoinsChanged += OnCoinsChanged;
        if (player != null)
        {
            player.HealthChanged += OnHealthChanged;
            player.Died += OnPlayerDied;
        }
    }

    void Start()
    {
        // Wallet мог создаться позже, подписываемся еще раз на всякий случай
        if (Wallet.Instance != null)
        {
            Wallet.Instance.CoinsChanged -= OnCoinsChanged;
            Wallet.Instance.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(Wallet.Instance.Coins, Wallet.Instance.Total);
        }
        if (player != null) OnHealthChanged(player.CurrentHP, player.maxHP);
    }

    void OnDisable()
    {
        if (Wallet.Instance != null) Wallet.Instance.CoinsChanged -= OnCoinsChanged;
        if (player != null)
        {
            player.HealthChanged -= OnHealthChanged;
            player.Died -= OnPlayerDied;
        }
    }

    void Update()
    {
        if (gameOver && Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnCoinsChanged(int coins, int total)
    {
        if (coinsText != null) coinsText.text = "Монеты: " + coins + "   (всего: " + total + ")";
    }

    private void OnHealthChanged(int hp, int max)
    {
        if (hpText == null) return;
        hpText.text = "HP: " + hp + " / " + max;
        hpText.color = (player != null && hp <= player.lowHPWarning) ? new Color(1f, 0.3f, 0.3f) : Color.white;
    }

    private void OnPlayerDied()
    {
        gameOver = true;
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Игрок уничтожен!\nНажмите R, чтобы начать заново";
        }
    }
}
