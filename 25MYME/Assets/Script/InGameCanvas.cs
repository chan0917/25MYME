using UnityEngine;

public class InGameCanvas : MonoBehaviour
{
    public Animator IsAnimator;

    public Animator FixAnimator;

    public Animator PopAnimator;

    public void TakeDamage()
    {
        
        IsAnimator.Play("IsP");
    }

    public void PickFail()
    {
        IsAnimator.Play("Sad");
    }

    public void Fix()
    {
        FixAnimator.Play("Fix");
    }

    public void Pop()
    {
        PopAnimator.Play("PopUpAnim");
    }
}
