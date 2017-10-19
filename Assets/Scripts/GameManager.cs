using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public CDebug.EDebugLevel debugLevel = CDebug.EDebugLevel.TRACE;

    public float distFromCamera = 10.0f;

    [Tooltip("Time before enemies begin to spawn (seconds)")]
    public float startDelay = 1.0f;
    [Tooltip("Radius of swat zone")]
    public float swatRadius = 1.0f;
    [Tooltip("Number of kills to next increase in success")]
    public int nextKillCount = 3;
    [Tooltip("Number of successes to next increase in difficulty")]
    public int nextSuccessCount = 3;
    [Tooltip("Max number of enemies in each zone before game over")]
    public int maxNumEnemies = 3;
    [Tooltip("Duration of stun (seconds)")]
    public float stunTime = 1.0f;

    // controls the frequency of spawning
    [Tooltip("Change in frequency on success")]
    public float deltaSpawnRate = 1.0f;
    [Tooltip("Min frequency before all enemies are unlocked")]
    public float initialMinSpawnRate = 1.0f;
    [Tooltip("Min frequency")]
    public float minSpawnRate = 0.1f;

    // controls the number of enemies that are spawned
    [Tooltip("Change in number of enemies on success")]
    public float deltaSpawnNum = 0.25f;
    [Tooltip("Max number of enemies")]
    public float maxSpawnNum = 1f;

    public Transform enemyPrefab;
    public GameObject[] frogs;
    public GameObject babyFrog;
    public AudioSource audioSource;
    public AudioClip sound;

    // Sprites
    public Sprite frogDisabledSprite;
    public Sprite frogEnabledSprite;
    public Sprite beeSprite;
    public Sprite ladybugSprite;

    // UI elements
    public GameObject inGameUI;
    public GameObject pauseGameUI;
    public GameObject endGameUI;

    public const string ZONE_UP = "Up";
    public const string ZONE_DOWN = "Down";
    public const string ZONE_LEFT = "Left";
    public const string ZONE_RIGHT = "Right";

    private static string[] ZONE_AXES = { "Vertical", "Horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;

    // Game status
    private bool isGameOver = false;
    private int killCount = 0;
    private int successCount = 0;
    private int infestationCount = 0;
    private Dictionary<string, bool> isFrogEnabledInZone = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> frogInZone = new Dictionary<string, GameObject>();

    private Dictionary<string, Queue<GameObject>> infestationZones = new Dictionary<string, Queue<GameObject>>();
    
    // Spawn rates per enemy during the game
    private Dictionary<Enemy.Type, float> currSpawnRatePerEnemy;
    private Dictionary<Enemy.Type, float> currSpawnNumPerEnemy;
    private Dictionary<Enemy.Type, float> lastIncreasedPerEnemy;
    private Enemy.Type highestEnemyType;

    private Animator anim;

    // Initial values
    private static Dictionary<Enemy.Type, float> initialSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>()
    {
        { Enemy.Type.BEE, 3.0f },
        { Enemy.Type.LADYBUG, 5.0f }
    };

    private static Dictionary<Enemy.Type, float> initialSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>()
    {
        { Enemy.Type.BEE, 1.0f },
        { Enemy.Type.LADYBUG, 1.0f }
    };

    private static Dictionary<Enemy.Type, float> initialLastIncreasedPerEnemy = new Dictionary<Enemy.Type, float>()
    {
        { Enemy.Type.BEE, 0f },
        { Enemy.Type.LADYBUG, 0f }
    };

    private static Enemy.Type initialEnemyType = Enemy.Type.BEE;

    // Use this for initialization
    void Start()
    {
        /*
        float s_baseOrthographicSize = Screen.height / 32.0f / 2.0f;
        Camera.main.orthographicSize = s_baseOrthographicSize;
        */

        anim = babyFrog.GetComponent<Animator>();

        CDebug.SetDebugLoggingLevel((int) debugLevel);
        
        frogInZone.Add("Up", frogs[0]);
        frogInZone.Add("Down", frogs[1]);
        frogInZone.Add("Left", frogs[2]);
        frogInZone.Add("Right", frogs[3]);

        ResetGame();
    }
	
	// Update is called once per frame
	void Update()
    {
        if (isGameOver)
        {
            return;
        }

        float currTime = Time.time;
        foreach (KeyValuePair<Enemy.Type, float> entry in currSpawnRatePerEnemy)
        {
            if (entry.Key > highestEnemyType)
            {
                continue;
            }

            if (currTime - (startDelay + lastIncreasedPerEnemy[entry.Key]) > entry.Value)
            {
                lastIncreasedPerEnemy[entry.Key] = currTime;
                for (int i = 0; i < currSpawnNumPerEnemy[entry.Key]; i++)
                {
                    SpawnEnemy(entry.Key);
                }
            }
        }
    }
    
    // Spawns an enemy tagged with its corresponding zone
    void SpawnEnemy(Enemy.Type type)
    {
        string zone = GenerateZone();
        Vector3 position = GeneratePosition(zone);
        Quaternion rotation = GenerateRotation(position);

        CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("spawn position={0} | rotation={1} | time={2}", position, rotation, Time.time));

        GameObject enemy = Instantiate(enemyPrefab, position, rotation).gameObject;

        // populate with zone tag
        GameObject zoneInfo = new GameObject("Zone");
        zoneInfo.tag = "Zone" + zone;
        zoneInfo.transform.parent = enemy.transform;

        // populate with enemy-specific info
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.zone = zone;
        enemyScript.type = type;

        if (type == Enemy.Type.BEE)
        {
            // default values
        } else if (type == Enemy.Type.LADYBUG)
        {
            enemyScript.movement = 1.5f;
            enemy.GetComponent<SpriteRenderer>().sprite = ladybugSprite;
        }
    }

    // Returns a random zone: Up, Down, Left or Right
    string GenerateZone()
    {
        string axis = ZONE_AXES[Mathf.FloorToInt(Random.Range(0, 2))];
        int sign = Mathf.FloorToInt(Random.Range(0, 2)) == 1 ? 1 : 0;

        if (axis == "Horizontal")
        {
            return sign == 1 ? ZONE_RIGHT : ZONE_LEFT;
        } else
        {
            return sign == 1 ? ZONE_UP : ZONE_DOWN;
        }
    }

    // Returns a spawning position at the edge of the screen within the specified zone
    Vector3 GeneratePosition(string zone) {
        Vector3 position;

        if (zone == ZONE_UP)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(0.5f, AXIS_MAX), 1, distFromCamera));
        } else if (zone == ZONE_DOWN)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(Random.Range(AXIS_MIN, 0.5f), 0, distFromCamera));
        } else if (zone == ZONE_LEFT)
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(0.25f, Random.Range(0.5f, AXIS_MAX), distFromCamera));
        } else
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(0.75f, Random.Range(AXIS_MIN, 0.5f), distFromCamera));
        }

        return position;
    }

    // Returns the rotation needed for a game object to face the origin
    Quaternion GenerateRotation(Vector3 position)
    {
        Vector3 direction = -position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        return rotation;
    }

    // Swats an enemy that is already in the infestation zone, or swats the
    // enemy closest to the player in the specified zone
    public void Swat(string zone)
    {
        CDebug.Log(CDebug.EDebugLevel.INFO, zone);
        
        // prioritize enemies in the infestation zone
        if (infestationZones[zone].Count > 0)
        {
            GameObject enemy = infestationZones[zone].Dequeue();
            UpdateInfestationCount(infestationCount - 1);
            EatAndDestroyEnemy(zone, enemy);
            return;
        }

        GameObject closest = FindClosestEnemy(zone);
        bool isSwatterActive = isFrogEnabledInZone[zone];

        if (isSwatterActive && closest != null)
        {
            // if swatter is enabled and at least one enemy is within the swat zone,
            // destroy the closest enemy
            if (closest.GetComponent<Enemy>().Swat())
            {
                EatAndDestroyEnemy(zone, closest);
                EnemySwatted(closest);
            } else
            {
                EatEnemy(zone, closest);
            }
        } else if (isSwatterActive)
        {
            // no enemy was found within the swat zone, disable the swatter temporarily
            isFrogEnabledInZone[zone] = false;
            SpriteRenderer sr = frogInZone[zone].GetComponent<SpriteRenderer>();
            sr.sprite = frogDisabledSprite;
            StartCoroutine(EnableSwatter(stunTime, zone));
        }
    }

    // Returns the closest enemy in the swat zone, or null if none were found
    private GameObject FindClosestEnemy(string zone)
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Zone" + zone);
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 position = new Vector3(0, 0, 0);

        Vector3 coords = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1 - swatRadius, distFromCamera));
        float radius = coords.sqrMagnitude;

        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.parent.position - position;
            float length = diff.sqrMagnitude;

            if (length < radius && length < minDistance)
            {
                closest = go;
                minDistance = length;
            }
        }

        return closest == null ? closest : closest.transform.parent.gameObject;
    }

    IEnumerator DisableSwatter(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    IEnumerator EnableSwatter(float delay, string zone)
    {
        yield return new WaitForSeconds(delay);
        isFrogEnabledInZone[zone] = true;
        SpriteRenderer sr = frogInZone[zone].GetComponent<SpriteRenderer>();
        sr.sprite = frogEnabledSprite;
    }

    void EatEnemy(string zone, GameObject enemy)
    {
        audioSource.PlayOneShot(sound);
        AnimateFrog(zone, enemy);
    }

    void EatAndDestroyEnemy(string zone, GameObject enemy)
    {
        audioSource.PlayOneShot(sound);
        AnimateFrog(zone, enemy);
        Object.Destroy(enemy);
    }

    private void AnimateFrog(string zone, GameObject enemy)
    {
        GameObject frog = frogInZone[zone];
        Transform tongue = frog.transform.Find("tongue");
        Transform tongueLength = tongue.transform.Find("tongue-length");
        
        Vector3 direction = frog.transform.position - enemy.transform.position;
        frog.transform.up = direction.normalized;

        float targetSize = direction.magnitude;
        float currentSize = tongueLength.GetComponent<Renderer>().bounds.size.y;
        float scale = targetSize / currentSize;
        
        tongueLength.localScale = new Vector3(1, scale, 1);
        tongueLength.transform.localPosition = new Vector3(0, -targetSize/2 + 1, 0);
        
        StartCoroutine(ResetFrog(zone));
    }
    
    IEnumerator ResetFrog(string zone)
    {
        yield return new WaitForSeconds(0.2f);
        GameObject frog = frogInZone[zone];
        Transform tongueLength = frog.transform.Find("tongue").Find("tongue-length");

        // reset the sprite
        tongueLength.localScale = new Vector3(1, 1, 1);
        tongueLength.localPosition = new Vector3(0, 0, 0);

        switch (zone)
        {
            case "Up":
                frog.transform.up = Vector3.down;
                break;
            case "Down":
                frog.transform.right = Vector3.left;
                break;
            case "Left":
                frog.transform.right = Vector3.down;
                break;
            case "Right":
                frog.transform.right = Vector3.up;
                break;
        }
    }

    // Inform GameManager that an enemy has been successfully swatted
    public void EnemySwatted(GameObject enemy)
    {
        if (highestEnemyType == Enemy.Type.MAX)
        {
            Dictionary<Enemy.Type, float> newSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>();
            Dictionary<Enemy.Type, float> newSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>();
            foreach (KeyValuePair<Enemy.Type, float> entry in currSpawnRatePerEnemy)
            {
                newSpawnRatePerEnemy[entry.Key] = Mathf.Max(currSpawnRatePerEnemy[entry.Key] - deltaSpawnRate, minSpawnRate);
                newSpawnNumPerEnemy[entry.Key] = Mathf.Min(currSpawnNumPerEnemy[entry.Key] + deltaSpawnNum, maxSpawnNum);
            }
            currSpawnRatePerEnemy = newSpawnRatePerEnemy;
            currSpawnNumPerEnemy = newSpawnNumPerEnemy;
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased spawn rate={0} | number={1}", currSpawnRatePerEnemy.Values.ToString(), currSpawnNumPerEnemy.Values.ToString()));
            return;
        }

        Enemy.Type type = enemy.GetComponent<Enemy>().type;
        if (type == highestEnemyType)
        {
            killCount++;
            currSpawnRatePerEnemy[type] = Mathf.Max(currSpawnRatePerEnemy[type] - deltaSpawnRate, initialMinSpawnRate);
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased kill count={0}", killCount));
        }
        if (killCount == nextKillCount)
        {
            killCount = 0;
            successCount++;
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased success count={0}", successCount));
        }
        if (successCount == nextSuccessCount && highestEnemyType < Enemy.Type.MAX)
        {
            successCount = 0;
            highestEnemyType++;
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Unlocked enemy={0}", highestEnemyType));
        }
    }
    
    // Inform GameManager that an enemy has reached the infestation zone
    public void EnemyReached(GameObject obj)
    {
        Enemy control = (Enemy) obj.GetComponent("Enemy");
        control.angle = Mathf.Atan2(obj.transform.position.y, obj.transform.position.x);
        control.isTrapped = true;

        string zone = control.zone;
        infestationZones[zone].Enqueue(obj);
        UpdateInfestationCount(infestationCount + 1);
        control.pivot = frogInZone[zone].transform.position;

        if (infestationCount >= maxNumEnemies)
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "game over");
            isGameOver = true;
            inGameUI.SetActive(false);
            endGameUI.SetActive(true);
        }
    }

    public void OnPauseButton()
    {
        Time.timeScale = 0;

        pauseGameUI.SetActive(true);
        inGameUI.SetActive(false);
    }

    public void OnResumeButton()
    {
        Time.timeScale = 1.0f;

        pauseGameUI.SetActive(false);
        inGameUI.SetActive(true);
    }

    public void OnPlayButton()
    {
        ResetGame();
    }

    void ResetGame()
    {
        inGameUI.SetActive(true);
        pauseGameUI.SetActive(false);
        endGameUI.SetActive(false);

        // enable all frogs
        isFrogEnabledInZone.Clear();
        isFrogEnabledInZone.Add("Up", true);
        isFrogEnabledInZone.Add("Down", true);
        isFrogEnabledInZone.Add("Left", true);
        isFrogEnabledInZone.Add("Right", true);

        // reset frog sprites to normal state
        foreach (GameObject sprite in frogInZone.Values)
        {
            SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
            sr.sprite = frogEnabledSprite;
        }

        // remove all remaining enemies
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Object.Destroy(obj);
        }

        // reset game state
        isGameOver = false;
        killCount = 0;
        successCount = 0;
        infestationCount = 0;

        infestationZones.Clear();
        infestationZones.Add("Up", new Queue<GameObject>());
        infestationZones.Add("Down", new Queue<GameObject>());
        infestationZones.Add("Left", new Queue<GameObject>());
        infestationZones.Add("Right", new Queue<GameObject>());

        currSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>(initialSpawnRatePerEnemy);
        currSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>(initialSpawnNumPerEnemy);
        lastIncreasedPerEnemy = new Dictionary<Enemy.Type, float>(initialLastIncreasedPerEnemy);
        highestEnemyType = initialEnemyType;

        UpdateInfestationCount(0);
    }

    void UpdateInfestationCount(int count)
    {
        CDebug.Log(CDebug.EDebugLevel.DEBUG, "count=" + count);
        infestationCount = count;
        anim.SetInteger("count", infestationCount);
    }

}
