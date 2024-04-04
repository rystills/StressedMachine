using UnityEngine;

public class RoundAbout : MonoBehaviour
{
    [SerializeField] private float rotSensitivity = 1;
    [SerializeField] private float rotDecel = 2;
    [SerializeField] private float rotMax = 20;
    [SerializeField] private Collider col;
    public bool interacting;
    [SerializeField] private float rotSpeed;
    [SerializeField] private Hourglass hourglass;
    [SerializeField] private AudioSource rotateSnd;
    public bool locked;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
    }

    private void OnMouseDown()
    {
        if (!locked)
        {
            if (interacting = Player.InRangeOf(col)) Player.LendControl();
            if (!enabled && (enabled |= interacting)) rotateSnd.Play();
        }
    }

    private void Update()
    {
        if (interacting = interacting && Input.GetMouseButton(0) && Player.InRetainRangeOf(col))
            rotSpeed = Mathf.Clamp(rotSpeed + Input.GetAxisRaw("Vertical") * rotSensitivity * Time.deltaTime, -rotMax, rotMax);
        else Player.ReturnControl();

        // rotate
        transform.localEulerAngles = new(transform.localEulerAngles.x,
                                         transform.localEulerAngles.y - rotSpeed * Time.deltaTime,
                                         transform.localEulerAngles.z);

        hourglass.AddRotation(rotSpeed * Time.deltaTime);

        // decelerate
        rotSpeed = Mathf.MoveTowards(rotSpeed, 0, rotDecel * Time.deltaTime);

        // rotation sound
        if (enabled = rotSpeed != 0 || interacting)
            rotateSnd.pitch = Mathf.Sqrt(Mathf.Abs(rotSpeed)) * .3f;
        else rotateSnd.Stop();
    }
}
