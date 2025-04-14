using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class VRDebugLogger : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI debugText;
    [SerializeField] private int maxLogLines = 10;
    [SerializeField] private bool showTimestamp = true;
    
    private Queue<string> logLines = new Queue<string>();
    
    private static VRDebugLogger instance;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 빈 텍스트로 시작
        UpdateDebugText();
    }
    
    public static void Log(string message)
    {
        if (instance != null)
        {
            instance.AddLogMessage(message);
        }
    }
    
    private void AddLogMessage(string message)
    {
        string logEntry = showTimestamp 
            ? $"[{DateTime.Now.ToString("HH:mm:ss.fff")}] {message}" 
            : message;
        
        logLines.Enqueue(logEntry);
        
        // 최대 로그 라인 수를 유지
        while (logLines.Count > maxLogLines)
        {
            logLines.Dequeue();
        }
        
        UpdateDebugText();
    }
    
    private void UpdateDebugText()
    {
        if (debugText != null)
        {
            debugText.text = string.Join("\n", logLines.ToArray());
        }
    }
    
    // 로그 지우기
    public static void Clear()
    {
        if (instance != null)
        {
            instance.logLines.Clear();
            instance.UpdateDebugText();
        }
    }
}