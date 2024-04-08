using UnityEngine;

public class MachineFaceController : MonoBehaviour
{
    private Transform faceplate, eyeSocketLeft, eyeLeft, eyeStalkLeft, eyeSocketRight, eyeRight, eyeStalkRight, mouth, mouthStalk;

    // eye data
    private float eyeLeft_rotSpeed;
    private float eyeRight_rotSpeed;
    [SerializeField] private float eyeAccel;
    [SerializeField] private float eyeMaxSpeed;
    [SerializeField] private float eyeJitterChance;
    [SerializeField] private float eyeJitterProportion;
    
    // eye socket data
    private Vector3 eyeSocketLeft_initialForward;
    private Vector3 eyeSocketRight_initialForward;

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

    // sounds
    [SerializeField] private AudioSource eyeLeftSpinSnd;
    [SerializeField] private AudioSource eyeLeftSnagSnd;
    [SerializeField] private AudioSource eyeLeftPopSnd;
    [SerializeField] private AudioSource eyeRightSpinSnd;
    [SerializeField] private AudioSource eyeRightSnagSnd;
    [SerializeField] private AudioSource eyeRightPopSnd;
    [SerializeField] private AudioSource mouthSpinSnd;
    [SerializeField] private AudioSource mouthPopSnd;
    [SerializeField] private AudioSource headBobSnd;

    private float GetRandomFacePlateTarget() => fp_initialRot + Random.Range(2, 11);
    private bool fp_isAscending => fp_targetAng != fp_initialRot;

    private void Awake()
    {
        // establish transform references
        faceplate      = transform.Find("facePlate");
        eyeSocketLeft  = faceplate.Find("eyeSocketLeft");
        eyeSocketRight = faceplate.Find("eyeSocketRight");
        eyeLeft        = eyeSocketLeft.Find("eyeLeft");
        eyeRight       = eyeSocketRight.Find("eyeRight");
        eyeStalkLeft   = eyeLeft.Find("eyeStalkLeft");
        eyeStalkRight  = eyeRight.Find("eyeStalkRight");
        mouth          = faceplate.Find("mouth");
        mouthStalk     = mouth.Find("mouthStalk");

        eyeSocketLeft_initialForward = eyeSocketLeft.forward;
        eyeSocketRight_initialForward = eyeSocketRight.forward;

        fp_initialRot = faceplate.localEulerAngles.x;
        fp_targetAng = GetRandomFacePlateTarget();
    }

    private void Update()
    {
        // rotate eyes jaggedly
        eyeLeft_rotSpeed = Mathf.MoveTowards(eyeLeft_rotSpeed, eyeMaxSpeed, eyeAccel * Time.deltaTime);
        if (Random.Range(0,100f) <= eyeJitterChance * Time.deltaTime)
        {
            eyeLeft_rotSpeed *= eyeJitterProportion;
            eyeLeftSnagSnd.Play();
        }
        eyeRight_rotSpeed = Mathf.MoveTowards(eyeRight_rotSpeed, eyeMaxSpeed, eyeAccel * Time.deltaTime);
        if (Random.Range(0, 100f) <= eyeJitterChance * Time.deltaTime)
        {
            eyeRight_rotSpeed *= eyeJitterProportion;
            eyeRightSnagSnd.Play();
        }

        eyeLeft.Rotate(Vector3.forward, eyeLeft_rotSpeed * Time.deltaTime);
        eyeRight.Rotate(Vector3.forward, -eyeRight_rotSpeed * Time.deltaTime);

        // tilt eye sockets towards player
        eyeSocketLeft.forward  = Vector3.Lerp(eyeSocketLeft_initialForward,  (Player.transform.position - eyeSocketLeft.position).normalized,  .15f);
        eyeSocketRight.forward = Vector3.Lerp(eyeSocketRight_initialForward, (Player.transform.position - eyeSocketRight.position).normalized, .15f);

        // pop out eye/mouth stalks
        if ((Time.time - esl_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
        {
            esl_lastTriggerTime = Time.time;
            eyeLeftPopSnd.Play();
        }
        eyeStalkLeft.localPosition  = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - esl_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esl_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                    : Mathf.Lerp(1, 0, (Time.time - esl_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));
        if ((Time.time - esr_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
        {
            esr_lastTriggerTime = Time.time;
            eyeRightPopSnd.Play();
        }
        eyeStalkRight.localPosition = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - esr_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - esr_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                    : Mathf.Lerp(1, 0, (Time.time - esr_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));
        if ((Time.time - ms_lastTriggerTime > 2 * eyeStalkMoveHDur) && Random.Range(0f, 100f) < Time.deltaTime * eyeStalkMoveOdds)
        {
            ms_lastTriggerTime = Time.time;
            mouthPopSnd.Play();
        }
        mouthStalk.localPosition = Vector3.forward *
            (eyeStalkMoveDist * (Time.time - ms_lastTriggerTime < eyeStalkMoveHDur ? Mathf.Lerp(0, 1, (Time.time - ms_lastTriggerTime) / eyeStalkMoveHDur)
                                                                                   : Mathf.Lerp(1, 0, (Time.time - ms_lastTriggerTime - eyeStalkMoveHDur) / eyeStalkMoveHDur)));

        // rotate mouth back/forth
        mouth.localEulerAngles = new(mouth.localEulerAngles.x, mouth.localEulerAngles.y, Mathf.Sin(Time.time) * 10);

        // rotate face plate up/down
        fp_rotSpeed = Mathf.MoveTowards(fp_rotSpeed, faceplateMaxRotSpeed * (fp_isAscending ? 1 : -1),
            (faceplateRotAccel + Mathf.Abs(fp_targetAng - faceplate.localEulerAngles.x)) * Time.deltaTime);
        for (float moveDistRem = fp_rotSpeed * Time.deltaTime;;)
        {
            float totalDist = Mathf.Abs(fp_targetAng - faceplate.localEulerAngles.x);
            if (totalDist > Mathf.Abs(moveDistRem))
            {
                faceplate.localEulerAngles = new(faceplate.localEulerAngles.x + moveDistRem, faceplate.localEulerAngles.y, faceplate.localEulerAngles.z);
                break;
            }
            faceplate.localEulerAngles = new(fp_targetAng, faceplate.localEulerAngles.y, faceplate.localEulerAngles.z);
            fp_targetAng = fp_isAscending ? fp_initialRot : GetRandomFacePlateTarget();
            moveDistRem = Mathf.MoveTowards(moveDistRem, 0, totalDist);
        }

        eyeLeftSpinSnd.pitch = Mathf.Sqrt(eyeLeft_rotSpeed) * .06f;
        eyeRightSpinSnd.pitch = Mathf.Sqrt(eyeRight_rotSpeed) * .06f;
        mouthSpinSnd.pitch = Mathf.Sqrt(1 - Mathf.Abs(Mathf.Sin(Time.time))) * .45f;
        headBobSnd.pitch = Mathf.Pow(Mathf.Abs(fp_rotSpeed), .25f) * 1;
    }
}
