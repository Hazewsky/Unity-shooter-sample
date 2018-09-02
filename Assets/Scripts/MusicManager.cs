using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour {

    public AudioClip mainTheme;
    public AudioClip menuTheme;
    string sceneName;
	// Use this for initialization
	void Start () {
        OnLevelWasLoaded(0);
	}

    // Update is called once per frame
    private void OnLevelWasLoaded(int level)
    {
        string newSceneName = SceneManager.GetActiveScene().name;
        if(newSceneName != sceneName)
        {
            sceneName = newSceneName;
            //not to allow to call playMusic twice because of duplicate delete things
            Invoke("PlayMusic", .2f);
        }
    }
    void PlayMusic()
    {
        AudioClip clipToPlay = null;
        if(sceneName == "Menu")
        {
            clipToPlay = menuTheme;
        }else if(sceneName == "SampleScene")
        {
            clipToPlay = mainTheme;
        }
        if (clipToPlay != null)
        {
            AudioManager.instance.PlayMusic(clipToPlay, 2);
            //loop
            Invoke("PlayMusic", clipToPlay.length);
        }
    }
}
