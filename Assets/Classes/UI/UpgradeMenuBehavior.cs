using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeMenuBehavior : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI SellButtonText;
    [SerializeField] private TextMeshProUGUI UpgradeButton1Text;
    [SerializeField] private TextMeshProUGUI UpgradeButton2Text;
    [SerializeField] private TextMeshProUGUI UpgradeButton3Text;
    [SerializeField] private TextMeshProUGUI TowerNameText;
    [SerializeField] private TextMeshProUGUI TargetTypeText;
    public GameObject UpgradePanel;
    private TowerBehavior CurrentSelectedTower;
    private TowerBehavior TowerUpgrade1;
    private TowerBehavior TowerUpgrade2;
    private TowerBehavior TowerUpgrade3;
    public void Start()
    {
        UpgradePanel.SetActive(false);
    }

    void Update()
    {
        if (UpgradePanel.activeSelf && Input.GetMouseButtonDown(0))
        {
            // Check if pointer is over UI element
            if (!IsPointerOverUIElement())
            {
                UpgradePanel.SetActive(false);
            }
        }
    }

    public bool IsPointerOverUIElement()
    {
        // Works for mouse or touch input
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void OpenUpgradeMenu(TowerBehavior tower)
    {
        if (tower == null)
        {
            return;
        }

        if (tower.Upgrade1 != null)
        {
            TowerUpgrade1 = tower.Upgrade1.GetComponent<TowerBehavior>();
            UpgradeButton1Text.SetText($"{TowerUpgrade1.UpgradeName} - {TowerUpgrade1.SummonCost}");
        }
        else
            UpgradeButton1Text.SetText("No Available Upgrade");
        if (tower.Upgrade2 != null)
        {
            TowerUpgrade2 = tower.Upgrade2.GetComponent<TowerBehavior>();
            UpgradeButton2Text.SetText($"{TowerUpgrade2.UpgradeName} - {TowerUpgrade2.SummonCost}");
        }
        else
            UpgradeButton2Text.SetText("No Available Upgrade");
        if (tower.Upgrade3 != null)
        {
            TowerUpgrade3 = tower.Upgrade3.GetComponent<TowerBehavior>();
            UpgradeButton3Text.SetText($"{TowerUpgrade3.UpgradeName} - {TowerUpgrade3.SummonCost}");
        }
        else
            UpgradeButton3Text.SetText("No Available Upgrade");
        CurrentSelectedTower = tower;
        UpgradePanel.SetActive(true);
        SellButtonText.SetText($"Sell - {CurrentSelectedTower.Refund}");
        TargetTypeText.SetText($"Target: {CurrentSelectedTower.CurrentTargetType}");
        TowerNameText.SetText(CurrentSelectedTower.Name);
    }

    public void CloseUpgradeMenu()
    {
        UpgradePanel.SetActive(false);
        CurrentSelectedTower = null;
    }

    public void TestButtonClick()
    {
        Debug.Log("Button Clicked!");
    }

    public void OnUpgradeButton1Pressed()
    {
        Debug.Log("Upgrade1 Pressed");
        if (CurrentSelectedTower != null)
        {
            Debug.Log("Upgrade1 Request Sent");
            GameLoopManager.EnqueueUpgradeRequest(CurrentSelectedTower, 1);
        }
    }

    public void OnUpgradeButton2Pressed()
    {
        if (CurrentSelectedTower != null)
        {
            GameLoopManager.EnqueueUpgradeRequest(CurrentSelectedTower, 2);
        }
    }

    public void OnUpgradeButton3Pressed()
    {
        if (CurrentSelectedTower != null)
        {
            GameLoopManager.EnqueueUpgradeRequest(CurrentSelectedTower, 3);
        }
    }

    public void OnSellButtonPressed()
    {
        if (CurrentSelectedTower != null)
        {
            CurrentSelectedTower.SellTower();
            UpgradePanel.SetActive(false);
        }
    }

    public void UpdateSelectedTower(TowerBehavior Tower)
    {
        CurrentSelectedTower = Tower;
    }

    public void CycleTargetType()
    {
        int count = System.Enum.GetNames(typeof(TowerTargeting.TargetType)).Length;
        int next = ((int)CurrentSelectedTower.CurrentTargetType + 1) % count;
        CurrentSelectedTower.CurrentTargetType = (TowerTargeting.TargetType)next;

        TargetTypeText.SetText($"Target: {CurrentSelectedTower.CurrentTargetType}");
    }
}
