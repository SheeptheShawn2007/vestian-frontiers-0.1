using UnityEngine;
using UnityEngine.EventSystems;

public class TowerPlacement : MonoBehaviour
{
    [SerializeField] private LayerMask PlacementCheckMask;
    [SerializeField] private LayerMask PlacementCollideMask;
    [SerializeField] private PlayerStats PlayerStatistics;
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private UpgradeMenuBehavior UpgradeMenu;
    private GameObject CurrentPlacingTower;

    // Update is called once per frame
    void Update()
    {
        if( CurrentPlacingTower != null )
        {
            Ray CamRay = PlayerCamera.ScreenPointToRay( Input.mousePosition );
            RaycastHit HitInfo;

            if( Physics.Raycast( CamRay, out HitInfo, 100f, PlacementCollideMask ) )
            {
                CurrentPlacingTower.transform.position = HitInfo.point;

                if( Input.GetMouseButtonDown( 0 ) && HitInfo.collider.gameObject != null && !EventSystem.current.IsPointerOverGameObject() )
                {
                    if( !HitInfo.collider.gameObject.CompareTag( "CantPlace" ) )
                    {
                        BoxCollider TowerCollider = CurrentPlacingTower.gameObject.GetComponent<BoxCollider>();

                        if (TowerCollider == null)
                            {
                                Debug.LogError("Tower prefab is missing a BoxCollider!");
                                return;
                            }                    

                        Vector3 BoxCenter = CurrentPlacingTower.gameObject.transform.position + TowerCollider.center;
                        Vector3 HalfExtents = TowerCollider.size / 2;
                        if( !Physics.CheckBox( BoxCenter, HalfExtents, Quaternion.identity, PlacementCheckMask ) )
                        {
                            GameLoopManager.TowersInGame.Add( CurrentPlacingTower.GetComponent<TowerBehavior>() );

                            int TowerSummonCost = CurrentPlacingTower.GetComponent<TowerBehavior>().SummonCost; //handles cost changes
                            PlayerStatistics.ChangeMoney( -TowerSummonCost );

                            CurrentPlacingTower.layer = 3; //makes towers actually real fr fr
                            CurrentPlacingTower = null;
                        }
                    }
                }
            }

            if( Input.GetKeyDown( KeyCode.X ) )
            {
                Destroy( CurrentPlacingTower );
                CurrentPlacingTower = null;
                return;
            }
        }
    }

    public void SetTowerToPlace( GameObject Tower )
    {
        UpgradeMenu.CloseUpgradeMenu();
        if (CurrentPlacingTower != null)
        {
            Destroy(CurrentPlacingTower);
            CurrentPlacingTower = null;
        }
        int TowerSummonCost = Tower.GetComponent<TowerBehavior>().SummonCost;
        if( PlayerStatistics.GetMoney() >= TowerSummonCost )
        {
            CurrentPlacingTower = Instantiate( Tower, Vector3.zero, Quaternion.identity );
        }
        else
        {
            Debug.Log( "You need more money to purchase a " + Tower.name );
        }
    }

    public bool IsPlacingTower()
    {
        return CurrentPlacingTower != null;
    }
}
