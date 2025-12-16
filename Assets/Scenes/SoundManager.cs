using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        public bool loop = false;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Sound Library")]
    [SerializeField] private List<Sound> sounds = new List<Sound>();

    [Header("Settings")]
    [SerializeField] private float masterVolume = 1f;
    [SerializeField] private bool autoPlayBGM = true; // Avvia automaticamente la musica
    [SerializeField] private string bgmSoundName = "BGM"; // Nome del BGM da suonare

    // Singleton
    public static SoundManager Instance { get; private set; }

    void Awake()
    {
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste tra le scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Crea AudioSource per ogni suono
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.playOnAwake = false;
        }
    }

    void Start()
    {
        // Avvia automaticamente la musica di sottofondo se richiesto
        if (autoPlayBGM && !string.IsNullOrEmpty(bgmSoundName))
        {
            Play(bgmSoundName);
            Debug.Log($"BGM '{bgmSoundName}' avviato automaticamente");
        }
    }

    /// <summary>
    /// Suona un suono per nome
    /// </summary>
    public void Play(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        if (sound.source == null)
        {
            Debug.LogWarning($"AudioSource non trovato per '{soundName}'!");
            return;
        }

        sound.source.volume = sound.volume * masterVolume;
        sound.source.Play();
    }

    /// <summary>
    /// Suona un suono con volume personalizzato
    /// </summary>
    public void Play(string soundName, float customVolume)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        sound.source.volume = customVolume * masterVolume;
        sound.source.Play();
    }

    /// <summary>
    /// Suona un suono con pitch casuale (per varietà)
    /// </summary>
    public void PlayWithRandomPitch(string soundName, float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        sound.source.pitch = Random.Range(minPitch, maxPitch);
        sound.source.volume = sound.volume * masterVolume;
        sound.source.Play();
    }

    /// <summary>
    /// Ferma un suono
    /// </summary>
    public void Stop(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        sound.source.Stop();
    }

    /// <summary>
    /// Pausa un suono
    /// </summary>
    public void Pause(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        sound.source.Pause();
    }

    /// <summary>
    /// Riprendi un suono in pausa
    /// </summary>
    public void Resume(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        sound.source.UnPause();
    }

    /// <summary>
    /// Controlla se un suono sta suonando
    /// </summary>
    public bool IsPlaying(string soundName)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null || sound.source == null)
            return false;

        return sound.source.isPlaying;
    }

    /// <summary>
    /// Imposta il volume master (0-1)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Ferma tutti i suoni
    /// </summary>
    public void StopAll()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }
    }

    /// <summary>
    /// Suona un suono 3D in una posizione specifica
    /// </summary>
    public void PlayAtPosition(string soundName, Vector3 position)
    {
        Sound sound = sounds.Find(s => s.name == soundName);

        if (sound == null || sound.clip == null)
        {
            Debug.LogWarning($"Suono '{soundName}' non trovato!");
            return;
        }

        AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume * masterVolume);
    }
}