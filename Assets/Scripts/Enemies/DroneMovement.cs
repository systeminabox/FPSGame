
using UnityEngine;

public class DroneMovement : EnemyMovement
{
    private const string OpenGuns = "OpenGuns";
    private const string CloseGuns = "CloseGuns";
    private const string IsAttacking = "IsAttacking";
    private const string Reload = "Reload";
    
    
    protected override void OnAttack()
    {
        if (animator)
        {
            animator.SetBool(IsAttacking, true);
        }
    }
    
    protected override void OnSeeTarget()
    {
        if (animator)
        {
            animator.SetTrigger(OpenGuns);
        }
    }
    
    protected override void OnReload()
    {
        if (animator)
        {
            animator.SetTrigger(Reload);
        }
    }
    
    protected override void OnLoseTarget()
    {
        if (animator)
        {
            animator.SetBool(IsAttacking, false);
            animator.SetTrigger(CloseGuns);
        }
    }
}
