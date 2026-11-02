using System;
using UnityEngine;
using UnityEngine.UI;


public class InputSpriteComponent : MonoBehaviour
{
    
    
    private SpriteRenderer _spriteRenderer;
    private Image _image;


    private void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _image = GetComponentInChildren<Image>();
        
        
        
    }

    public void SetSprite(Sprite sprite)
    {
        if(_spriteRenderer != null) _spriteRenderer.sprite = sprite;
        if(_image != null) _image.sprite = sprite;
    }
}
