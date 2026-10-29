using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;

public class DeviceState : Singleton<DeviceState>
{
    public InputDevice CurrentDevice;
    
    public Action OnDeviceChange;
    public bool IsGamepad => CurrentDevice is Gamepad;
    
    public void Start()
    {
        InputSystem.onActionChange += OnActionChange;
    }
    
    
    
    private void OnActionChange(object sender, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = (InputAction)sender;
            if (action != null)
            {
                if( action.activeControl != null)
                {
                    if(CurrentDevice == action.activeControl.device)
                        return;
                    CurrentDevice = action.activeControl.device;

                    HandleDeviceChange();
                }
            }
        }
    }

    private void HandleDeviceChange()
    {
        if (CurrentDevice != null)
        {
            OnDeviceChange?.Invoke();
            if (CurrentDevice is Keyboard keyboard)
            {
                DebugLog.PrintLog(keyboard.name);
                return;
            }

            if (CurrentDevice is Mouse mouse)
            {
                DebugLog.PrintLog(mouse.name);
                return;
            }

            if (CurrentDevice is Gamepad gamepad)
            {
                
                if (CurrentDevice is XInputController)
                {
                    DebugLog.PrintLog("XInputController");
                    return;
                }
                            
                if (CurrentDevice is DualShockGamepad)
                {
                    DebugLog.PrintLog("DualShockGamepad");
                    return;
                }

                if (CurrentDevice is SwitchProController)
                {
                    DebugLog.PrintLog("SwitchProController");
                    return;
                }
                            
                DebugLog.PrintLog("Unknown Gamepad");
                DebugLog.PrintLog(gamepad.name);
            }
                        
        }
    }
    
}
