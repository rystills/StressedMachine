using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Transform leftDoor, rightDoor;
    [SerializeField] private float openSpeed;
    [SerializeField] private float openDistance;
    private bool wantsActive;
    private Vector3 leftDoorInitialPos, rightDoorInitialPos, leftDoorTargetPos, rightDoorTargetPos;

    private void Awake()
    {
        leftDoor = transform.Find("doorLeft");
        rightDoor = transform.Find("doorRight");
        leftDoorInitialPos = leftDoor.localPosition;
        rightDoorInitialPos = rightDoor.localPosition;
        leftDoorTargetPos = leftDoorInitialPos + leftDoor.right * openDistance;
        rightDoorTargetPos = rightDoorInitialPos - leftDoor.right * openDistance;
        enabled = false;
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
