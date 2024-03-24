using UnityEngine;

public class RoundAbout : MonoBehaviour
{
    [SerializeField] private float rotSensitivity = 1;
    [SerializeField] private float rotDecel = 2;
    [SerializeField] private float rotMax = 20;
    private Collider col;
    public bool interacting;
    private float rotSpeed;
    [SerializeField] private Hourglass hourglass;
    [SerializeField] private AudioSource rotateSnd;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
    }

    private void OnMouseDown()
    {
        interacting = Player.InRangeOf(col);
        if (!enabled && (enabled |= interacting)) rotateSnd.Play();
    }

    private void Update()
    {
        if (interacting = interacting && Input.GetMouseButton(0))
            rotSpeed = Mathf.Clamp(rotSpeed + Input.GetAxisRaw("Vertical") * rotSensitivity, -rotMax, rotMax);

        // rotate
        transform.localEulerAngles = new(transform.localEulerAngles.x,
                                         transform.localEulerAngles.y - rotSpeed * Time.deltaTime,
                                         transform.localEulerAngles.z);

        hourglass.AddRotation(rotSpeed * Time.deltaTime);

        // decelerate
        rotSpeed = Mathf.MoveTowards(rotSpeed, 0, rotDecel * Time.deltaTime);

        // rotation sound
        if (enabled = rotSpeed != 0 || interacting)
            rotateSnd.pitch = Mathf.Sqrt(Mathf.Abs(rotSpeed)) * Mathf.Sign(rotSpeed) * .3f;
        else rotateSnd.Stop();
    }
}
