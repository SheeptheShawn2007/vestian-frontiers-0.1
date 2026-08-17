using TMPro;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int StartingMoney;
    [SerializeField] private int StartingManPower;
    [SerializeField] private TextMeshProUGUI MoneyDisplayText;
    [SerializeField] private TextMeshProUGUI ManPowerDisplayText;
    [SerializeField] private TextMeshProUGUI WaveDisplayText;
    [SerializeField] private TextMeshProUGUI LeakDisplayText;
    private int CurrentMoney;
    private int CurrentManPower;
    private int Leaks;
    private int Wave;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentMoney = StartingMoney;
        CurrentManPower = StartingManPower;
        Leaks = 0;
        MoneyDisplayText.SetText($"Requisition: {StartingMoney}");
        LeakDisplayText.SetText($"Lives Lost: {Leaks}");
        WaveDisplayText.SetText($"Wave {Wave}");
    }

    public void ChangeMoney(int MoneyToAdd)
    {
        CurrentMoney += MoneyToAdd;
        MoneyDisplayText.SetText($"Requisition: {CurrentMoney}");
    }

    public void ChangeManpower(int ManPowerToAdd)
    {
        CurrentManPower += ManPowerToAdd;
    }

    public void ChangeLeak(int LeakChange)
    {
        Leaks += LeakChange;
        LeakDisplayText.SetText($"Lives Lost: {Leaks}");
    }

    public void ChangeWave(int WaveChange)
    {
        Wave += WaveChange;
        WaveDisplayText.SetText($"Wave {Wave}");
    }

    public int GetMoney()
    {
        return CurrentMoney;
    }

    public int GetManpower()
    {
        return StartingManPower;
    }
}
