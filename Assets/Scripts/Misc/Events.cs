using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class ModeChangeEvent : UnityEvent<GameState?, GameState, GameState?> { }