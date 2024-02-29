using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineFaceController : MonoBehaviour
{
    Transform eyeLeft,  eyeStalkLeft,
              eyeRight, eyeStalkRight,
              faceplate;

    private float esl_lastTriggerTime = -999;
    private float esr_lastTriggerTime = -999;
    [SerializeField] private float eyeStalkMoveHDur;
    [SerializeField] private float eyeStalkMoveDist;
    [SerializeField] private float eyeStalkMoveOdds;
    
    private void Awake()
    {
        faceplate     = transform.Find("facePlate");
        eyeLeft       = faceplate.Find("eyeLeft");
        eyeRight      = faceplate.Find("eyeRight");
        eyeStalkLeft  = eyeLeft.Find("eyeStalkLeft");
        eyeStalkRight = eyeRight.Find("eyeStalkRight");
    }

    private void Update()
    {
        // rotate eyes
        eyeLeft.Rotate(Vector3.forward, 100f * Time.deltaTime);
        eyeRight.Rotate(Vector3.forward, -100f * Time.deltaTime);

        if ((Time.time - esl_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
            esl_lastTriggerTime = Time.time;

        if ((Time.time - esr_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
            esr_lastTriggerTime = Time.time;

        // pop out eye stalks
        eyeStalkLeft.localPosition  = Vector3.forward * (eyeStalkMoveDist * (Time.time - esl_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esl_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                                                                : Mathf.Lerp(1, 0, (Time.time - esl_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));

        eyeStalkRight.localPosition = Vector3.forward * (eyeStalkMoveDist * (Time.time - esr_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esr_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                                                                : Mathf.Lerp(1, 0, (Time.time - esr_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));
    }
}
