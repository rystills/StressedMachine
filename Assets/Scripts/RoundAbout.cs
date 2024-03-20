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

        enabled = rotSpeed != 0 || interacting;
    }
}
