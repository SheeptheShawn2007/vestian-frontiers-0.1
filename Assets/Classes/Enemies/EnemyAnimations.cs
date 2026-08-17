using UnityEngine;

public class EnemyAnimations : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    public int AnimationState = 0;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = rb.linearVelocity.magnitude;

        if (speed < 0.1f)
        {
            AnimationState = 0; // Idle
        }
        else if (speed < 3f)
        {
            AnimationState = 1; // Walk
        }
        else
        {
            AnimationState = 2; // Run
        }

        animator.SetInteger("AnimationState", AnimationState);
    }
}
