using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class Logger : MonoBehaviour
{
    // Assign this in the Inspector with your TextMeshProUGUI component
    public TMP_Text textLogComponent;

    // List to keep track of log messages
    private List<string> _logMessages = new List<string>();

    // Call this method to add a log entry
    public void AddLog(string message)
    {
        StartCoroutine(Log(message));
    }

    private IEnumerator Log(string message)
    {
        // Add the message to the list and update the display
        _logMessages.Add(message);
        UpdatePlayerLog();

        // Wait for 2 seconds before removing this message
        yield return new WaitForSeconds(2f);

        // Remove the message (removes the first occurrence)
        _logMessages.Remove(message);
        UpdatePlayerLog();
    }

    // Update the TextMeshProUGUI component based on current messages
    private void UpdatePlayerLog()
    {
        textLogComponent.text = string.Join("\n", _logMessages.ToArray());
    }
}