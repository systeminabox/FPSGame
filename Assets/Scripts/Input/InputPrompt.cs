using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputPrompt : MonoBehaviour
{
    [SerializeField] private InputIconSet inputIconSet;
    [SerializeField] private InputActionReference inputActionReference;

    // Match these to the control scheme names in your Input Actions asset
    [SerializeField] private string keyboardScheme = "Keyboard";
    [SerializeField] private string gamepadScheme = "Gamepad";
    
    
    // Set by InputPromptEditor. Empty = use the first binding found for the scheme.
    [HideInInspector] [SerializeField] private string keyboardBindingId;
    [HideInInspector] [SerializeField] private string gamepadBindingId;

    [HideInInspector] public bool overrideText;
    [HideInInspector] public string labelText;

    [SerializeField]private SpriteRenderer _icon;
    [SerializeField]private TMPro.TextMeshProUGUI _text;
    

    private void Awake()
    {
        
        
    }

    private void OnEnable()
    {
        DeviceState.Instance.OnDeviceChange += Refresh;
        Refresh(); // show the right icon immediately, not only after the next device switch
    }

    private void OnDisable()
    {
        // Guard so we don't touch the singleton while it's shutting down
        if (DeviceState.Instance != null)
        {
            DeviceState.Instance.OnDeviceChange -= Refresh;
        }
    }

    private void Refresh()
    {
        InputAction action = inputActionReference.action;
        bool isGamepad = DeviceState.Instance.CurrentDevice is Gamepad;
 
        int index = FindBindingIndex(action, isGamepad);
        if (index < 0)
        {
            _icon.sprite = null;
            return;
        }
 
        action.GetBindingDisplayString(index, out _, out string controlPath);
 
        var sprite = isGamepad
            ? inputIconSet.xBox.GetSprite(controlPath)
            : inputIconSet.keyboard.GetSprite(controlPath);
 
        SetSprite(sprite);
        SetText(inputActionReference.action.name);
    }

    private void SetText(string text)
    {
        if (overrideText)
        {
            _text.text = labelText;
            return;
        }
        
        _text.text = text;
    }
    
    private int FindBindingIndex(InputAction action, bool isGamepad)
    {
        string id = isGamepad ? gamepadBindingId : keyboardBindingId;
 
        // Specific binding picked in the inspector (can be a composite part)
        if (!string.IsNullOrEmpty(id))
        {
            int byId = FindBindingIndexById(action, id);
            if (byId >= 0) return byId;
            // Stale id (binding was deleted): fall through to the scheme lookup
        }
 
        // Default: first binding assigned to the current control scheme
        return action.GetBindingIndex(group: isGamepad ? gamepadScheme : keyboardScheme);
    }
    
    private static int FindBindingIndexById(InputAction action, string id)
    {
        if (!System.Guid.TryParse(id, out System.Guid guid)) return -1;

        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].id == guid) return i;
        }
        return -1;
    }

    void SetSprite(Sprite sprite)
    {
        if (_icon)
        {
            _icon.sprite = sprite;
            
        }

        
    }
}