using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Transform leftDoor, rightDoor;
    [SerializeField] private float openSpeed;
    [SerializeField] private float openDistance;
    private bool wantsActive;
    private Vector3 leftDoorInitialPos, rightDoorInitialPos, leftDoorTargetPos, rightDoorTargetPos;
    [SerializeField] private Material lightMat;
    [SerializeField] private Light light;
    private Collider triggerCol;

    private void Awake()
    {
        leftDoor = transform.Find("doorLeft");
        rightDoor = transform.Find("doorRight");
        leftDoorInitialPos = leftDoor.localPosition;
        rightDoorInitialPos = rightDoor.localPosition;
        leftDoorTargetPos = leftDoorInitialPos + leftDoor.right * openDistance;
        rightDoorTargetPos = rightDoorInitialPos - leftDoor.right * openDistance;
        triggerCol = GetComponent<Collider>();
        enabled = false;
        ToggleLock();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            wantsActive = true;
            enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            wantsActive = false;
            enabled = true;
        }
    }

    public void ToggleLock()
    {
        if (triggerCol.enabled = !triggerCol.enabled)
        {
            lightMat.color = new(.5f, 1, .5f);
            lightMat.SetColor("_EmissionColor", new Color(0, .5f, 0) * 4);
            light.color = Color.green;
        }
        else
        {
            lightMat.color = new(1, .5f, .5f);
            lightMat.SetColor("_EmissionColor", new Color(.5f, 0, 0) * 4);
            light.color = Color.red;
        }
    }

    private void Update()
    {
        if (wantsActive)
        {
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorTargetPos, openSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorTargetPos, openSpeed * Time.deltaTime);
            enabled = leftDoor.localPosition != leftDoorTargetPos;
        }
        else
        {
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorInitialPos, openSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorInitialPos, openSpeed * Time.deltaTime);
            enabled = leftDoor.localPosition != leftDoorInitialPos;
        }
    }
}
