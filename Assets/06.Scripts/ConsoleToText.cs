using TMPro;
using UnityEngine;

public class ConsoleToText : MonoBehaviour
{
    public TextMeshProUGUI DebugText;
    private string m_InfoText = "";
	 
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
	 
    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
        ClearLog();
    }
	 
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // "Look rotation viewing vector is zero" 메시지는 무시
        if (!logString.Contains("Look rotation viewing vector is zero"))
        {
            m_InfoText = logString + "\n" + m_InfoText;
        }
    }
	 
    private void OnGUI()
    {
        DebugText.text = m_InfoText;
    }
	 
    public void ClearLog()
    {
        m_InfoText = "";
    }
}
