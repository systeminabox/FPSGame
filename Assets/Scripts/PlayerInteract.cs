using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 10f;

    public GameObject camera;

    public GameObject prompt;
    InputPrompt inputPrompt;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputPrompt = prompt.GetComponent<InputPrompt>();
        prompt.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InteractInput)
        {
            if(!GameManager.Instance.playerCanMove) return;
            Interact();
        }
        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, interactRange);
        if (hit.collider != null)
        {
//            Debug.Log(hit.collider.gameObject);
            var obj = hit.collider.gameObject.GetComponentInParent<Interactable>();
            //Debug.Log(obj);
            if (obj != null)
            {
                inputPrompt.overrideText = true;
                inputPrompt.labelText = obj.IsActivated ? obj.deactivateText : obj.activateText;
                prompt.SetActive(true);
            }
            else
            {
                var vObj = hit.collider.gameObject.GetComponentInParent<VattalusInteractable>();
                if (vObj != null)
                {
                    inputPrompt.overrideText = true;
                    inputPrompt.labelText = vObj.isActivated ? vObj.deactivateText : vObj.activateText;
                    prompt.SetActive(true);
                }
                else
                {
                    prompt.SetActive(false);
                   // Debug.Log("No interactable found");
                }
            }
        }
    }
    
    void FixedUpdate()
    {
        
    }

    private void Interact()
    {
        Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit, interactRange);
        Debug.DrawRay(transform.position, transform.forward * interactRange, Color.red);
        Debug.Log("Attempting Interact " + hit.collider);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject);
            var obj = hit.collider.gameObject.GetComponentInParent<Interactable>();
            Debug.Log(obj);
            if (obj != null)
            {
                obj.PlayerInteract();
            }
            else
            {
                var vObj = hit.collider.gameObject.GetComponentInParent<VattalusInteractable>();
                if (vObj != null)
                {
                    vObj.Iteract();
                }
            }
        }
    }
}
