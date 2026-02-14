using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Source")]
    public AudioSource bgmPlayer;

    [Header("BGM Clips")]
    public AudioClip titleBGM;
    public AudioClip mapBGM;
    public AudioClip shopBGM;
    public AudioClip normalBattleBGM;
    public AudioClip eliteBattleBGM;

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

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 이미 같은 음악이 재생 중이면 다시 틀지 않음 (끊김 방지)
        if (bgmPlayer.clip == clip && bgmPlayer.isPlaying) return;

        bgmPlayer.clip = clip;
        bgmPlayer.loop = true;
        bgmPlayer.Play();
    }
}
