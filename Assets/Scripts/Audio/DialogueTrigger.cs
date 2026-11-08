
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] DialogueAsset dialogueAsset;
    
    private BoxCollider2D _collider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();
        _collider.isTrigger = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            AudioManager.Instance.PlayDialogue(dialogueAsset);
            Destroy(gameObject);
        }
    }
    
}
