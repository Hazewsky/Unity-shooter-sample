using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Menu : MonoBehaviour {
    public GameObject mainMenuHolder;
    public GameObject optionsMenuHolder;

    public Slider[] volumeSliders;
    public Toggle[] resToggles;
    public Toggle fullScreenToggle;
    public int[] screenWidths;
    int activeScreenResIndex;

    public void Play()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        mainMenuHolder.SetActive(false);
        optionsMenuHolder.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenuHolder.SetActive(true);
        optionsMenuHolder.SetActive(false);

    }
    //i - toggle
    public void SetScreenResolution(int i)
    {
        if (resToggles[i].isOn)
        {
            activeScreenResIndex = i;
            PlayerPrefs.SetInt("Screen res index", activeScreenResIndex);
            float aspectRatio = 16 / 9f;
            Screen.SetResolution(screenWidths[i], (int)(screenWidths[i]/aspectRatio),false);
            PlayerPrefs.Save();
        }
    }

    public void SetFullScreen(bool isFullScreen)
    {
        for(int i=0; i< resToggles.Length; i++)
        {
            resToggles[i].interactable = !isFullScreen;
        }
        if (isFullScreen)
        {
            //mas native res
            Resolution[] allRes = Screen.resolutions;
            //last
            Resolution maxRes = allRes[allRes.Length - 1];
            Screen.SetResolution(maxRes.width, maxRes.height,true);
        }
        else
        {
            SetScreenResolution(activeScreenResIndex);
        }
        PlayerPrefs.SetInt("fullscreen", (isFullScreen)?1:0);
        PlayerPrefs.Save();
    }
    public void SetMasterVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Master);
    }
    public void SetMusicVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Music);
    }
    public void SetSfxVolume(float value)
    {
        AudioManager.instance.SetVolume(value, AudioManager.AudioChannel.Sfx);
    }
    // Use this for initialization
    void Start () {
        activeScreenResIndex = PlayerPrefs.GetInt("Screen res index");
        bool isFullScreen = (PlayerPrefs.GetInt("fullscreen") == 1);
       
        volumeSliders[0].value = AudioManager.instance.masterVolumePercent;
        volumeSliders[1].value = AudioManager.instance.musicVolumePercent;
        volumeSliders[2].value = AudioManager.instance.sfxVolumePercent;

        for(int i=0; i< resToggles.Length; i++)
        {
            resToggles[i].isOn = i == activeScreenResIndex;
        }
        fullScreenToggle.isOn = isFullScreen;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
