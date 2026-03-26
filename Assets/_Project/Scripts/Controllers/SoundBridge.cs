using UnityEngine;

public class SoundBridge : MonoBehaviour
{
    public void PlayClickSFX() 
    { 
        if(SoundManager.Instance != null) SoundManager.Instance.PlayUIClickSFX(); 
    }

    public void PlaySceneChangeSFX() 
    { 
        if(SoundManager.Instance != null) SoundManager.Instance.PlaySceneChangeSFX(); 
    }

    public void PlayDeckCheckSFX() 
    { 
        if(SoundManager.Instance != null) SoundManager.Instance.PlayDeckCheckSFX();
    }

    public void PlayShopBuySFX() 
    { 
        if(SoundManager.Instance != null) SoundManager.Instance.PlayShopBuySFX();
    }
}
