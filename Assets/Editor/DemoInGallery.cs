using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public class DemoInGallery : MonoBehaviour
{
    private static string previousScene;
    private static double startClickTime = 0;
    private static GameObject galleryKicker;
    private static GameObject pathHommonculous;

    public static string lastPath = "";

    static DemoInGallery()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;

        

        
    }

    private static void OnPlayModeChanged(PlayModeStateChange _eStateChange)
    {
        switch (_eStateChange)
        {
            case PlayModeStateChange.EnteredEditMode:
                //If we are coming back from PlayMode which was started via PlayFromHere, we delete the helper game object
                OnExitPlay();
                
                break;
            case PlayModeStateChange.ExitingEditMode:
                break;
            case PlayModeStateChange.EnteredPlayMode:
                //If we entered PlayMode and a Helper Object exists, we should use its position to initialize the PlayFromHere-Objects
                if (GameManager.Instance.Player.GetComponent<PlayerController>())
                {
                    var pos = SessionState.GetVector3("PlayerStartPosition", new Vector3());
                    if(pos != Vector3.zero)
                        GameManager.Instance.Player.GetComponent<PlayerController>().WarpToPosition(pos,Vector3.forward );
                }
                break;
            case PlayModeStateChange.ExitingPlayMode:
                break;
            default:
                break;
        }
    }


    private static void OnExitPlay()
    {
        Debug.Log("on exit play");
        SessionState.EraseVector3("PlayerStartPosition");
        
    }
    
    

    private static void OnSceneGUI(SceneView sceneView)
    {
        //Check for right mouse button
        
        if(Event.current.isMouse && Event.current.button == 1)
        {
            if (!EditorApplication.isPlaying)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    //Save time of mouse down Event to later check for click duration
                    
                    startClickTime = EditorApplication.timeSinceStartup;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    //Check time since mouse down and open menu for short click
                    
                    if (EditorApplication.timeSinceStartup - startClickTime < .25f)
                    {
                        
                        Event e = Event.current;
                    
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Play From Here"), false,() => OnConfirmPlayFromHere(e.mousePosition));

                        menu.ShowAsContext();

                        Event.current.Use();
                    
                    }
                }
            }
            else
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    //Save time of mouse down Event to later check for click duration
                    startClickTime = EditorApplication.timeSinceStartup;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    //Check time since mouse down and open menu for short click
                    if (EditorApplication.timeSinceStartup - startClickTime < .25f)
                    {
                            Event e = Event.current;
                        
                            GenericMenu menu = new GenericMenu();
                            menu.AddItem(new GUIContent("Warp to here"), false, () => OnConfirmWarpToHere(e.mousePosition));
                            menu.ShowAsContext();
                            Event.current.Use();

                    }
                }
            }
        }
    }

    private static void OnConfirmWarpToHere(Vector2 mousePosition)
    {
        var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground", "Default"), QueryTriggerInteraction.Ignore))
            GameManager.Instance.Player.GetComponent<PlayerController>().WarpToPosition(hit.point + hit.normal, Camera.current.transform.forward);

    }

    private static void OnConfirmPlayFromHere(Vector2 mousePosition)
    {
        //Start PlayMode

        
        
        
        var ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        RaycastHit hit;

       
        
        if (Physics.Raycast(ray, out hit, float.MaxValue, LayerMask.GetMask("Ground", "Default"),
                QueryTriggerInteraction.Ignore))
        {
            
            SessionState.SetVector3("PlayerStartPosition", hit.point + hit.normal);
        }
            

        
        EditorApplication.isPlaying = true;
        
        
        
        
        

        Debug.Log("starting the game");
        //Unity reinitializes all scripts when starting PlayMode so we can't save any variables regularly
        //Create a GameObject with "DontSave"-Flag to "save" Position for start of PlayMode

    }
    
    

}
