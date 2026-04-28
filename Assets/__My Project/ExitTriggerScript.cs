using UnityEngine;

public class ExitTriggerScript : MonoBehaviour
{
    public ElevatorDoorController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.OnPlayerExitElevator();
        }
    }
}