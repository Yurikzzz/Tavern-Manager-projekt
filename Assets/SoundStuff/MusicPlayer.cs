using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    [Header("Drop songs here")]
    public AudioClip[] songs;

    private AudioSource audioSource;
    private List<AudioClip> playlist = new List<AudioClip>();

    public static MusicPlayer instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            instance.ChangePlaylist(this.songs);
            Destroy(gameObject); 
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false;
    }

    void Start()
    {
        PlayNextSong();
    }

    void Update()
    {
        if (!audioSource.isPlaying && songs.Length > 0)
        {
            PlayNextSong();
        }
    }

    void PlayNextSong()
    {
        if (playlist.Count == 0)
        {
            playlist.AddRange(songs);
            ShufflePlaylist();
        }

        AudioClip nextSong = playlist[0];
        playlist.RemoveAt(0);

        audioSource.clip = nextSong;
        audioSource.Play();
    }

    void ShufflePlaylist()
    {
        for (int i = 0; i < playlist.Count; i++)
        {
            AudioClip temp = playlist[i];
            int randomIndex = Random.Range(i, playlist.Count);
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
        }
    }
    public void SetVolume(float newVolume)
    {
        audioSource.volume = newVolume;
    }

    public void ChangePlaylist(AudioClip[] newSongs)
    {
        if (this.songs == newSongs) return;

        songs = newSongs;   
        playlist.Clear(); 
        audioSource.Stop(); 
        PlayNextSong(); 
    }

}