using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using static Extensions;

public partial class LevelManager : MonoBehaviour
{
    [Header("Events")]
    public ModeChangeEvent onModeChangeEvent;

    private GameState mode = GameState.Edit;
    public GameState Mode => mode;

    private GameState? nextMode;
    public GameState? NextMode => mode == GameState.Transition ? nextMode : null;
    private GameState? prevMode;
    public GameState? PrevMode => mode == GameState.Transition ? prevMode : null;

    public float TransitionProgress { get; set; }

    private Animator animator;

    public void TransitionTo(GameState state)
    {
        if (state != GameState.Transition)
        {
            nextMode = state;
            animator.SetTrigger("SwitchTo" + state);
        }
    }

    public void OnNewMode(BaseState currentState, GameState newMode)
    {
        animator.parameters.Any(x =>
        {
            if (x.type == AnimatorControllerParameterType.Trigger)
            {
                animator.ResetTrigger(x.nameHash);
            }
            return false;
        });

        prevMode = mode;
        mode = newMode;

        StartCoroutine(InvokeEvent(PrevMode, mode, NextMode));
    }

    IEnumerator InvokeEvent(GameState? PrevMode, GameState newMode, GameState? NextMode)
    {
        // execute during update!
        yield return null;
        onModeChangeEvent?.Invoke(PrevMode, newMode, NextMode);
        this.ModeChanged(PrevMode, newMode, NextMode);

    }
}
