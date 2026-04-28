using UnityEngine;

public class BackZoneScript : MonoBehaviour
{
    public ElevatorDoorController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.playerInBackZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            controller.playerInBackZone = false;
        }
    }
}