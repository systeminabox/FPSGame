using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum DamageSource
{
    Unknown,
    Debug,
    EnemyAttack,
    PlayerAttack,
    Environmental,
    Regen,
    HealAbility,
    DamageOverTime,
}

public struct HealthChangeInfo
{
    public float Percent;
    public float Delta;
    public DamageSource Source;
    
    
    public HealthChangeInfo(float percent, float delta, DamageSource source)
    {
        Percent = percent;
        Delta = delta;
        Source = source;
    }
}


public class BasicBody : MonoBehaviour
{
    public float CurrentHealth { get; private set; }
    [SerializeField] protected float maxHealth;
    public bool useUIBar = false;
    private SpriteRenderer _bar;
    [SerializeField] protected HealthBarController healthBar;
    private Image _barImage;
    private GameObject _healthBarCanvas;
    private GameObject bar;
    public bool invulnerable = false;
    public AudioClip deathSound;
    
    
    public event Action<HealthChangeInfo> OnHealthChanged;

    protected virtual void Awake()
    {
        
        CurrentHealth = maxHealth;
//        Debug.Log($"Max health: {maxHealth} Current health: {CurrentHealth} {gameObject.name}", healthBar);
        healthBar = GetComponentInChildren<HealthBarController>();

    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        if (useUIBar && healthBar)
        {
            
            if (healthBar != null)
            {
                healthBar.basicBody = this;
                // Debug.Log($"Setting health bar to {GetHealthPercent()}" , healthBar);
                // Debug.Log($"Max health: {maxHealth} Current health: {CurrentHealth}", healthBar);
                healthBar.SetHealth(GetHealthPercent());
            }
                
            


        }
    }
    

    

    void UpdateHealthBar()
    {
        if (!useUIBar) return;
        if (_barImage == null) return;
        _barImage.fillAmount = GetHealthPercent();
        
    }
    
    

    public virtual void TakeDamage(float amount, DamageSource source = DamageSource.Unknown)
    {
        if(invulnerable) return;
        float previousPercent = GetHealthPercent();
        CurrentHealth -= amount;
        
        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            HandleDeath();
        }
        Debug.Log("Health: " + CurrentHealth);
        float newPercent = GetHealthPercent();
        OnHealthChanged?.Invoke(new HealthChangeInfo(newPercent,  newPercent - previousPercent, source));


    }

    public void Heal(float amount, DamageSource source = DamageSource.Unknown)
    {
        float previousPercent = GetHealthPercent();
        CurrentHealth += amount;
        if (CurrentHealth > maxHealth)
        {
            CurrentHealth = maxHealth;
        }
        float newPercent = GetHealthPercent();
        OnHealthChanged?.Invoke(new HealthChangeInfo(newPercent, newPercent - previousPercent, source));
    }
    
    public void SetHealth(float percent)
    {
        CurrentHealth = percent * maxHealth;
        OnHealthChanged?.Invoke(new HealthChangeInfo(percent, 0, DamageSource.Unknown));
    }

    protected virtual void HandleDeath()
    {
        
        if (deathSound)
        {
            //AudioManager.Instance.PlaySfx(deathSound);
        }
        
        Destroy(gameObject);
    }
    
    public float GetHealthPercent()
    {
        return CurrentHealth / maxHealth;
    }

    
}



