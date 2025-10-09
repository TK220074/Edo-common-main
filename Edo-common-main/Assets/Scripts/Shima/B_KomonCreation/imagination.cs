using UnityEngine;
using UnityEngine.Video;

public class imagination : MonoBehaviour
{
    [SerializeField]private VideoPlayer videoPlayer;
    [SerializeField] private coverPatternBG coverPatternBG;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = Manager_CommonGroup.instance.audioM.GetAudioSource(AudioManager.WhichAudio.BGM);
    }

    /// <summary>
    /// Ä¶‚µ‚½‚¢“®‰æ‚ğƒZƒbƒg‚·‚é
    /// </summary>
    /// <param name="clip">Ä¶‚µ‚½‚¢“®‰æ</param>
    public void setVideo(VideoClip clip)
    {
        if(audioSource == null)
        {
            audioSource = Manager_CommonGroup.instance.audioM.GetAudioSource(AudioManager.WhichAudio.BGM);
        }

        coverPatternBG.setCoverPatternBG();

        videoPlayer.clip = clip;

        if(videoPlayer.GetTargetAudioSource(0) == null)
        {
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }
    }
}
