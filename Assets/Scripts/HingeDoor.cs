using UnityEngine;

public class HingeDoor : MonoBehaviour
{
    [SerializeField] private float rotSensitivity = 1;
    [SerializeField] private float rotDecel = 2;
    private Vector2 rotSpeed;
    private Collider col;
    private bool interacting;
    [SerializeField] private AudioSource rotateSnd;
    [SerializeField] private bool locked;
    [SerializeField] private MetaballManager metaMan;
    [SerializeField] private RadiationManager radMan;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
    }

    private void OnMouseDown()
    {
        if (!locked)
        {
            interacting = Player.InRangeOf(col);
            if (!enabled && (enabled |= interacting)) rotateSnd.Play();
        }
    }

    public void Reset()
    {
        transform.localEulerAngles = new(transform.localEulerAngles.x, 0, transform.localEulerAngles.z);
        rotSpeed = Vector2.zero;
    }
    public void ToggleLock() => radMan.enabled = !(locked = !locked);

    public float openRatio => (360 - transform.localEulerAngles.y) % 360 / 125f;
    public int isOpen => transform.localEulerAngles.y < 355 && transform.localEulerAngles.y > 0 ? 1 : 0;
    public int isClosed => 1 - isOpen;

    private void Update()
    {
        // rebalance
        if (GameState.rebalancing)
        {
            interacting = false;
            rotSpeed.x = -400;
            transform.localEulerAngles = new(transform.localEulerAngles.x,
                                             transform.localEulerAngles.y - rotSpeed.x * Time.deltaTime,
                                             transform.localEulerAngles.z);
            if (transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 235)
                Reset();
        }
        else
        {
            // accelerate
            if (interacting = interacting && Input.GetMouseButton(0) && Player.InRetainRangeOf(col))
                rotSpeed.x += Input.GetAxis("Mouse X") * rotSensitivity;

            // rotate
            transform.localEulerAngles = new(transform.localEulerAngles.x,
                                             transform.localEulerAngles.y - rotSpeed.x * Time.deltaTime,
                                             transform.localEulerAngles.z);

            // decelerate
            rotSpeed.x = Mathf.MoveTowards(rotSpeed.x, 0, rotDecel * Time.deltaTime);

            // stop or bounce if moving quickly enough
            if (transform.localEulerAngles.y > 0 && transform.localEulerAngles.y < 235)
            {
                transform.localEulerAngles = new(transform.localEulerAngles.x, 
                                                 transform.localEulerAngles.y > 112.5f ? 235: 0,
                                                 transform.localEulerAngles.z);
                rotSpeed.x = Mathf.MoveTowards(-.2f * rotSpeed.x, 0, 25);
            }
        }

        // rotation sound
        if (enabled = rotSpeed.x != 0 || interacting)
            rotateSnd.pitch = Mathf.Sqrt(Mathf.Abs(rotSpeed.x)) * .3f;
        else rotateSnd.Stop();
    }
}
