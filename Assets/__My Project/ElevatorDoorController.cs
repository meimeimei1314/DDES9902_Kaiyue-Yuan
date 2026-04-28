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

    [Header("Spatial Zone Check")]
    public bool playerInBackZone = false;

    [Header("Gaze-Based Spatial Interaction")]
    public Transform playerTransform;
    public Transform backLookTarget;
    public float backLookThreshold = 0.6f;

    [Header("Dialogue Panels")]
    public GameObject dialoguePanel;
    public GameObject morningFollowUpPanel;
    public GameObject askFollowUpPanel;
    public GameObject silentFollowUpPanel;
    public GameObject missedConversationPanel;

    [Header("Result")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultLevelText;

    [Header("Exit Guidance Sign")]
    public GameObject exitPromptSign;

    public MonoBehaviour raycastInteractor;

    [Header("Speech Bubble")]
    public GameObject speechBubble;
    public TextMeshProUGUI speechBubbleText;

    private string finalResult = "";
    private bool waitingForExit = false;

    void Start()
    {
        leftClosedPos = doorLeft.localPosition;
        rightClosedPos = doorRight.localPosition;

        leftOpenPos = leftClosedPos + new Vector3(openDistance, 0, 0);
        rightOpenPos = rightClosedPos + new Vector3(-openDistance, 0, 0);

        if (npcStudent != null) npcStudent.SetActive(false);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (morningFollowUpPanel != null) morningFollowUpPanel.SetActive(false);
        if (askFollowUpPanel != null) askFollowUpPanel.SetActive(false);
        if (silentFollowUpPanel != null) silentFollowUpPanel.SetActive(false);
        if (missedConversationPanel != null) missedConversationPanel.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(false);
        if (speechBubble != null) speechBubble.SetActive(false);
        if (exitPromptSign != null) exitPromptSign.SetActive(false);
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

    public void OpenDoorWithoutAutoClose()
    {
        isOpen = true;

        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
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

        StartCoroutine(CheckSpatialInteraction());
    }

    IEnumerator CheckSpatialInteraction()
    {
        yield return new WaitForSeconds(0.8f);

        bool lookingAtBackTarget = false;

        if (playerTransform != null && backLookTarget != null)
        {
            Vector3 directionToBack = backLookTarget.position - playerTransform.position;
            directionToBack.y = 0;

            Vector3 playerForward = playerTransform.forward;
            playerForward.y = 0;

            float dot = Vector3.Dot(playerForward.normalized, directionToBack.normalized);
            lookingAtBackTarget = dot > backLookThreshold;
        }

        if (playerInBackZone)
        {
            StartCoroutine(MissedByBackZoneRoutine());
        }
        else if (lookingAtBackTarget)
        {
            StartCoroutine(MissedByLookingBackRoutine());
        }
        else
        {
            StartCoroutine(StartConversation());
        }
    }

    IEnumerator MissedByBackZoneRoutine()
    {
        finalResult = "Low";

        ShowSpeechBubble("...");

        yield return new WaitForSeconds(3f);

        HideSpeechBubble();

        OpenDoorWithoutAutoClose();

        waitingForExit = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (raycastInteractor != null)
            raycastInteractor.enabled = true;

        if (exitPromptSign != null)
            exitPromptSign.SetActive(true);
    }

    IEnumerator MissedByLookingBackRoutine()
    {
        finalResult = "Low";

        yield return new WaitForSeconds(1f);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (raycastInteractor != null)
            raycastInteractor.enabled = false;

        if (missedConversationPanel != null)
            missedConversationPanel.SetActive(true);
    }

    public void OnClickMissedConversationOK()
    {
        if (missedConversationPanel != null)
            missedConversationPanel.SetActive(false);

        ShowResultPanel("Low");
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

        ShowSpeechBubble("Morning.");

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
        ShowSpeechBubble("Heading to class?");

        yield return new WaitForSeconds(2f);
        HideSpeechBubble();

        yield return new WaitForSeconds(0.3f);

        if (morningFollowUpPanel != null)
            morningFollowUpPanel.SetActive(true);
    }

    public void OnClickHaveClassSoon()
    {
        if (morningFollowUpPanel != null)
            morningFollowUpPanel.SetActive(false);

        StartCoroutine(HaveClassSoonBranch());
    }

    IEnumerator HaveClassSoonBranch()
    {
        ShowSpeechBubble("Same here. Hope your class goes well.");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("Medium"));
    }

    public void OnClickHeadingOut()
    {
        if (morningFollowUpPanel != null)
            morningFollowUpPanel.SetActive(false);

        StartCoroutine(HeadingOutBranch());
    }

    IEnumerator HeadingOutBranch()
    {
        ShowSpeechBubble("Nice. Have a good day.");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("Medium"));
    }

    public void OnClickHi()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(AskClassBranch());
    }

    IEnumerator AskClassBranch()
    {
        ShowSpeechBubble("Yeah, I have a lecture soon. What about you?");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        yield return new WaitForSeconds(0.3f);

        if (askFollowUpPanel != null)
            askFollowUpPanel.SetActive(true);
    }

    public void OnClickSameClass()
    {
        if (askFollowUpPanel != null)
            askFollowUpPanel.SetActive(false);

        StartCoroutine(SameClassBranch());
    }

    IEnumerator SameClassBranch()
    {
        ShowSpeechBubble("Nice, good luck with your class.");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("High"));
    }

    public void OnClickGoingToStudy()
    {
        if (askFollowUpPanel != null)
            askFollowUpPanel.SetActive(false);

        StartCoroutine(GoingToStudyBranch());
    }

    IEnumerator GoingToStudyBranch()
    {
        ShowSpeechBubble("That sounds productive. Good luck!");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("High"));
    }

    public void OnClickJustNod()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        StartCoroutine(SilentBranch());
    }

    IEnumerator SilentBranch()
    {
        ShowSpeechBubble("Oh, quiet morning?");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        yield return new WaitForSeconds(0.3f);

        if (silentFollowUpPanel != null)
            silentFollowUpPanel.SetActive(true);
    }

    public void OnClickTired()
    {
        if (silentFollowUpPanel != null)
            silentFollowUpPanel.SetActive(false);

        StartCoroutine(TiredBranch());
    }

    IEnumerator TiredBranch()
    {
        ShowSpeechBubble("No worries. Early mornings are hard.");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("Medium"));
    }

    public void OnClickSorryMorning()
    {
        if (silentFollowUpPanel != null)
            silentFollowUpPanel.SetActive(false);

        StartCoroutine(SorryMorningBranch());
    }

    IEnumerator SorryMorningBranch()
    {
        ShowSpeechBubble("All good. Morning.");

        yield return new WaitForSeconds(2.5f);
        HideSpeechBubble();

        StartCoroutine(EndConversationThenOpenDoor("Medium"));
    }

    IEnumerator EndConversationThenOpenDoor(string level)
    {
        finalResult = level;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (morningFollowUpPanel != null) morningFollowUpPanel.SetActive(false);
        if (askFollowUpPanel != null) askFollowUpPanel.SetActive(false);
        if (silentFollowUpPanel != null) silentFollowUpPanel.SetActive(false);
        if (missedConversationPanel != null) missedConversationPanel.SetActive(false);

        yield return new WaitForSeconds(5f);

        OpenDoorWithoutAutoClose();

        waitingForExit = true;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (raycastInteractor != null)
            raycastInteractor.enabled = true;

        if (exitPromptSign != null)
            exitPromptSign.SetActive(true);
    }

    public void OnPlayerExitElevator()
    {
        if (!waitingForExit) return;

        waitingForExit = false;

        if (exitPromptSign != null)
            exitPromptSign.SetActive(false);

        ShowResultPanel(finalResult);
    }

    void ShowResultPanel(string level)
    {
        HideSpeechBubble();

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (morningFollowUpPanel != null) morningFollowUpPanel.SetActive(false);
        if (askFollowUpPanel != null) askFollowUpPanel.SetActive(false);
        if (silentFollowUpPanel != null) silentFollowUpPanel.SetActive(false);
        if (missedConversationPanel != null) missedConversationPanel.SetActive(false);

        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultLevelText != null)
        {
            resultLevelText.text = level;

            if (level == "High")
                resultLevelText.color = Color.green;
            else if (level == "Medium")
                resultLevelText.color = Color.yellow;
            else if (level == "Low")
                resultLevelText.color = Color.red;
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