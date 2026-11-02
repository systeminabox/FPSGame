using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InteractInput)
        {
            Interact();
        }
    }

    private void Interact()
    {
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, interactRange);
        Debug.DrawRay(transform.position, transform.forward * interactRange, Color.red);
        if (hit.collider != null)
        {
            Debug.Log(hit.collider.gameObject);
            var obj = hit.collider.gameObject.GetComponentInParent<Interactable>();
            Debug.Log(obj);
            if (obj != null)
            {
                obj.PlayerInteract();
            }
        }
    }
}
