using Unity.VisualScripting;
using UnityEngine;

public class TowerClickManager : MonoBehaviour
{
    [SerializeField] private LayerMask TowerLayerMask;
    [SerializeField] private TowerPlacement TowerPlacement;
    [SerializeField] private UpgradeMenuBehavior UpgradeMenu;

    void Update()
    {
        // Only allow clicking if not currently placing a tower
        if ( TowerPlacement.IsPlacingTower() )
            return;

        if ( Input.GetMouseButtonDown( 0 ) )
        {
            if (!UpgradeMenu.IsPointerOverUIElement())
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, TowerLayerMask))
                {
                    TowerBehavior Tower = hit.collider.GetComponent<TowerBehavior>();
                    if (Tower != null)
                    {
                        Vector3 screenPos = Camera.main.WorldToScreenPoint(Tower.transform.position);
                        UpgradeMenu.OpenUpgradeMenu(Tower);
                    }
                }
            }
        }
    }
}