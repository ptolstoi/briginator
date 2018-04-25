using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class TransitionState : BaseState
{
    public float duration = 1;
    private float transition = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        transition = 0;
    }
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        transition += Time.deltaTime;

        levelManager.TransitionProgress = Mathf.Clamp01(transition / duration);

        if (transition > duration)
        {
            animator.SetTrigger("Continue");
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        levelManager.TransitionProgress = 1;

        animator.ResetTrigger("Continue");
    }
}
