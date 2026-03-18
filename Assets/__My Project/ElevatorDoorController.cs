using System.Collections;
using UnityEngine;

public class ElevatorDoorController : MonoBehaviour
{
    // Door
    public Transform doorLeft;
    public Transform doorRight;

    public float openDistance = 1.5f;
    public float speed = 2f;
    public float autoCloseDelay = 4f;

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;
    private Vector3 leftOpenPos;
    private Vector3 rightOpenPos;

    private bool isOpen = false;
    private Coroutine autoCloseCoroutine;

    // NPC
    public GameObject npcStudent;
    public Transform npcStartPoint;
    public Transform npcStandPoint;
    public float npcMoveSpeed = 1.5f;

    // elevator
    public float travelDelay = 5f;
    private Coroutine travelCoroutine;

    // Dialogue UI
    public GameObject dialoguePanel;

    public MonoBehaviour raycastInteractor;

    void Start()
    {
        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;

        
        leftOpenPos = leftClosedPos + new Vector3(openDistance, 0, 0);
        rightOpenPos = rightClosedPos + new Vector3(-openDistance, 0, 0);

        if (npcStudent != null)
        {
            npcStudent.SetActive(false);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    void Update()
    {
        Vector3 targetLeft = isOpen ? leftOpenPos : leftClosedPos;
        Vector3 targetRight = isOpen ? rightOpenPos : rightClosedPos;

        doorLeft.localPosition = Vector3.Lerp(
            doorLeft.localPosition,
            targetLeft,
            Time.deltaTime * speed
        );

        doorRight.localPosition = Vector3.Lerp(
            doorRight.localPosition,
            targetRight,
            Time.deltaTime * speed
        );
    }

    public void OpenDoor()
    {
        isOpen = true;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
        }

        autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
    }

    public void CloseDoor()
    {
        isOpen = false;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        isOpen = false;
        autoCloseCoroutine = null;
    }

    public void SimulateElevatorRide()
    {
        CloseDoor();

        if (travelCoroutine != null)
        {
            StopCoroutine(travelCoroutine);
        }

        travelCoroutine = StartCoroutine(ElevatorRideRoutine());
    }

    IEnumerator ElevatorRideRoutine()
    {
        yield return new WaitForSeconds(travelDelay);

        if (npcStudent == null || npcStartPoint == null || npcStandPoint == null)
        {
            Debug.LogWarning("NPC references are missing.");
            yield break;
        }

        npcStudent.SetActive(true);
        npcStudent.transform.position = npcStartPoint.position;
        npcStudent.transform.rotation = npcStartPoint.rotation;

        OpenDoor();

        StartCoroutine(MoveNPC());

        travelCoroutine = null;
    }

    IEnumerator MoveNPC()
    {
        while (Vector3.Distance(npcStudent.transform.position, npcStandPoint.position) > 0.05f)
        {
            npcStudent.transform.position = Vector3.MoveTowards(
                npcStudent.transform.position,
                npcStandPoint.position,
                npcMoveSpeed * Time.deltaTime
            );

            yield return null;
        }

        StartCoroutine(StartConversation());
    }

    IEnumerator StartConversation()
{
    yield return new WaitForSeconds(0.5f);

    Debug.Log("NPC: Morning");

    yield return new WaitForSeconds(1f);

    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(true);
    }

    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    if (raycastInteractor != null)
    {
        raycastInteractor.enabled = false;
    }
}

    public void OnClickMorning()
{
    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    if (raycastInteractor != null)
    {
        raycastInteractor.enabled = true;
    }

    StartCoroutine(NPCReplyGood());
}

   public void OnClickHi()
{
    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    if (raycastInteractor != null)
    {
        raycastInteractor.enabled = true;
    }

    StartCoroutine(NPCReplyNeutral());
}

    public void OnClickJustNod()
{
    if (dialoguePanel != null)
    {
        dialoguePanel.SetActive(false);
    }

    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    if (raycastInteractor != null)
    {
        raycastInteractor.enabled = true;
    }

    StartCoroutine(NPCReplySilent());
}

    IEnumerator NPCReplyGood()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("You: Morning!");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("NPC: Nice to see you.");
    }

    IEnumerator NPCReplyNeutral()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("You: Hi, are you heading to class?");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("NPC: Yeah, I have a lecture soon.");
    }

    IEnumerator NPCReplySilent()
    {
        yield return new WaitForSeconds(0.5f);
        Debug.Log("You just nod.");
        yield return new WaitForSeconds(0.5f);
        Debug.Log("NPC looks a little awkward.");
    }
}