using System;

/// <summary>
/// 
///Use this attribute to mark a method as a debug command. <br/>
///<example>[DebugCommand("commandName", "commandUsage")]</example>
/// commandName is the name of the command and should be unique, one word, and use camel case (e.g. "myCommand")
///commandUsage is optional and is expected to follow the format: "commandName [arg1] [arg2] ..."
/// </summary>
[AttributeUsage(AttributeTargets.Method)]

public class DebugCommandAttribute : Attribute
{
    public string Name { get; }
    public string Usage { get; }
    public DebugCommandAttribute(string name, string usage)
    {
        Name = name;
        Usage = usage ?? name;
    }
    public DebugCommandAttribute(string name) : this(name, null) { }
}
