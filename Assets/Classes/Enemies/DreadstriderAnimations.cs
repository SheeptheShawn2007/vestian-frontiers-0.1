using UnityEngine;

public class DreadstriderAnimations : MonoBehaviour
{
    public Animator animator;
    public Rigidbody rb;
    public int AnimationState = 0;

    private float timer = 0f;
    private bool isPlayingWalkAnimation = false;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (!isPlayingWalkAnimation && timer >= 10f)
        {
            isPlayingWalkAnimation = true;
            timer = 0f;
            AnimationState = 1; // Walk
            animator.SetInteger("AnimationState", AnimationState);
        }
        else if (isPlayingWalkAnimation && timer >= 2f)
        {
            isPlayingWalkAnimation = false;
            timer = 0f;
            AnimationState = 0; // Idle
            animator.SetInteger("AnimationState", AnimationState);
        }
    }
}
