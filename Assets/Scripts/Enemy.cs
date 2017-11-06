using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    // For normal movement
    public bool isTrapped = false;
    public float movement = 1;

    // For movement in the infestation zone
    public float angle = 0;
    public float radius = 0.3f;
    public Vector3 pivot;

    // For enemy info
    public Enemy.Type type;
    public string zone;
    public int hitsLeft = 1;

    // For health bar
    public GameObject healthBar;
    public SpriteRenderer healthBarSR;
    public Animator healthBarAnim;

    // For transitioning to the trapped state
    private bool hasBeenTrapped = false;
    private float deltaTimeTrapped = 0;
    private Vector3 targetPos, originalPos;
    private Vector3 targetRot, originalRot;

    protected const float PPU_SCALE = 512 / 100.0f;

    public enum Type
    {
        FLY, LADYBUG, BEE, BEETLE, FIREFLY, MAX
    }

    // Use this for initialization
    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (isTrapped)
        {
            Trapped();
        }
        else
        {
            Move();
        }
    }

    public void Trap()
    {
        isTrapped = true;

        originalPos = transform.position;
        angle = Mathf.Atan2(transform.position.y, transform.position.x);
        float x = radius * Mathf.Sin(angle) + pivot.x;
        float y = radius * Mathf.Cos(angle) + pivot.y;
        targetPos = new Vector3(x, y, transform.position.z);

        originalRot = transform.up;
        targetRot = pivot;
    }

    public virtual void Trapped()
    {
        if (!hasBeenTrapped)
        {
            transform.position = Vector3.Lerp(originalPos, targetPos, deltaTimeTrapped * 2);
            transform.up = Vector3.Lerp(originalRot, targetRot, deltaTimeTrapped * 2);
            deltaTimeTrapped += Time.deltaTime;
            if (transform.position == targetPos)
            {
                hasBeenTrapped = true;
            }
            return;
        }

        float x = radius * Mathf.Sin(angle) + pivot.x;
        float y = radius * Mathf.Cos(angle) + pivot.y;
        Vector3 position = new Vector3(x, y, transform.position.z);

        // update position and rotation around the pivot point
        transform.position = position;
        transform.up = pivot;

        if (healthBar != null)
        {
            healthBar.transform.rotation = Quaternion.identity;
        }

        angle += Time.deltaTime;
    }

    public virtual void Move()
    {
        Vector3 translation = movement * PPU_SCALE * new Vector3(0, -1, 0) * Time.deltaTime;
        transform.Translate(translation);
    }

    // Returns true if the enemy is killed on this hit
    public virtual bool Swat()
    {
        hitsLeft--;
        if (hitsLeft == 1)
        {
            healthBarAnim.speed = 1;
            StartCoroutine(DestroyOnAnimationEnd(healthBar, healthBarAnim.runtimeAnimatorController.animationClips[0].length));
        }
        return hitsLeft <= 0;
    }

    public void AddHealthBar(GameObject prefab)
    {
        healthBar = Instantiate(prefab);

        healthBar.transform.parent = transform;
        healthBar.transform.rotation = Quaternion.identity;
        healthBar.transform.localPosition = Vector3.zero;

        healthBarSR = healthBar.GetComponent<SpriteRenderer>();
        healthBarAnim = healthBar.GetComponent<Animator>();
        healthBarAnim.speed = 0;
    }

    private IEnumerator DestroyOnAnimationEnd(GameObject gameObject, float length)
    {
        yield return new WaitForSeconds(length);
        Object.Destroy(gameObject);
    }

}
