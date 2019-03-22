using UnityEngine;

public class LogEvent : MonoBehaviour
{
    private EventLog eventLog;

    void Start()
    {
        eventLog = GetComponent<EventLog>();
    }

    void LateUpdate()
    {
        if (Input.GetKey(KeyCode.A))
            eventLog.AddEvent("Player Moves Left");

        if (Input.GetKey(KeyCode.D))
            eventLog.AddEvent("Player Moves Right");
    }
}
