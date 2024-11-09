using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource[] audioSources;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSources = GetComponentsInChildren<AudioSource>();
    }

    public void SetSoundState(bool soundOn)
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.mute = !soundOn;
            }
        }
    }
}
