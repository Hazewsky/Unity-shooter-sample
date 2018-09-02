using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour {

    public static float score { get; private set; }
    //CCCOMBO
    float lastEnemyKilledTIme;
    int streakCount;
    float streakExpireTime = 1;

	// Use this for initialization
	void Start () {
        //does not unsibscribe
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
        score = 0;
	}
	
    void OnEnemyKilled()
    {
        if(Time.time < lastEnemyKilledTIme + streakExpireTime)
        {
            //CCCOMBO
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }
        lastEnemyKilledTIme = Time.time;
        score += 5 + streakCount*streakCount;

    }
	// Update is called once per frame
	void Update () {
       // if (score > 0) score -= Time.deltaTime; 
	}

    void OnPlayerDeath()
    {
        //unsubscribe from event;
        ///static do not unsib on their own - can cause exploits, fire twice, 3 times, ets
        Enemy.OnDeathStatic -= OnEnemyKilled;
    }
}
