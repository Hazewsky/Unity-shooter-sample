 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Image fadePlane;
    //ref to holder
    public GameObject gameOverUI;
    public RectTransform newWaveBanner;
    public Text newWaveTitle, newWaveEnemyCount;
    public Text scoreUI;
    public Text gameoverScoreUI;
    public RectTransform healthBar;
    Player player;
    Spawner spawner;
	// Use this for initialization
	void Start () {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
       
	}
    private void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }
    void OnNewWave(int waveNum)
    {
        string[] numbers = { "One", "Two", "Three", "Four", "Five" };
        newWaveTitle.text = "- Wave " + numbers[waveNum - 1] + " -";
        string enemyCountString = (spawner.waves[waveNum - 1].isInfinite) ? "Infinite" : spawner.waves[waveNum - 1].enemyCount.ToString();
        newWaveEnemyCount.text = "Enemies: " + enemyCountString;
        StopCoroutine("AnimateNewWaveBanner");
        StartCoroutine("AnimateNewWaveBanner");
    }

    IEnumerator AnimateNewWaveBanner()
    {
        float speed = 2.5f;
        //time for player to read
        float delayTime = 1.5f;
        float animatePercent = 0;
        //1 - gonna up, -1 down
        int dir = 1;
        //time + time for banner to reach + time for player to read + 
        float endDelayTime = Time.time + 1/speed + delayTime;
        //reach 1, wait, reach 0
        while(animatePercent >= 0)
        {
            animatePercent += Time.deltaTime * speed * dir;
            if(animatePercent >= 1)
            {
                animatePercent = 1;
                //finish waiting
                if(Time.time > endDelayTime)
                {
                    dir = -1;
                }
            }
            newWaveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-520, -150, animatePercent);
            //wait a frame
            yield return null;
        }

    }

    void OnGameOver()
    {
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
        StartCoroutine(Fade(Color.clear, new Color(0,0,0,.95f), 1));
        gameoverScoreUI.text = scoreUI.text;
       // scoreUI.gameObject.SetActive(false);
        //healthBar.transform.parent.gameObject.SetActive(false);
        gameOverUI.SetActive(true);
    }
	// Update is called once per frame
	void Update () {
        //format - 6 decimals
       
        scoreUI.text = ScoreKeeper.score.ToString();
        float healthPercent = 0;
        if (player != null)
        {
            healthPercent = player.health / player.startingHealth;
            
        }
        healthBar.localScale = new Vector3(healthPercent, 1, 1);
    }

    IEnumerator Fade(Color from,Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;
        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }
    }
    public void StartNewGame()
    {
        SceneManager.LoadScene("SampleScene");
    }
   
    public void ToMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}
