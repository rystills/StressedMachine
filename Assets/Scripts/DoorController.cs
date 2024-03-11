using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Transform leftDoor, rightDoor;
    [SerializeField] private float activationRange;
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
    }

    private void Update()
    {
        if (wantsActive = Vector3.Distance(transform.position, Player.transform.position) < activationRange)
        {
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorTargetPos, openSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorTargetPos, openSpeed * Time.deltaTime);
        }
        else
        {
            leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftDoorInitialPos, openSpeed * Time.deltaTime);
            rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightDoorInitialPos, openSpeed * Time.deltaTime);
        }
    }
}
