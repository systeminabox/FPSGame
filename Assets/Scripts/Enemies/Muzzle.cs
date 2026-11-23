using UnityEngine;
using UnityEngine.VFX;


public class Muzzle : MonoBehaviour
{
    
    public VisualEffect muzzleEffect;

    public void PlayMuzzleEffect()
    {
        muzzleEffect.Reinit();
    }
}
