using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logger : MonoBehaviour
{
    public TMP_Text textLogComponent;
    public Transform cameraAnchor;
    public Vector3 offset;
    
    // List to keep track of log messages
    private List<string> _logMessages = new List<string>();

    private void Start()
    {
        textLogComponent.text = string.Empty;
    }

    private void LateUpdate () {
        // Calculate the target position relative to the camera's local space.
        Vector3 desiredPosition = cameraAnchor.TransformPoint(offset);
    
        // Smoothly move toward the desired position.
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
    
        // Optionally, smooth the rotation as well.
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraAnchor.rotation, Time.deltaTime * 10);

    }

    public void LogErrorText(string message)
    {
        StartCoroutine(LogText(message));
    }

    private IEnumerator LogText(string message)
    {
        // Add the message to the list and update the display
        _logMessages.Add(message);
        UpdatePlayerLog();

        yield return new WaitForSeconds(2f);

        // Remove the message (removes the first occurrence)
        _logMessages.Remove(message);
        UpdatePlayerLog();
    }

    private void UpdatePlayerLog()
    {
        textLogComponent.text = string.Join("\n", _logMessages.ToArray());
    }


    public void LogIcon()
    {
        
    }
}