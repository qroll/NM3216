using UnityEngine;

public class Enemy : MonoBehaviour {

    // For normal movement
    public bool isTrapped = false;
    public float movement = 1;
    
    // For movement in the infestation zone
    public float angle = 0;
    public float radius = 0.1f;
    public Vector3 pivot;

    // For enemy info
    public Enemy.Type type;
    public string zone;
    public int hitsLeft = 1;
    public GameObject plus;
    public Sprite plusSprite;

    public enum Type
    {
        FLY, BEE, LADYBUG, MAX
    }

    // Use this for initialization
    void Start()
    {

	}
	
	// Update is called once per frame
	void Update()
    {
        if (isTrapped)
        {
            Trapped();
        } else
        {
            Move();
        }
    }

    void Trapped()
    {
        float x = radius * Mathf.Sin(angle) + pivot.x;
        float y = radius * Mathf.Cos(angle) + pivot.y;
        Vector3 position = new Vector3(x, y, transform.position.z);

        // update position and rotation around the pivot point
        transform.position = position;
        transform.right = position;

        angle += Time.deltaTime;
    }

    public virtual void Move()
    {
        Vector3 translation = movement * (512 / 100) * new Vector3(0, -1, 0) * Time.deltaTime;
        transform.Translate(translation);
    }

    // Returns true if the enemy is killed on this hit
    public bool Swat()
    {
        hitsLeft--;
        if (hitsLeft == 1)
        {
            plus.SetActive(false);
        }
        return hitsLeft <= 0;
    }

    public void AddSprite(Sprite plusSprite)
    {
        // add the plus sprite
        plus = new GameObject("Plus");
        SpriteRenderer plusSr = plus.AddComponent<SpriteRenderer>();
        plusSr.sprite = plusSprite;
        plusSr.sortingLayerName = "Pickups";
        plus.transform.parent = transform;
        plus.transform.rotation = transform.rotation;
        plus.transform.localPosition = new Vector3(0.4f, -0.6f, 0);
    }

}
