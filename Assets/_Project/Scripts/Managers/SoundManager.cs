using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmPlayer;
    public AudioSource sfxPlayer;

    [Header("BGM Clips")]
    public AudioClip titleBGM;
    public AudioClip mapBGM;
    public AudioClip shopBGM;
    public AudioClip normalBattleBGM;
    public AudioClip eliteBattleBGM;
    public AudioClip bossBattleBGM;

    [Header("SFX Clips - Player")]
    public AudioClip moveSFX;   // 추후 유닛별 이동소리가 추가되면 unit.cs로 이동

    [Header("SFX Clips - UI & System")]
    public AudioClip uiClickSFX;
    public AudioClip sceneChangeSFX;
    public AudioClip deckCheckSFX;
    public AudioClip shopBuySFX;

    [Header("SFX Clips - Card & Items")]
    public AudioClip cardAttachSFX;
    public AudioClip defenseCardSFX;
    public AudioClip ppRecoverySFX;
    public AudioClip potionUseSFX;

    [Header("SFX Clips - Results")]
    public AudioClip victorySFX;
    public AudioClip defeatSFX;

    [Header("SFX Clips - Combat")]
    public AudioClip allyHitSFX;
    public AudioClip enemyHitSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmPlayer.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxPlayer.volume = volume;
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (bgmPlayer.clip == clip && bgmPlayer.isPlaying) return;

        bgmPlayer.clip = clip;
        bgmPlayer.loop = true;
        bgmPlayer.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxPlayer != null)
        {
            sfxPlayer.PlayOneShot(clip);
        }
    }

    // ==========================================
    // UI 버튼 이벤트(On Click) 연결용 도우미 함수들
    // ==========================================
    public void PlayUIClickSFX() { PlaySFX(uiClickSFX); }
    public void PlaySceneChangeSFX() { PlaySFX(sceneChangeSFX); }
    public void PlayDeckCheckSFX() { PlaySFX(deckCheckSFX); }
    public void PlayShopBuySFX() { PlaySFX(shopBuySFX); }
}
