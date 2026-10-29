using UnityEngine;

[CreateAssetMenu(fileName = "InputIconSet", menuName = "Scriptable Objects/InputIconSet")]
public class InputIconSet : ScriptableObject
{
    
    public GamepadIcons xBox;
    public KeyboardIcons keyboard;
    
    [ System.Serializable]
        public struct GamepadIcons
        {
            public Sprite buttonSouth;
            public Sprite buttonNorth;
            public Sprite buttonEast;
            public Sprite buttonWest;
            public Sprite startButton;
            public Sprite selectButton;
            public Sprite leftTrigger;
            public Sprite rightTrigger;
            public Sprite leftShoulder;
            public Sprite rightShoulder;
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            public Sprite GetSprite(string controlPath)
            {
                // From the input system, we get the path of the control on device. So we can just
                // map from that to the sprites we have for gamepads.
                switch (controlPath)
                {
                    case "buttonSouth": return buttonSouth;
                    case "buttonNorth": return buttonNorth;
                    case "buttonEast": return buttonEast;
                    case "buttonWest": return buttonWest;
                    case "start": return startButton;
                    case "select": return selectButton;
                    case "leftTrigger": return leftTrigger;
                    case "leftTriggerButton": return leftTrigger;
                    case "rightTrigger": return rightTrigger;
                    case "rightTriggerButton": return rightTrigger;
                    case "leftShoulder": return leftShoulder;
                    case "rightShoulder": return rightShoulder;
                    case "dpad": return dpad;
                    case "dpad/up": return dpadUp;
                    case "dpad/down": return dpadDown;
                    case "dpad/left": return dpadLeft;
                    case "dpad/right": return dpadRight;
                    case "leftStick": return leftStick;
                    case "rightStick": return rightStick;
                    case "leftStickPress": return leftStickPress;
                    case "rightStickPress": return rightStickPress;
                }
                return null;
            }
        }
        [ System.Serializable]
        public struct KeyboardIcons
        {
            public Sprite KeyA;
            public Sprite KeyB;
            public Sprite KeyC;
            public Sprite KeyD;
            public Sprite KeyE;
            public Sprite KeyF;
            public Sprite KeyG;
            public Sprite KeyH;
            public Sprite KeyI;
            public Sprite KeyJ;
            public Sprite KeyK;
            public Sprite KeyL;
            public Sprite KeyM;
            public Sprite KeyN;
            public Sprite KeyO;
            public Sprite KeyP;
            public Sprite KeyQ;
            public Sprite KeyR;
            public Sprite KeyS;
            public Sprite KeyT;
            public Sprite KeyU;
            public Sprite KeyV;
            public Sprite KeyW;
            public Sprite KeyX;
            public Sprite KeyY;
            public Sprite KeyZ;
            public Sprite Escape;
            public Sprite Function1;
            public Sprite Function2;
            public Sprite Function3;
            public Sprite Function4;
            public Sprite Function5;
            public Sprite Function6;
            public Sprite Function7;
            public Sprite Function8;
            public Sprite Function9;
            public Sprite Function10;
            public Sprite Function11;
            public Sprite Function12;
            public Sprite Space;
            public Sprite Enter;
            public Sprite Backspace;
            public Sprite Tab;
            public Sprite Shift;
            public Sprite Control;
            public Sprite Alt;
            public Sprite Tilda;
            public Sprite One;
            public Sprite Two;
            public Sprite Three;
            public Sprite Four;
            public Sprite Five;
            public Sprite Six;
            public Sprite Seven;
            public Sprite Eight;
            public Sprite Nine;
            public Sprite Zero;
            public Sprite Minus;
            public Sprite Equals;
            public Sprite LeftBracket;
            public Sprite RightBracket;
            public Sprite Backslash;
            public Sprite Semicolon;
            public Sprite Quote;
            public Sprite Comma;
            public Sprite Period;
            public Sprite Slash;
            public Sprite UpArrow;
            public Sprite DownArrow;
            public Sprite LeftArrow;
            public Sprite RightArrow;
            public Sprite NumPad0;
            public Sprite NumPad1;
            public Sprite NumPad2;
            public Sprite NumPad3;
            public Sprite NumPad4;
            public Sprite NumPad5;
            public Sprite NumPad6;
            public Sprite NumPad7;
            public Sprite NumPad8;
            public Sprite NumPad9;
            public Sprite NumPadDecimal;
            public Sprite NumPadDivide;
            public Sprite NumPadMultiply;
            public Sprite NumPadSubtract;
            public Sprite NumPadAdd;
            public Sprite NumPadEnter;
            public Sprite M1;
            public Sprite M2;
            public Sprite M3;

            public Sprite GetSprite(string controlPath)
            {
                switch (controlPath)
                {
                    case "a": return KeyA;
                    case "b": return KeyB;
                    case "c": return KeyC;
                    case "d": return KeyD;
                    case "e": return KeyE;
                    case "f": return KeyF;
                    case "g": return KeyG;
                    case "h": return KeyH;
                    case "i": return KeyI;
                    case "j": return KeyJ;
                    case "k": return KeyK;
                    case "l": return KeyL;
                    case "m": return KeyM;
                    case "n": return KeyN;
                    case "o": return KeyO;
                    case "p": return KeyP;
                    case "q": return KeyQ;
                    case "r": return KeyR;
                    case "s": return KeyS;
                    case "t": return KeyT;
                    case "u": return KeyU;
                    case "v": return KeyV;
                    case "w": return KeyW;
                    case "x": return KeyX;
                    case "y": return KeyY;
                    case "z": return KeyZ;
                    case "escape": return Escape;
                    case "f1" : return Function1;
                    case "f2" : return Function2;
                    case "f3" : return Function3;
                    case "f4" : return Function4;
                    case "f5" : return Function5;
                    case "f6" : return Function6;
                    case "f7" : return Function7;
                    case "f8" : return Function8;
                    case "f9" : return Function9;
                    case "f10" : return Function10;
                    case "f11" : return Function11;
                    case "f12" : return Function12;
                    case "space" : return Space;
                    case "enter" : return Enter;
                    case "backspace" : return Backspace;
                    case "tab" : return Tab;
                    case "shift" : return Shift;
                    case "control" : return Control;
                    case "alt" : return Alt;
                    case "backquote" : return Tilda;
                    case "1" : return One;
                    case "2" : return Two;
                    case "3" : return Three;
                    case "4" : return Four;
                    case "5" : return Five;
                    case "6" : return Six;
                    case "7" : return Seven;
                    case "8" : return Eight;
                    case "9" : return Nine;
                    case "0" : return Zero;
                    case "minus" : return Minus;
                    case "equal" : return Equals;
                    case "leftBracket" : return LeftBracket;
                    case "rightBracket" : return RightBracket;
                    case "backslash" : return Backslash;
                    case "semicolon" : return Semicolon;
                    case "quote" : return Quote;
                    case "comma" : return Comma;
                    case "period" : return Period;
                    case "slash" : return Slash;
                    case "upArrow" : return UpArrow;
                    case "downArrow" : return DownArrow;    
                    case "leftArrow" : return LeftArrow;
                    case "rightArrow" : return RightArrow;
                    case "numpad0" : return NumPad0;
                    case "numpad1" : return NumPad1;
                    case "numpad2" : return NumPad2;
                    case "numpad3" : return NumPad3;
                    case "numpad4" : return NumPad4;
                    case "numpad5" : return NumPad5;
                    case "numpad6" : return NumPad6;
                    case "numpad7" : return NumPad7;
                    case "numpad8" : return NumPad8;
                    case "numpad9" : return NumPad9;
                    case "numpadPeriod" : return NumPadDecimal;
                    case "numpadDivide" : return NumPadDivide;
                    case "numpadMultiply" : return NumPadMultiply;
                    case "numpadSubtract" : return NumPadSubtract;
                    case "numpadAdd" : return NumPadAdd;
                    case "numpadEnter" : return NumPadEnter;
                    case "leftButton" : return M1;
                    case "rightButton" : return M2;
                    default: return null;
                }
            }
        }
}
