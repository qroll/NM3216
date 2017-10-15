using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public float distFromCamera = 10.0f;
    public float startDelay = 1.0f;
    public float swatRadius = 0.3f;
    public float spawnFreqRate = 1.0f;
    public float deltaSpawnFreqRate = 0.1f;
    public float minSpawnFreqRate = 0.5f;
    public float spawnNumRate = 1.0f;
    public float deltaSpawnNumRate = 1.0f;
    public float maxSpawnNumRate = 10.0f;
    public CDebug.EDebugLevel debugLevel = CDebug.EDebugLevel.TRACE;

    public Transform enemy;
    public Sprite frogDisabledSprite;
    public Sprite frogEnabledSprite;
    public GameObject[] swatters;
    public GameObject[] frogs;
    public GameObject menuCanvas;
    public GameObject inGameCanvas;
    public GameObject endGameCanvas;

    public const string ZONE_UP = "Up";
    public const string ZONE_DOWN = "Down";
    public const string ZONE_LEFT = "Left";
    public const string ZONE_RIGHT = "Right";

    private static string[] ZONE_AXES = { "Vertical", "Horizontal" };
    private static float AXIS_MIN = 0.3f;
    private static float AXIS_MAX = 0.7f;
    private static float STUN_TIME = 2.0f;

    // Game status
    private bool isGameOver = false;
    private int killCount = 0;
    private float lastIncreased = 0;
    private Queue<GameObject> infestationZone = new Queue<GameObject>();
    private Dictionary<string, bool> swatterEnabled = new Dictionary<string, bool>();
    private Dictionary<string, GameObject> swatterSprites = new Dictionary<string, GameObject>();

    // Use this for initialization
    void Start () {
        CDebug.SetDebugLoggingLevel((int) debugLevel);
        
        // initialise swatters
        foreach (GameObject swatter in swatters)
        {
            swatter.SetActive(false);
        }

        swatterEnabled.Add("Up", true);
        swatterEnabled.Add("Down", true);
        swatterEnabled.Add("Left", true);
        swatterEnabled.Add("Right", true);

        swatterSprites.Add("Up", frogs[0]);
        swatterSprites.Add("Down", frogs[1]);
        swatterSprites.Add("Left", frogs[2]);
        swatterSprites.Add("Right", frogs[3]);

        menuCanvas.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        if (isGameOver)
        {
            return;
        }

        CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("time={0} | last update={1}", Time.time, (startDelay + lastIncreased)));
        if (Time.time - (startDelay + lastIncreased) > spawnFreqRate)
        {
            for (int i = 0; i < spawnNumRate; i++)
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
        CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("spawn position={0} | rotation={1} | time={2}", position, rotation, Time.time));
        GameObject spawn = Instantiate(enemy, position, rotation).gameObject;
        GameObject go = new GameObject("Zone");
        go.tag = "Zone" + zone;
        go.transform.parent = spawn.transform;

        Movement control = (Movement) spawn.GetComponent("Movement");
        control.zone = zone;
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
        Vector3 direction = new Vector3(0,0,0) - position;
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
            CDebug.Log(CDebug.EDebugLevel.TRACE, "destroying bug in infestation zone");
            GameObject enemy = infestationZone.Dequeue();
            AnimateFrog(zone, enemy, true);
            Object.Destroy(enemy);
            return;
        }

        GameObject closest = FindClosestEnemy(zone);
        bool isSwatterActive = swatterEnabled[zone];

        if (isSwatterActive && closest != null)
        {
            // if swatter is enabled and at least one enemy is within the swat zone,
            // destroy the closest enemy
            AnimateFrog(zone, closest, false);
            Object.Destroy(closest.transform.parent.gameObject);
        } else if (isSwatterActive)
        {
            // no enemy was found within the swat zone, disable the swatter temporarily
            swatterEnabled[zone] = false;
            SpriteRenderer sr = swatterSprites[zone].GetComponent<SpriteRenderer>();
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

        return closest;
    }

    IEnumerator DisableSwatter(float delay, GameObject obj)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    IEnumerator EnableSwatter(float delay, string zone)
    {
        yield return new WaitForSeconds(delay);
        swatterEnabled[zone] = true;
        SpriteRenderer sr = swatterSprites[zone].GetComponent<SpriteRenderer>();
        sr.sprite = frogEnabledSprite;
    }

    // Animates the frog swatting
    // The tongue extends towards the specified enemy and retracts afterwards
    // If turn bool is true, the frog will turn around first
    private void AnimateFrog(string zone, GameObject obj, bool turn)
    {
        GameObject frog = swatterSprites[zone];
        Transform tongue = frog.transform.Find("tongue");

        Vector3 originalScale = tongue.localScale;
        Vector3 originalPosition = tongue.position;
        Quaternion originalRotation = tongue.rotation;
        
        ExtendTongue(frog, obj, turn);
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
        Vector3 localScale = tongue.localScale;
        
        float targetSize = direction.magnitude;
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
        tongue.localScale = originalScale;
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
        spawnFreqRate = Mathf.Max(spawnFreqRate - deltaSpawnFreqRate, minSpawnFreqRate);
        spawnNumRate = Mathf.Min(spawnNumRate + deltaSpawnNumRate, maxSpawnNumRate);
        CDebug.Log(CDebug.EDebugLevel.INFO, "kill count=" + killCount + " | spawn freq=" + spawnFreqRate + " | spawn num=" + spawnNumRate);
    }

    // Inform GameManager that an enemy has reached the infestation zone
    public void EnemyReached(GameObject obj)
    {
        Movement control = (Movement) obj.GetComponent("Movement");
        control.isTrapped = true;
        infestationZone.Enqueue(obj);

        if (infestationZone.Count >= 3)
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "game over");
            isGameOver = true;
            endGameCanvas.SetActive(true);
        }
    }

    public void OnPauseButton()
    {
        Time.timeScale = 0;

        menuCanvas.SetActive(true);
    }

    public void OnResumeButton()
    {
        Time.timeScale = 1.0f;

        menuCanvas.SetActive(false);
    }

    public void OnPlayButton()
    {
        menuCanvas.SetActive(false);
        endGameCanvas.SetActive(false);
        
        // enable all swatters
        swatterEnabled.Clear();
        swatterEnabled.Add("Up", true);
        swatterEnabled.Add("Down", true);
        swatterEnabled.Add("Left", true);
        swatterEnabled.Add("Right", true);

        // reset frog sprites to normal state
        foreach (GameObject sprite in swatterSprites.Values)
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
        infestationZone.Clear();
    }

}
