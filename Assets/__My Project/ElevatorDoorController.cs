using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    public Transform doorLeft;
    public Transform doorRight;

    public float openDistance = 0.5f;
    public float speed = 2f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = true;

    void Start()
    {
        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;

        // 方向
        leftOpenPos = leftClosedPos + new Vector3(openDistance, 0, 0);
        rightOpenPos = rightClosedPos + new Vector3(-openDistance, 0, 0);
    }

    void Update()
    {
        Vector3 targetLeft = isOpen ? leftOpenPos : leftClosedPos;
        Vector3 targetRight = isOpen ? rightOpenPos : rightClosedPos;

        doorLeft.localPosition = Vector3.Lerp(doorLeft.localPosition, targetLeft, Time.deltaTime * speed);
        doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, targetRight, Time.deltaTime * speed);
    }

    public void OpenDoor()
    {
        isOpen = true;
    }
}
