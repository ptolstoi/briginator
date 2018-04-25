using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class BaseState : StateMachineBehaviour
{
    [SerializeField]
    private GameState state;
    public GameState State => state;

    protected LevelManager levelManager;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (levelManager == null)
        {
            levelManager = animator.GetComponent<LevelManager>();
        }

        levelManager.TransitionProgress = 0;

        levelManager.OnNewMode(this, state);
    }
}
