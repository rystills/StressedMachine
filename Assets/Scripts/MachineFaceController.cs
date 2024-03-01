using UnityEngine;

public class MachineFaceController : MonoBehaviour
{
    Transform faceplate, eyeLeft, eyeStalkLeft, eyeRight, eyeStalkRight, mouth, mouthStalk;

    // eye data
    private float eyeLeft_rotSpeed;
    private float eyeRight_rotSpeed;
    [SerializeField] private float eyeAccel;
    [SerializeField] private float eyeMaxSpeed;
    [SerializeField] private float eyeJitterChance;
    [SerializeField] private float eyeJitterProportion;
    
    // eye stalk data
    private float esl_lastTriggerTime = -999;
    private float esr_lastTriggerTime = -999;
    private float ms_lastTriggerTime = -999;
    [SerializeField] private float eyeStalkMoveHDur;
    [SerializeField] private float eyeStalkMoveDist;
    [SerializeField] private float eyeStalkMoveOdds;

    // face plate data
    private float fp_targetAng;
    private float fp_rotSpeed;
    [SerializeField] private float faceplateRotAccel;
    [SerializeField] private float faceplateMaxRotSpeed;
    private float fp_initialRot;
    private float GetRandomFacePlateTarget() => fp_initialRot + Random.Range(2, 11);
    private bool fp_isAscending => fp_targetAng != fp_initialRot;

    private void Awake()
    {
        // establish transform references
        faceplate     = transform.Find("facePlate");
        eyeLeft       = faceplate.Find("eyeLeft");
        eyeRight      = faceplate.Find("eyeRight");
        eyeStalkLeft  = eyeLeft.Find("eyeStalkLeft");
        eyeStalkRight = eyeRight.Find("eyeStalkRight");
        mouth = faceplate.Find("mouth");
        mouthStalk = mouth.Find("mouthStalk");

        fp_initialRot = faceplate.localEulerAngles.x;
        fp_targetAng = GetRandomFacePlateTarget();
    }

    private void Update()
    {
        // rotate eyes jaggedly
        eyeLeft_rotSpeed = Mathf.MoveTowards(eyeLeft_rotSpeed, eyeMaxSpeed, eyeAccel * Time.deltaTime);
        if (Random.Range(0,100f) <= eyeJitterChance * Time.deltaTime)
            eyeLeft_rotSpeed *= eyeJitterProportion;
        eyeRight_rotSpeed = Mathf.MoveTowards(eyeRight_rotSpeed, eyeMaxSpeed, eyeAccel * Time.deltaTime);
        if (Random.Range(0, 100f) <= eyeJitterChance * Time.deltaTime)
            eyeRight_rotSpeed *= eyeJitterProportion;

        eyeLeft.Rotate(Vector3.forward, eyeLeft_rotSpeed * Time.deltaTime);
        eyeRight.Rotate(Vector3.forward, -eyeRight_rotSpeed * Time.deltaTime);

        // pop out eye/mouth stalks
        if ((Time.time - esl_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
            esl_lastTriggerTime = Time.time;
        eyeStalkLeft.localPosition  = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - esl_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esl_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                    : Mathf.Lerp(1, 0, (Time.time - esl_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));
        if ((Time.time - esr_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
            esr_lastTriggerTime = Time.time;
        eyeStalkRight.localPosition = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - esr_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esr_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                    : Mathf.Lerp(1, 0, (Time.time - esr_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));
        if ((Time.time - ms_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
            ms_lastTriggerTime = Time.time;
        mouthStalk.localPosition = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - ms_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - ms_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                   : Mathf.Lerp(1, 0, (Time.time - ms_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));

        // rotate mouth back/forth
        mouth.localEulerAngles = new Vector3(mouth.localEulerAngles.x, mouth.localEulerAngles.y, Mathf.Sin(Time.time) * 10);

        // rotate face plate up/down
        fp_rotSpeed = Mathf.MoveTowards(fp_rotSpeed, faceplateMaxRotSpeed * (fp_isAscending ? 1 : -1),
            (faceplateRotAccel + Mathf.Abs(fp_targetAng - faceplate.localEulerAngles.x)) * Time.deltaTime);
        for (float moveDistRem = fp_rotSpeed * Time.deltaTime;;)
        {
            float totalDist = Mathf.Abs(fp_targetAng - faceplate.localEulerAngles.x);
            if (totalDist > Mathf.Abs(moveDistRem))
            {
                faceplate.localEulerAngles = new Vector3(faceplate.localEulerAngles.x + moveDistRem, faceplate.localEulerAngles.y, faceplate.localEulerAngles.z);
                break;
            }
            faceplate.localEulerAngles = new Vector3(fp_targetAng, faceplate.localEulerAngles.y, faceplate.localEulerAngles.z);
            fp_targetAng = fp_isAscending ? fp_initialRot : GetRandomFacePlateTarget();
            moveDistRem = Mathf.MoveTowards(moveDistRem, 0, totalDist);
        }
    }
}
