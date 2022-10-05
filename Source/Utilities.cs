using UnityEngine;

public class Utilities
{
    static public bool IsAnimationPlaying(Animator animator, string clipName, int layerIndex = 0)
    {
        return animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(clipName);
    }

    static public bool IsAnimationFinish(Animator animator, string clipName, int layerIndex = 0)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(layerIndex);
        return (info.IsName(clipName) && info.normalizedTime > 0.98f);
    }

    static public void ResetAllTriggers(Animator animator)
    {
        foreach(var param in animator.parameters)
        {
            if(param.type == AnimatorControllerParameterType.Trigger)
                animator.ResetTrigger(param.name);
        }
    }
}