using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Object = System.Object;

public enum LogSeverity
{
    /// <summary>
    ///   <para>LogType used for Errors.</para>
    /// </summary>
    Error,
    /// <summary>
    ///   <para>LogType used for Asserts. (These could also indicate an error inside Unity itself.)</para>
    /// </summary>
    Assert,
    /// <summary>
    ///   <para>LogType used for Warnings.</para>
    /// </summary>
    Warning,
    /// <summary>
    ///   <para>LogType used for regular log messages.</para>
    /// </summary>
    Log,
    /// <summary>
    ///   <para>LogType used for Exceptions.</para>
    /// </summary>
    Exception,
    /// <summary>
    ///  <para>Log type used by system messages</para>
    /// </summary>
    System,
    
}

public struct Message
{
    public string Content;
    public LogSeverity Severity;
    public DateTime Timestamp;
    public Object Messenger;

    public Message(string content, LogSeverity severity, DateTime dateTime, Object messenger)
    {
        Content = content;
        Severity = severity;
        Timestamp = dateTime;
        Messenger = messenger;
    }
}
[CreateAssetMenu(fileName = "DebugConfig", menuName = "Debug/Debug Config", order = 1)]
public class DebugConfig : ScriptableObject
{
    public int maximumCachedLogMessages = 1000;
}


public class DebugLog: Singleton<DebugLog>
{
    public Queue<Message> messageQueue { private set; get; } = new Queue<Message>();
    public DebugConfig debugConfig;
    
    public Action<Message> OnLogMessage;
    
    
    private bool _debugConsoleOpen = false;
    [SerializeField] private DebugConsole _debugConsole;
    
    [SerializeField] private GameObject _debugConsoleFirstSelected;

    protected override void Awake()
    {
        base.Awake();
        debugConfig = Resources.Load<DebugConfig>("DebugConfig");
        if (!debugConfig)
        {
            Debug.LogWarning($"Debug config file not found. Creating...");
            debugConfig = ScriptableObject.CreateInstance<DebugConfig>();
            if (!debugConfig)
            {
                throw new Exception("Something went wrong creating the debug config, stopping");
            }
        }
        
        
        
        
        
        Application.logMessageReceived += CreateMessageFromUnity;
    }

    void Update()
    {
        if(!_debugConsole) return;
        if (InputManager.Instance.DebugInput)
        {
            if (_debugConsoleOpen)
            {
                CloseLog();
            }
            else
            {
                OpenLog();
            }
        }

        
    }

    public void CloseLog()
    {
        _debugConsoleOpen = false;
        
        EventSystem.current.SetSelectedGameObject(null);
        _debugConsole.gameObject.SetActive(false);
        
    }
    public void OpenLog()
    {
        _debugConsoleOpen = true;
        _debugConsole.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(_debugConsoleFirstSelected);
    }

    public static void PrintLog(string message, LogSeverity severity = LogSeverity.Log, Object messenger = null)
    {
        Message msg = new Message(message, severity, DateTime.Now, messenger);


        Instance.InternalLog(msg);
    }

    private void InternalLog(Message message)
    {
        if (messageQueue.Count > debugConfig.maximumCachedLogMessages)
        {
            messageQueue.Dequeue();
        }
        
        messageQueue.Enqueue(message);
        OnLogMessage?.Invoke(message);
    }

    private void CreateMessageFromUnity(string condition, string stackTrace, LogType type)
    {
        if(!Application.isPlaying) return;
        if(this == null) return;
        LogSeverity severity = LogSeverity.Log;
        switch (type)
        {
            case LogType.Error:
                severity = LogSeverity.Error;
                break;
            case LogType.Warning:
                severity = LogSeverity.Warning;
                break;
            case LogType.Assert:
                severity = LogSeverity.Assert;
                break;
            case LogType.Exception:
                severity = LogSeverity.Exception;
                break;
            case LogType.Log:
                severity = LogSeverity.Log;
                break;
            
        }
        condition = $"[Unity] {severity}: {condition}";
        Message msg = new Message(condition, severity, DateTime.Now, null);
        
        InternalLog(msg);
    }
    


    void OnDisable()
    {
        Application.logMessageReceived -= CreateMessageFromUnity;
    }
}
