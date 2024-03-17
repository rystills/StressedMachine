using UnityEngine;

public class HingeDoor : MonoBehaviour
{
    [SerializeField] private float rotSensitivity = 1;
    [SerializeField] private float rotDecel = 2;
    private Vector2 rotSpeed;
    private Collider col;
    private bool interacting;

    private void Awake()
    {
        col = GetComponent<Collider>();
        enabled = false;
    }

    private void OnMouseDown()
    {
        interacting = Player.InRangeOf(col);
        enabled |= interacting;
    }

    public float openRatio => (360 - transform.localEulerAngles.y) % 360 / 125f;
    public int isOpen => transform.localEulerAngles.y < 355 && transform.localEulerAngles.y != 0 ? 1 : 0;
    public int isClosed => 1 - isOpen;

    private void Update()
    {
        // accelerate
        if (interacting = interacting && Input.GetMouseButton(0))
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

        enabled = rotSpeed.x != 0 || interacting;
    }
}
