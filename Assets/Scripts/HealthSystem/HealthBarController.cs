using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    [FormerlySerializedAs("_basicBody")] public BasicBody basicBody;
    [FormerlySerializedAs("_healthBar")] [SerializeField] private Image healthBar;
    public bool isPlayer = false;
    public bool useWorldSpace = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (isPlayer)
        {
            basicBody = GameManager.Instance.Player.GetComponent<BasicBody>();
            SetHealth(basicBody.GetHealthPercent());
        }
        if(basicBody == null) return;
        basicBody.OnHealthChanged += (info) => healthBar.fillAmount = info.Percent;
    }

    // Update is called once per frame
    void Update()
    {
        if (!basicBody)
        {
            Destroy(gameObject);
            return;
        }
        if (!useWorldSpace) return;
        
        
        gameObject.transform.position = basicBody.gameObject.transform.position;
    }
    
    public void SetHealth(float percent)
    {
        healthBar.fillAmount = percent;
    }
}
