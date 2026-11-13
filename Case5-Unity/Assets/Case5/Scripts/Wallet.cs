using System;
using UnityEngine;

// Кошелек игрока. Хранит монеты за текущую игру и общее количество (сохраняется в PlayerPrefs)
public class Wallet : MonoBehaviour
{
    public static Wallet Instance { get; private set; }

    private const string SaveKey = "Case5_WalletTotal";

    public int Coins { get; private set; }  // собрано за эту попытку
    public int Total { get; private set; }  // всего за все игры

    public event Action<int, int> CoinsChanged;

    void Awake()
    {
        Instance = this;
        Total = PlayerPrefs.GetInt(SaveKey, 0);
    }

    void Start()
    {
        Notify();
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        Total += amount;
        PlayerPrefs.SetInt(SaveKey, Total);
        PlayerPrefs.Save();
        Notify();
    }

    // потратить монеты (на будущее, например магазин)
    public bool Spend(int amount)
    {
        if (amount <= 0 || amount > Coins) return false;
        Coins -= amount;
        Total -= amount;
        PlayerPrefs.SetInt(SaveKey, Total);
        Notify();
        return true;
    }

    private void Notify()
    {
        if (CoinsChanged != null) CoinsChanged(Coins, Total);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
