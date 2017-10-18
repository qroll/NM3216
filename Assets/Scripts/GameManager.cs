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

    // controls the frequency of spawning
    [Tooltip("Starting frequency at which enemies are spawned")]
    public float spawnFreqRate = 1.0f;
    [Tooltip("Change in frequency on success")]
    public float deltaSpawnFreqRate = 0.1f;
    [Tooltip("Minumum frequency")]
    public float minSpawnFreqRate = 0.5f;

    // controls the number of enemies that are spawned
    [Tooltip("Starting number of enemies spawned each time")]
    public float spawnNumRate = 1.0f;
    [Tooltip("Change in number of enemies on success")]
    public float deltaSpawnNumRate = 0.5f;
    [Tooltip("Maximum number of enemies")]
    public float maxSpawnNumRate = 3.0f;
    
    public Transform enemy;
    public GameObject[] frogs;
    public AudioSource audioSource;
    public AudioClip whack;

    // sprites
    public Sprite frogDisabledSprite;
    public Sprite frogEnabledSprite;

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
    private static float STUN_TIME = 1.0f;
    private static int MAX_NUM_ENEMIES = 5;

    // Game status
    private bool isGameOver = false;
    private int killCount = 0;
    private float lastIncreased = 0;
    private float currSpawnFreqRate = 1.0f;
    private float currSpawnNumRate = 1.0f;
    private Queue<GameObject> infestationZone = new Queue<GameObject>();
    private Dictionary<string, bool> isFrogEnabledInZone = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> frogInZone = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Start()
    {
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

        // CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("time={0} | last update={1}", Time.time, (startDelay + lastIncreased)));
        if (Time.time - (startDelay + lastIncreased) > currSpawnFreqRate)
        {
            for (int i = 0; i < currSpawnNumRate; i++)
            {
                SpawnEnemy();
            }
            lastIncreased = Time.time;
        }
    }

    // Spawns an enemy tagged with its corresponding zone
    void SpawnEnemy()
    {
        string zone = GenerateZone();
        Vector3 position = GeneratePosition(zone);
        Quaternion rotation = GenerateRotation(position);

        CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("spawn position={0} | rotation={1} | time={2}", position, rotation, Time.time));

        GameObject spawn = Instantiate(enemy, position, rotation).gameObject;
        GameObject go = new GameObject("Zone");
        go.tag = "Zone" + zone;
        go.transform.parent = spawn.transform;
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
            position = Camera.main.ViewportToWorldPoint(new Vector3(0, Random.Range(0.5f, AXIS_MAX), distFromCamera));
        } else
        {
            position = Camera.main.ViewportToWorldPoint(new Vector3(1, Random.Range(AXIS_MIN, 0.5f), distFromCamera));
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
        if (infestationZone.Count > 0)
        {
            GameObject enemy = infestationZone.Dequeue();
            AnimateFrogTongue(zone, enemy, true);
            Object.Destroy(enemy);
            return;
        }

        GameObject closest = FindClosestEnemy(zone);
        bool isSwatterActive = isFrogEnabledInZone[zone];

        if (isSwatterActive && closest != null)
        {
            // if swatter is enabled and at least one enemy is within the swat zone,
            // destroy the closest enemy
            AnimateFrogTongue(zone, closest, false);
            audioSource.PlayOneShot(whack);
            if (closest.GetComponent<Enemy>().Swat())
            {
                Object.Destroy(closest);
                EnemySwatted();
            }
        } else if (isSwatterActive)
        {
            // no enemy was found within the swat zone, disable the swatter temporarily
            isFrogEnabledInZone[zone] = false;
            SpriteRenderer sr = frogInZone[zone].GetComponent<SpriteRenderer>();
            sr.sprite = frogDisabledSprite;
            StartCoroutine(EnableSwatter(STUN_TIME, zone));
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

    // Animates the frog
    // The tongue extends towards the specified enemy and retracts afterwards
    // If turn bool is true, the frog will turn around first
    private void AnimateFrogTongue(string zone, GameObject enemy, bool turn)
    {
        GameObject frog = frogInZone[zone];
        Transform tongue = frog.transform.Find("tongue");

        Vector3 originalScale = tongue.localScale;
        Vector3 originalPosition = tongue.position;
        Quaternion originalRotation = tongue.rotation;
        
        ExtendTongue(frog, enemy, turn);
        StartCoroutine(RetractTongue(frog, originalScale, originalPosition, originalRotation, turn));
    }
    
    // Stretch the tongue to the current position of the specified enemy
    // TODO: improve tongue animation; it's the wrong length and isn't pointing in the right direction
    void ExtendTongue(GameObject frog, GameObject enemy, bool turn)
    {
        if (turn)
        {
            frog.transform.Rotate(new Vector3(0, 0, 180));
        }

        Transform tongue = frog.transform.Find("tongue");

        Vector3 direction = enemy.transform.position - tongue.position;
        float targetSize = direction.magnitude;
        Vector3 localScale = tongue.localScale;
        
        float currentSize = tongue.Find("tongue-length").GetComponent<Renderer>().bounds.size.y;
        float scale = localScale.y * (targetSize / currentSize);

        tongue.localScale = new Vector3(localScale.x, scale, localScale.z);
        
        tongue.LookAt(Vector3.forward, Vector3.Cross(Vector3.forward, direction));
        tongue.localRotation = tongue.localRotation * Quaternion.AngleAxis(-90, Vector3.forward);
    }

    IEnumerator RetractTongue(GameObject frog, Vector3 originalScale, Vector3 originalPosition, Quaternion originalRotation, bool turn)
    {
        yield return new WaitForSeconds(0.2f);
        Transform tongue = frog.transform.Find("tongue");

        // reset the sprite
        tongue.localScale = new Vector3(1, 0.1f, 1);
        tongue.rotation = originalRotation;
        if (turn)
        {
            frog.transform.Rotate(new Vector3(0, 0, 180));
        }
    }

    // Inform GameManager that an enemy has been successfully swatted
    // TODO: track specific enemy types and increase difficulty
    public void EnemySwatted()
    {
        killCount++;
        currSpawnFreqRate = Mathf.Max(currSpawnFreqRate - deltaSpawnFreqRate, minSpawnFreqRate);
        currSpawnNumRate = Mathf.Min(currSpawnNumRate + deltaSpawnNumRate, maxSpawnNumRate);
        CDebug.Log(CDebug.EDebugLevel.INFO, "kill count=" + killCount + " | spawn freq=" + spawnFreqRate + " | spawn num=" + spawnNumRate);
    }

    // Inform GameManager that an enemy has reached the infestation zone
    public void EnemyReached(GameObject obj)
    {
        Enemy control = (Enemy) obj.GetComponent("Enemy");
        control.angle = Mathf.Atan2(obj.transform.position.y, obj.transform.position.x);
        control.isTrapped = true;
        infestationZone.Enqueue(obj);

        if (infestationZone.Count >= MAX_NUM_ENEMIES)
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
        lastIncreased = 0;
        currSpawnFreqRate = spawnFreqRate;
        currSpawnNumRate = spawnNumRate;
        infestationZone.Clear();
    }

}
