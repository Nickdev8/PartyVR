using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Logger : MonoBehaviour
{
    public TMP_Text textLogComponent;
    public Transform cameraAnchor;
    
    // List to keep track of log messages
    private List<string> _logMessages = new List<string>();
    
    // follow the camara
    private void LateUpdate () {
        transform.position = Vector3.Lerp(transform.position, cameraAnchor.position, Time.deltaTime * 100);
        transform.rotation = Quaternion.Lerp(transform.rotation, cameraAnchor.rotation, Time.deltaTime * 100);
    }

    public void AddLog(string message)
    {
        StartCoroutine(Log(message));
    }

    private IEnumerator Log(string message)
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
}