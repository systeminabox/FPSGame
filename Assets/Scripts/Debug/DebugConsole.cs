using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DebugConsole : MonoBehaviour
{
    
    [SerializeField] private  TMP_InputField inputField;
    [SerializeField] private ScrollRect scrollRect;
    
    private Dictionary<string, DebugCommand> _commands = new Dictionary<string, DebugCommand>();

    private void Awake()
    {
        RegisterAllCommands();
        
    }

    private void Start()
    {
        DebugLog.Instance.OnLogMessage += HandleMessage;
    }

    /// <summary>
    /// Uses reflection to find all public static methods with the DebugCommandAttribute.
    /// 
    /// </summary>
    private void RegisterAllCommands()
    {
        var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }

            foreach (var type in types)
            {
                foreach (var method in type.GetMethods(flags))
                {
                    var attr = method.GetCustomAttribute<DebugCommandAttribute>();
                    if (attr == null) continue;

                    if (_commands.ContainsKey(attr.Name))
                    {
                        Debug.LogWarning($"Duplicate command name: {attr.Name}");
                        continue;
                    }

                    _commands[attr.Name] = new DebugCommand
                    {
                        Name = attr.Name,
                        Usage = attr.Usage,
                        Method = method,
                        Target = null
                    };
                }
            }
        }
        
        Debug.Log($"Registered {_commands.Count} debug commands");
    }

    void ClearInput()
    {
        if(inputField != null) inputField.text = string.Empty;
    }

    public void OnSelected()
    {
        InputSystem.actions.FindActionMap("Player").Disable();
    }

    public void OnDeselected()
    {
        InputSystem.actions.FindActionMap("Player").Enable();
    }

    public void OnSubmit()
    {
        ProcessCommand(inputField.text);
    }

    /// <summary>
    /// Parses the input string and invokes the corresponding command.
    /// </summary>
    /// <param name="command">command found from DebugCommandAttribute</param>
    public void ProcessCommand(string command)
    {
        ClearInput();
        if (string.IsNullOrWhiteSpace(command)) return;
        
        var tokens = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var commandName = tokens[0];
        var argTokens = tokens.Skip(1).ToArray();

        if (commandName == "help")
        {
            DebugLog.PrintLog("[System] Available commands:", LogSeverity.System);
            foreach (var cmd in _commands.Values)
            {
                DebugLog.PrintLog($"- {cmd.Name}. Usages:{cmd.Usage}", LogSeverity.System);
            }

            return;
        }

        if (!_commands.TryGetValue(commandName, out var command_))
        {
            Debug.LogError($"Unknown command: {commandName}");
            return;
        }
        
        InvokeCommand(command_, argTokens);
    }
    
    
    /// <summary>
    /// actually invokes the command with the arguments from the parser
    /// </summary>
    /// <param name="command"></param>
    /// <param name="argTokens"></param>
    private void InvokeCommand(DebugCommand command, string[] argTokens)
    {
        var parameters = command.Method.GetParameters();

        if (parameters.Length != argTokens.Length)
        {
            DebugLog.PrintLog($"Invalid arguments for command {command.Name}. Expected: {command.Usage}", LogSeverity.Error);
            return;
        }

        var convertedArgs = new object[parameters.Length];
        
        for(int i=0; i<parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;
            try
            {
                convertedArgs[i] = Convert.ChangeType(argTokens[i], paramType);
            }
            catch (Exception)
            {
                DebugLog.PrintLog(
                    $"Could not parse argument '{argTokens[i]}' as {paramType.Name} " +
                    $"for command '{command.Name}'. Usage: {command.Usage}", LogSeverity.Error);
                return;

            }
        }

        try
        {
            command.Method.Invoke(command.Target, convertedArgs);
        }
        catch (TargetInvocationException e)
        {
            DebugLog.PrintLog($"Command {command.Name} threw an exception: {e.InnerException}", LogSeverity.Error); 
        }
    }

    void HandleMessage(Message message)
    {
        GameObject textObject = new GameObject("Debug Text " + message.Timestamp);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = message.Content;
        text.fontSize = 12;
        text.fontStyle = FontStyles.Bold;

        switch (message.Severity)
        {
            case LogSeverity.Log:
                text.color = Color.white;
                break;
            case LogSeverity.Warning:
                text.color = Color.yellow;
                break;
            case LogSeverity.Error or LogSeverity.Assert or LogSeverity.Exception:
                text.color = Color.red;
                break;
            case LogSeverity.System:
                text.color = Color.cyan;
                break;
        }

        text.alignment = TextAlignmentOptions.Left;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.rectTransform.SetParent(scrollRect.content, false);
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
        if(scrollRect.verticalNormalizedPosition <= .3f)
            scrollRect.verticalNormalizedPosition = 0;
    }
    
    private void OnEnable()
    {
        DebugLog.Instance.OnLogMessage += HandleMessage;
    }

    private void OnDisable()
    {
        if( DebugLog.Instance != null)
            DebugLog.Instance.OnLogMessage -= HandleMessage;
    }
}
