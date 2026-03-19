using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ElevatorDoorController : MonoBehaviour
{
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

    public GameObject npcStudent;
    public Transform npcStartPoint;
    public Transform npcStandPoint;
    public float npcMoveSpeed = 1.5f;

    public float travelDelay = 5f;
    private Coroutine travelCoroutine;

    public GameObject dialoguePanel;   // 第一轮
    public GameObject followUpPanel;   // 第二轮

    public GameObject resultPanel;
    public TextMeshProUGUI resultLevelText;

    public MonoBehaviour raycastInteractor;

    public GameObject speechBubble;
    public TextMeshProUGUI speechBubbleText;

    void Start()
    {
        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;

        leftOpenPos = leftClosedPos + new Vector3(openDistance, 0, 0);
        rightOpenPos = rightClosedPos + new Vector3(-openDistance, 0, 0);

        if (npcStudent != null)
            npcStudent.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (followUpPanel != null)
            followUpPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(false);

        if (speechBubble != null)
            speechBubble.SetActive(false);
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
            StopCoroutine(autoCloseCoroutine);

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
            StopCoroutine(travelCoroutine);

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

    public void ShowSpeechBubble(string message)
    {
        if (speechBubble != null)
            speechBubble.SetActive(true);

        if (speechBubbleText != null)
            speechBubbleText.text = message;
    }

    public void HideSpeechBubble()
    {
        if (speechBubble != null)
            speechBubble.SetActive(false);
    }

    IEnumerator StartConversation()
    {
        yield return new WaitForSeconds(0.5f);

        ShowSpeechBubble("Morning");

        yield return new WaitForSeconds(1.5f);
        HideSpeechBubble();

        yield return new WaitForSeconds(0.5f);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (raycastInteractor != null)
            raycastInteractor.enabled = false;
    }

    public void OnClickMorning()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(MorningBranch());
    }

    IEnumerator MorningBranch()
    {
        ShowSpeechBubble("I'm heading to class now. What about you?");

        yield return new WaitForSeconds(2f);
        HideSpeechBubble();

        yield return new WaitForSeconds(0.3f);

        if (followUpPanel != null)
            followUpPanel.SetActive(true);
    }

    public void OnClickHi()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(AskClassBranch());
    }

    IEnumerator AskClassBranch()
    {
        ShowSpeechBubble("Yeah, I have a lecture soon.");

        yield return new WaitForSeconds(2f);
        HideSpeechBubble();

        ShowResultPanel("High");
    }

    public void OnClickJustNod()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(SilentBranch());
    }

    IEnumerator SilentBranch()
    {
        ShowSpeechBubble("...");

        yield return new WaitForSeconds(2f);
        HideSpeechBubble();

        ShowResultPanel("Low");
    }

    public void OnClickSame()
    {
        if (followUpPanel != null)
            followUpPanel.SetActive(false);

        ShowResultPanel("Medium");
    }

    public void OnClickStudy()
    {
        if (followUpPanel != null)
            followUpPanel.SetActive(false);

        ShowResultPanel("High");
    }

    void ShowResultPanel(string level)
    {
        HideSpeechBubble();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (followUpPanel != null)
            followUpPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultLevelText != null)
        {
            resultLevelText.text = level;

            if (level == "High")
            {
                resultLevelText.color = Color.green;
            }
            else if (level == "Medium")
            {
                resultLevelText.color = Color.yellow;
            }
            else if (level == "Low")
            {
                resultLevelText.color = Color.red;
            }
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (raycastInteractor != null)
            raycastInteractor.enabled = false;
    }

    public void RestartSceneState()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}