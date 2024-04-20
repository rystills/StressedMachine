using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerOverlay : MonoBehaviour
{
    [SerializeField] private Material timerMat;

    private void Update() => timerMat.SetFloat("stateCompletion", GameState.stateCompletion);
}