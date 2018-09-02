using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    Entity playerEntity;
    Transform playerT;

    int enemiesRemainingToSpawn;
    float nextSpawnTime;
    Wave currentWave;
    int currentWaveNumber;
    int enemiesRemainingAlive;
    MapGenerator map;
    //check if pplayer is standing at one place and start spawn on his head
    float timeBetweenCapmingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Color initTileColor;
    Vector3 lastPlayerPos;
    bool isCamping;
    bool isDisabled;

    public event System.Action<int> OnNewWave;

    [System.Serializable]
    public class Wave
    {
        public bool isInfinite;

        public int enemyCount;
        public float timeBetweenSpawns;

        public float moveSpeed;

        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;

    }
	// Use this for initialization
	void Start () {
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDeath;
        nextCampCheckTime = timeBetweenCapmingChecks + Time.time;
        lastPlayerPos = playerT.position;
        map = FindObjectOfType<MapGenerator>();
        NextWave();

		
	}
	
	// Update is called once per frame
	void Update () {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = Time.time + timeBetweenCapmingChecks;
                isCamping = (Vector3.Distance(playerT.position, lastPlayerPos) < campThresholdDistance);
                lastPlayerPos = playerT.position;
            }
            if ((enemiesRemainingToSpawn > 0 || currentWave.isInfinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
                //Quaternion.identity - No specific rotation
                StartCoroutine("SpawnEnemy");


            }
            if (!waves[currentWaveNumber-1].isInfinite && enemiesRemainingAlive <= 0)
            {
                NextWave();
            }
        }
        if (devMode)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>())
                {
                    GameObject.Destroy(enemy.gameObject);
                }
                NextWave();
            }
        }
	}

    IEnumerator SpawnEnemy()
    {
        //how long flash before spawn
        float spawnDelay = 1;
        //time of flash in sec
        float tileFlashSpeed = 4;
        Transform spawnTile = map.GetRandomOpenTile();
        if (isCamping)
        {
            
            spawnTile = map.GetTileFromPosition(playerT.position);
        }
        //flash
        //shared mat -all instances, mat - only current
        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initColor =  Color.white ;
        Color flashColor = Color.red;
        float spawnTimer = 0;
        while(spawnTimer < spawnDelay)
        {
            //FLASH
            //pingopong speed, duration % (0-1)
            tileMat.color = Color.Lerp(initColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
         Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        //delegate, subscribe to an event
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, 
            currentWave.hitsToKillPlayer, 
            currentWave.enemyHealth,
            currentWave.skinColor);
    }

    void OnPlayerDeath()
    {
        isDisabled = true;
    }
    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;

    }
    void NextWave()
    {
        print(currentWaveNumber);
       if(currentWaveNumber > 0)
        {
            print("Played");
            AudioManager.instance.PlaySound2D("Level Complete");
        }
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
            if(OnNewWave != null)
            {
                OnNewWave(currentWaveNumber);
            }
            ResetPlayerPos();
        }

    }

    void ResetPlayerPos()
    {
        //move to 0,0,0
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }
}
