using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public CDebug.EDebugLevel debugLevel = CDebug.EDebugLevel.TRACE;

    public float distFromCamera = 10.0f;

    [Tooltip("Time before enemies begin to spawn (seconds)")]
    public float startDelay = 1.0f;
    [Tooltip("Radius of swat zone")]
    public float swatRadius = 1.0f;
    [Tooltip("Number of kills to next increase in success")]
    public int nextKillCount = 5;
    [Tooltip("Number of successes to next increase in difficulty")]
    public int nextSuccessCount = 12;
    [Tooltip("Max number of enemies before game over")]
    public int maxNumEnemies = 3;
    [Tooltip("Duration of stun (seconds)")]
    public float stunTime = 1.0f;
    [Tooltip("Starting unlocked enemies")]
    public Enemy.Type initialEnemyType = Enemy.Type.FLY;
    [Tooltip("Turn on moderated difficulty mode")]
    public bool moderateDifficultyMode = true;

    // controls the frequency of spawning
    [Tooltip("Change in frequency on success")]
    public float deltaSpawnRate = 1.0f;
    [Tooltip("Min frequency")]
    public float minSpawnRate = 0.1f;

    // controls the number of enemies that are spawned
    [Tooltip("Change in number of enemies on success")]
    public float deltaSpawnNum = 0.25f;
    [Tooltip("Max number of enemies")]
    public float maxSpawnNum = 1f;

    // Zone specific references
    public GameObject upZone;
    public GameObject downZone;
    public GameObject leftZone;
    public GameObject rightZone;

    public GameObject upFrog;
    public GameObject downFrog;
    public GameObject leftFrog;
    public GameObject rightFrog;

    // Player specific references
    public GameObject babyFrog;
    public AudioSource audioSource;
    public AudioClip sound;

    // Enemy prefabs
    public Transform enemyPrefab;
    public Transform flyEnemyPrefab;
    public Transform beeEnemyPrefab;
    public Transform ladybugEnemyPrefab;
    public Transform beetleEnemyPrefab;
    public Transform fireflyEnemyPrefab;
    public GameObject bubblePrefab;

    // Sprites
    public Sprite frogDisabledSprite;
    public Sprite frogEnabledSprite;

    // UI elements
    public GameObject controlsOverlay;
    public GameObject newEnemyWarning;
    public GameObject inGameUI;
    public GameObject pauseGameUI;
    public GameObject endGameUI;
    public GameObject helpUI;

    public const string ZONE_UP = "Up";
    public const string ZONE_DOWN = "Down";
    public const string ZONE_LEFT = "Left";
    public const string ZONE_RIGHT = "Right";

    // constants
    private static string[] ZONE_AXES = { "Vertical", "Horizontal" };
    private const float AXIS_RANGE = 0.75f;
    private const float SUCCESS_TIME = 2.5f;

    // references to frequently accessed objects or components
    private Dictionary<string, GameObject> frogInZone = new Dictionary<string, GameObject>();
    private Animator babyFrogAnimator;

    // Game status
    private bool isGameOver = false;
    private bool isGamePaused = false;
    private bool firstKill = true;
    private Dictionary<string, bool> isFrogEnabledInZone = new Dictionary<string, bool>();
    private int infestationCount = 0;
    private Dictionary<string, Queue<GameObject>> infestationZones = new Dictionary<string, Queue<GameObject>>();
    private Enemy.Type currHighestEnemyType;

    // Success tracking
    private float lastSuccessTimeDelta = 0;
    private int killCount = 0;
    private int successCount = 0;

    private int fixedModifierCount = 0;
    private int modifierCount = 0;

    // Spawn rates per enemy during the game
    private Dictionary<Enemy.Type, float> currSpawnRatePerEnemy;
    private Dictionary<Enemy.Type, float> currSpawnNumPerEnemy;
    private Dictionary<Enemy.Type, float> lastIncreasedPerEnemy;

    // Initial values
    private static Dictionary<Enemy.Type, float> initialSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>()
    {
        { Enemy.Type.FLY, 3.0f },
        { Enemy.Type.BEE, 7.0f },
        { Enemy.Type.LADYBUG, 5.0f },
        { Enemy.Type.BEETLE, 7.0f },
        { Enemy.Type.FIREFLY, 10.0f }
    };

    private static Dictionary<Enemy.Type, float> initialSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>()
    {
        { Enemy.Type.FLY, 1.0f },
        { Enemy.Type.BEE, 1.0f },
        { Enemy.Type.LADYBUG, 1.0f },
        { Enemy.Type.BEETLE, 1.0f },
        { Enemy.Type.FIREFLY, 1.0f }
    };

    private static Dictionary<Enemy.Type, System.Func<int, float>> spawnRateFormula = new Dictionary<Enemy.Type, System.Func<int, float>>()
    {
        { Enemy.Type.FLY, x => 3 / Mathf.Pow(Mathf.Pow(3 / 0.8f, 1 / 11.0f), x) },
        { Enemy.Type.BEE, x => 7 / Mathf.Pow(Mathf.Pow(7 / 2.0f, 1 / 11.0f), x) },
        { Enemy.Type.LADYBUG, x => 5 / Mathf.Pow(Mathf.Pow(5 / 0.8f, 1 / 11.0f), x) },
        { Enemy.Type.BEETLE, x => 7 / Mathf.Pow(Mathf.Pow(7 / 2.0f, 1 / 11.0f), x) },
        { Enemy.Type.FIREFLY, x => 10 / Mathf.Pow(Mathf.Pow(10 / 4.2f, 1 / 11.0f), x) }
    };

    private static Dictionary<Enemy.Type, System.Func<int, float, float>> modSpawnRateFormula = new Dictionary<Enemy.Type, System.Func<int, float, float>>()
    {
        { Enemy.Type.FLY, (x, modifier) => 3 / Mathf.Pow(Mathf.Pow(3 / (0.8f + modifier), 1 / 11.0f), x) },
        { Enemy.Type.BEE, (x, modifier) => 7 / Mathf.Pow(Mathf.Pow(7 / (2.0f + modifier), 1 / 11.0f), x) },
        { Enemy.Type.LADYBUG, (x, modifier) => 5 / Mathf.Pow(Mathf.Pow(5 / (0.8f + modifier), 1 / 11.0f), x) },
        { Enemy.Type.BEETLE, (x, modifier) => 7 / Mathf.Pow(Mathf.Pow(7 / (2.0f + modifier), 1 / 11.0f), x) },
        { Enemy.Type.FIREFLY, (x, modifier) => 10 / Mathf.Pow(Mathf.Pow(10 / (4.2f + modifier), 1 / 11.0f), x) }
    };

    private static Dictionary<Enemy.Type, System.Func<int, float>> modFormula = new Dictionary<Enemy.Type, System.Func<int, float>>()
    {
        { Enemy.Type.FLY, x => 0.0f },
        { Enemy.Type.BEE, x => 3 / (1 + Mathf.Exp(Mathf.Log(9) * (-x / 0.4f + 1.5f))) },
        { Enemy.Type.LADYBUG, x => 0.0f },
        { Enemy.Type.BEETLE, x => 3 / (1 + Mathf.Exp(Mathf.Log(9) * (-x / 0.4f + 1.5f))) },
        { Enemy.Type.FIREFLY, x => 3 / (1 + Mathf.Exp(Mathf.Log(9) * (-x / 0.4f + 1.5f))) }
    };

    // Use this for initialization
    void Start()
    {
        CDebug.SetDebugLoggingLevel((int)debugLevel);

        FirstTimeSetUp();
        ResetGame();

        StartCoroutine(DisableControlsOverlay());
    }

    void FirstTimeSetUp()
    {
        // Screen.SetResolution(Screen.height, Screen.height, Screen.fullScreen);

        /*
        float s_baseOrthographicSize = Screen.height / 32.0f / 2.0f;
        Camera.main.orthographicSize = s_baseOrthographicSize;
        */

        // Position the pause button at the top right corner of the game map
        RectTransform canvas = inGameUI.GetComponent<RectTransform>();
        RectTransform button = inGameUI.transform.Find("Button").GetComponent<RectTransform>();
        float point = Camera.main.ViewportToWorldPoint(new Vector3(0, 0.95f, distFromCamera)).y;
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(new Vector2(point, point));
        float x = (viewportPos.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f);
        float y = (viewportPos.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f);
        Vector2 rectTransformPos = new Vector2(x, y);

        button.anchoredPosition = rectTransformPos;

        babyFrogAnimator = babyFrog.GetComponent<Animator>();

        frogInZone.Add("Up", upFrog);
        frogInZone.Add("Down", downFrog);
        frogInZone.Add("Left", leftFrog);
        frogInZone.Add("Right", rightFrog);
    }

    void ResetGame()
    {
        float currTime = Time.time;

        // reset UI elements
        inGameUI.SetActive(true);
        pauseGameUI.SetActive(false);
        endGameUI.SetActive(false);

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
        isGamePaused = false;
        firstKill = true;

        isFrogEnabledInZone.Clear();
        isFrogEnabledInZone.Add("Up", true);
        isFrogEnabledInZone.Add("Down", true);
        isFrogEnabledInZone.Add("Left", true);
        isFrogEnabledInZone.Add("Right", true);

        currHighestEnemyType = initialEnemyType;

        UpdateInfestationCount(0);

        infestationZones.Clear();
        infestationZones.Add("Up", new Queue<GameObject>());
        infestationZones.Add("Down", new Queue<GameObject>());
        infestationZones.Add("Left", new Queue<GameObject>());
        infestationZones.Add("Right", new Queue<GameObject>());

        lastSuccessTimeDelta = 0;
        killCount = 0;
        successCount = 0;

        fixedModifierCount = 0;
        modifierCount = 0;

        currSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>(initialSpawnRatePerEnemy);
        currSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>(initialSpawnNumPerEnemy);
        lastIncreasedPerEnemy = new Dictionary<Enemy.Type, float>()
        {
            { Enemy.Type.FLY, currTime },
            { Enemy.Type.BEE, currTime },
            { Enemy.Type.LADYBUG, currTime },
            { Enemy.Type.BEETLE, currTime },
            { Enemy.Type.FIREFLY, currTime }
        };

        Time.timeScale = 1.0f;
    }

    IEnumerator DisableControlsOverlay()
    {
        while (firstKill)
        {
            yield return null;
        }
        controlsOverlay.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver || isGamePaused)
        {
            return;
        }

        float currTime = Time.time;
        float deltaTime = Time.deltaTime;

        if (!firstKill)
        {
            if (lastSuccessTimeDelta + deltaTime > SUCCESS_TIME)
            {
                lastSuccessTimeDelta = 0;
                IncreaseDifficulty();
            }
            else
            {
                lastSuccessTimeDelta += deltaTime;
            }
        }

        foreach (KeyValuePair<Enemy.Type, float> entry in currSpawnRatePerEnemy)
        {
            var type = entry.Key;
            var spawnRate = entry.Value;

            if (type > currHighestEnemyType || spawnRate <= 0)
            {
                continue;
            }

            if (currTime - (startDelay + lastIncreasedPerEnemy[type]) > spawnRate)
            {
                lastIncreasedPerEnemy[type] = currTime;
                for (int i = 0; i < currSpawnNumPerEnemy[type]; i++)
                {
                    SpawnEnemy(type);
                }
            }
        }
    }

    // Spawns an enemy tagged with its corresponding zone
    void SpawnEnemy(Enemy.Type type)
    {
        string zone = GenerateZone();
        Vector3 position = GeneratePosition();
        Quaternion rotation = GenerateRotation(position);
        CDebug.Log(CDebug.EDebugLevel.DEBUG, string.Format("zone={0} | spawn position={1} | rotation={2} | time={3}", zone, position, rotation.eulerAngles, Time.time));

        // Instantiate and position enemy prefab
        GameObject enemy;
        switch (type)
        {
            case Enemy.Type.FLY:
                enemy = Instantiate(flyEnemyPrefab).gameObject;
                break;
            case Enemy.Type.BEE:
                enemy = Instantiate(beeEnemyPrefab).gameObject;
                break;
            case Enemy.Type.LADYBUG:
                enemy = Instantiate(ladybugEnemyPrefab).gameObject;
                break;
            case Enemy.Type.BEETLE:
                enemy = Instantiate(beetleEnemyPrefab).gameObject;
                break;
            case Enemy.Type.FIREFLY:
                enemy = Instantiate(fireflyEnemyPrefab).gameObject;
                break;
            default:
                enemy = Instantiate(enemyPrefab).gameObject;
                break;
        }

        switch (zone)
        {
            case "Up":
                enemy.transform.parent = upZone.transform;
                break;
            case "Down":
                enemy.transform.parent = downZone.transform;
                break;
            case "Left":
                enemy.transform.parent = leftZone.transform;
                break;
            case "Right":
                enemy.transform.parent = rightZone.transform;
                break;
        }

        enemy.transform.localPosition = position;
        enemy.transform.localRotation = rotation;

        // populate with zone tag
        GameObject zoneInfo = new GameObject("Zone");
        zoneInfo.tag = "Zone" + zone;
        zoneInfo.transform.parent = enemy.transform;

        // populate with enemy-specific info
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        enemyScript.zone = zone;
        enemyScript.pivot = frogInZone[zone].transform.position;

        switch (type)
        {
            case Enemy.Type.FLY:
                ((FlyEnemy)enemyScript).m_centerPosition = position;
                break;
            case Enemy.Type.BEE:
                break;
            case Enemy.Type.LADYBUG:
                enemyScript.AddHealthBar(bubblePrefab);
                break;
            case Enemy.Type.BEETLE:
                enemyScript.AddHealthBar(bubblePrefab);
                Vector3 coords = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1 - swatRadius, distFromCamera));
                float radius = coords.magnitude;
                ((BeetleEnemy)enemyScript).distance = radius;
                break;
            default:
                break;
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
        }
        else
        {
            return sign == 1 ? ZONE_UP : ZONE_DOWN;
        }
    }

    // Returns a spawning position at the edge of the screen
    Vector3 GeneratePosition()
    {
        Vector3 position;

        position = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1.0f, distFromCamera));
        position = new Vector3(Random.Range(AXIS_RANGE * -position.y, AXIS_RANGE * position.y), position.y, position.z);

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
        if (isGameOver || isGamePaused)
        {
            return;
        }

        CDebug.Log(CDebug.EDebugLevel.INFO, string.Format("swat in {0} zone", zone));

        if (firstKill)
        {
            firstKill = false;
        }

        // prioritize enemies in the infestation zone
        if (infestationZones[zone].Count > 0)
        {
            GameObject enemy = infestationZones[zone].Peek();
            if (enemy.GetComponent<Enemy>().Swat())
            {
                infestationZones[zone].Dequeue();
                UpdateInfestationCount(infestationCount - 1);
                EatAndDestroyEnemy(zone, enemy);
                // EnemySwatted(enemy);
            }
            else
            {
                EatEnemy(zone, enemy);
            }
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
                //EnemySwatted(closest);
            }
            else
            {
                EatEnemy(zone, closest);
            }
        }
        else if (isSwatterActive)
        {
            // no enemy was found within the swat zone, disable the swatter temporarily
            StunFrog(zone);
            // reset the kill count on a missed hit
            killCount = 0;
            lastSuccessTimeDelta = 0;
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
        tongueLength.transform.localPosition = new Vector3(0, -targetSize / 2 + 1, 0);

        StartCoroutine(ResetFrog(zone));
    }

    private void StunFrog(string zone)
    {
        isFrogEnabledInZone[zone] = false;

        // show stars circling above frog
        GameObject frog = frogInZone[zone];
        Transform dizzy = frog.transform.Find("dizzy");
        dizzy.gameObject.SetActive(true);

        // switch to sprite with disabled state
        SpriteRenderer sr = frogInZone[zone].GetComponent<SpriteRenderer>();
        sr.sprite = frogDisabledSprite;

        if (infestationCount == 2)
        {
            modifierCount++;
        }

        StartCoroutine(UnstunFrog(zone));
    }

    IEnumerator UnstunFrog(string zone)
    {
        yield return new WaitForSeconds(stunTime);

        isFrogEnabledInZone[zone] = true;

        GameObject frog = frogInZone[zone];
        Transform dizzy = frog.transform.Find("dizzy");
        dizzy.gameObject.SetActive(false);

        SpriteRenderer sr = frogInZone[zone].GetComponent<SpriteRenderer>();
        sr.sprite = frogEnabledSprite;
    }

    IEnumerator ResetFrog(string zone)
    {
        yield return new WaitForSeconds(0.1f);
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

    public void IncreaseDifficulty()
    {
        if (currHighestEnemyType == Enemy.Type.MAX)
        {
            Dictionary<Enemy.Type, float> newSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>();
            Dictionary<Enemy.Type, float> newSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>();
            foreach (KeyValuePair<Enemy.Type, float> entry in currSpawnRatePerEnemy)
            {
                var type = entry.Key;

                newSpawnRatePerEnemy[type] = Mathf.Max(currSpawnRatePerEnemy[type] - deltaSpawnRate, minSpawnRate);
                newSpawnNumPerEnemy[type] = Mathf.Min(currSpawnNumPerEnemy[type] + deltaSpawnNum, maxSpawnNum);
                CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Type={0} | increased spawn rate={0} | spawn num={1}", type, newSpawnRatePerEnemy[type], newSpawnNumPerEnemy[type]));
            }
            currSpawnRatePerEnemy = newSpawnRatePerEnemy;
            currSpawnNumPerEnemy = newSpawnNumPerEnemy;
            return;
        }
        else
        {
            successCount++;
            if (successCount < nextSuccessCount)
            {
                if (moderateDifficultyMode)
                {
                    currSpawnRatePerEnemy[currHighestEnemyType] = modSpawnRateFormula[currHighestEnemyType](successCount, modFormula[currHighestEnemyType](fixedModifierCount));
                }
                else
                {
                    currSpawnRatePerEnemy[currHighestEnemyType] = spawnRateFormula[currHighestEnemyType](successCount);
                }
                
                CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Type={0} | increased success count={1} | spawn rate={2}", currHighestEnemyType, successCount, currSpawnRatePerEnemy[currHighestEnemyType]));
            }
            else if (successCount >= nextSuccessCount && currHighestEnemyType < Enemy.Type.MAX)
            {
                successCount = 0;
                currHighestEnemyType++;
                if (currHighestEnemyType < Enemy.Type.MAX)
                {
                    lastIncreasedPerEnemy[currHighestEnemyType] = Time.time;
                    
                    if (moderateDifficultyMode)
                    {
                        fixedModifierCount = modifierCount;
                        modifierCount = 0;
                        CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Modifier={0}", fixedModifierCount));
                    }

                    // display new enemy warning
                    newEnemyWarning.SetActive(true);
                    StartCoroutine(ClearWaveWarning());
                }

                CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Unlocked enemy={0}", currHighestEnemyType));
            }
        }
    }

    // Inform GameManager that an enemy has been successfully swatted
    public void EnemySwatted(GameObject enemy)
    {
        if (currHighestEnemyType == Enemy.Type.MAX)
        {
            killCount++;
            if (killCount == nextKillCount)
            {
                killCount = 0;
                successCount++;
            }
            if (successCount == nextSuccessCount)
            {
                successCount = 0;
                Dictionary<Enemy.Type, float> newSpawnRatePerEnemy = new Dictionary<Enemy.Type, float>();
                Dictionary<Enemy.Type, float> newSpawnNumPerEnemy = new Dictionary<Enemy.Type, float>();
                foreach (KeyValuePair<Enemy.Type, float> entry in currSpawnRatePerEnemy)
                {
                    newSpawnRatePerEnemy[entry.Key] = Mathf.Max(currSpawnRatePerEnemy[entry.Key] - deltaSpawnRate, minSpawnRate);
                    newSpawnNumPerEnemy[entry.Key] = Mathf.Min(currSpawnNumPerEnemy[entry.Key] + deltaSpawnNum, maxSpawnNum);
                    CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased spawn rate={0} | spawn num={1}", newSpawnRatePerEnemy[entry.Key], newSpawnNumPerEnemy[entry.Key]));
                }
                currSpawnRatePerEnemy = newSpawnRatePerEnemy;
                currSpawnNumPerEnemy = newSpawnNumPerEnemy;
            }
            return;
        }

        Enemy.Type type = enemy.GetComponent<Enemy>().type;
        if (type == currHighestEnemyType)
        {
            killCount++;
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased kill count={0}", killCount));
        }
        if (killCount == nextKillCount)
        {
            killCount = 0;
            successCount++;
            currSpawnRatePerEnemy[type] = spawnRateFormula[type](successCount);
            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Increased success count={0} | spawn rate={1}", successCount, currSpawnRatePerEnemy[type]));
        }
        if (successCount == nextSuccessCount && currHighestEnemyType < Enemy.Type.MAX)
        {
            successCount = 0;
            currHighestEnemyType++;
            if (currHighestEnemyType < Enemy.Type.MAX)
            {
                lastIncreasedPerEnemy[currHighestEnemyType] = Time.time;
                newEnemyWarning.SetActive(true);
                StartCoroutine(ClearWaveWarning());
            }

            CDebug.Log(CDebug.EDebugLevel.TRACE, string.Format("Unlocked enemy={0}", currHighestEnemyType));
        }
    }

    IEnumerator ClearWaveWarning()
    {
        yield return new WaitForSeconds(1.5f);
        newEnemyWarning.SetActive(false);
    }

    // Inform GameManager that an enemy has reached the infestation zone
    public void EnemyReached(GameObject obj)
    {
        Enemy control = (Enemy)obj.GetComponent("Enemy");
        control.Trap();

        string zone = control.zone;
        infestationZones[zone].Enqueue(obj);
        UpdateInfestationCount(infestationCount + 1);

        // reset the success scenario
        lastSuccessTimeDelta = 0;

        if (infestationCount >= maxNumEnemies)
        {
            CDebug.Log(CDebug.EDebugLevel.INFO, "game over");
            inGameUI.SetActive(false);
            endGameUI.SetActive(true);
            isGameOver = true;

            GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < gos.Length; i++)
            {
                Enemy script = (Enemy)gos[i].GetComponent("Enemy");
                script.movement = 0;
            }
        }
    }

    public void TogglePause()
    {
        if (isGameOver)
        {
            return;
        }

        if (isGamePaused && pauseGameUI.activeInHierarchy)
        {
            OnResumeButton();
        }
        else if (!isGamePaused && inGameUI.activeInHierarchy)
        {
            OnPauseButton();
        }
    }

    public void OnPauseButton()
    {
        Time.timeScale = 0;
        isGamePaused = true;

        pauseGameUI.SetActive(true);
        inGameUI.SetActive(false);
    }

    public void OnResumeButton()
    {
        Time.timeScale = 1.0f;
        isGamePaused = false;

        pauseGameUI.SetActive(false);
        inGameUI.SetActive(true);
    }

    public void OnPlayButton()
    {
        ResetGame();
    }

    public void OnHelpClick()
    {
        pauseGameUI.SetActive(false);
        helpUI.SetActive(true);
    }

    public void OnBackClick()
    {
        pauseGameUI.SetActive(true);
        helpUI.SetActive(false);
    }

    void UpdateInfestationCount(int count)
    {
        CDebug.Log(CDebug.EDebugLevel.DEBUG, "count=" + count);
        infestationCount = count;
        babyFrogAnimator.SetInteger("count", infestationCount);
    }

}
