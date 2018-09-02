using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
    public enum AudioChannel { Master, Sfx, Music }
    //other classes cam not set but can get
    public float masterVolumePercent  { get; private set; } 
    public float sfxVolumePercent { get; private set; }
    public float musicVolumePercent { get; private set; }
    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;
    public static AudioManager instance;
    //ref to listenre
    Transform audioListener;
    Transform playerT;

    SoundLibrary library;

    private void Awake()
    {
        if (instance != null)
        {
            //is duplicate
            Destroy(gameObject);
        }
        else
        {
            //singleton
            instance = this;
            DontDestroyOnLoad(gameObject);
            library = GetComponent<SoundLibrary>();
            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music source" + (i + 1));
                //returns
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }
            GameObject newSfx2DSource = new GameObject("2D Sfx source");
            //returns
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            sfx2DSource.transform.parent = transform;


            audioListener = FindObjectOfType<AudioListener>().transform;
            if(FindObjectOfType<Player>() !=null)
                playerT = FindObjectOfType<Player>().transform;
            //name, default if does not exist
            masterVolumePercent =PlayerPrefs.GetFloat("Master Volume",1);
            sfxVolumePercent = PlayerPrefs.GetFloat("Sfx Volume",1);
            musicVolumePercent = PlayerPrefs.GetFloat("Music Volume",1);
            print("set");
        }
    }

    public void SetVolume(float volumePercent, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercent = volumePercent;
                break;
            case AudioChannel.Sfx:
                sfxVolumePercent = volumePercent;
                break;
            case AudioChannel.Music:
                musicVolumePercent = volumePercent;
                break;
        }
        //upodate
        musicSources[0].volume = musicVolumePercent * masterVolumePercent;
        musicSources[1].volume = sfxVolumePercent * masterVolumePercent;
        //player prefs
        PlayerPrefs.SetFloat("Master Volume", masterVolumePercent);
        PlayerPrefs.SetFloat("Sfx Volume", sfxVolumePercent);
        PlayerPrefs.SetFloat("Music Volume", musicVolumePercent);
        PlayerPrefs.Save();
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        //0 1 0 1
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();
        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }
    public void PlaySound(AudioClip clip, Vector3 pos)
    {
        if(clip != null)
            AudioSource.PlayClipAtPoint(clip, pos, sfxVolumePercent * masterVolumePercent);
    }
    //alter
    public void PlaySound(string soundName, Vector3 pos)
    {
        //get sound from /library/
        PlaySound(library.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {
        //no at spec point
        sfx2DSource.Stop();
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercent * masterVolumePercent);

    }
    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * 1 / duration;
            //fade in
            musicSources[activeMusicSourceIndex].volume = 
                Mathf.Lerp(0, musicVolumePercent * masterVolumePercent, percent);
            //fade out
            musicSources[1- activeMusicSourceIndex].volume =
               Mathf.Lerp(musicVolumePercent * masterVolumePercent, 0,percent);
            yield return null;
        }
    }
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(playerT != null)
        {
            audioListener.position = playerT.position;
        }
	}
}
