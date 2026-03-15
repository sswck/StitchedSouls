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

    [Header("SFX Clips - Results")]
    public AudioClip victorySFX;
    public AudioClip defeatSFX;

    [Header("SFX Clips - Combat")]
    public AudioClip allyAttackSFX;
    public AudioClip allyHitSFX;
    public AudioClip enemyAttackSFX;
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
}
