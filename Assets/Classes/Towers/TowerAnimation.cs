using UnityEngine;
using System.Collections;

public class TowerAnimation : MonoBehaviour
{
    public TowerBehavior Tower;
    public Animator animator;

    // void Start()
    // {
    //     animator = GetComponent<Animator>();
    //     Tower = GetComponent<TowerBehavior>();
    // }

    public void Fire()
    {
        animator.SetTrigger("Recoil");
        StartCoroutine( WaitForNextShot() );
    }

    private IEnumerator WaitForNextShot()
    {
        animator.SetBool("CanFire", false);
        yield return new WaitForSeconds(Tower.GetDelay());
        animator.SetBool("CanFire", true);
        if (Tower.Target != null)
        {
            animator.SetBool("Target", true);
        }
        else
        {
            animator.SetBool("Target", false);
        }
    }
}
