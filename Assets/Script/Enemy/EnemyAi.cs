using UnityEngine;

public class EnemyAi : MonoBehaviour
{
    Dominate closest;
    Rigidbody2D rb;
    float moveSpeed = 5f;
    public bool isIn;
    public GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        closest = FindClosestPlace();
        if (isIn)
        {
            if (closest != null)
            {
                if (gameObject.GetInstanceID() == closest.Conqueror_id && closest.isConquerorIn)
                    MoveTowards(target.transform.position);
                else if (gameObject.GetInstanceID() == closest.Conqueror_id && !closest.isConquerorIn)
                {
                    MoveTowards(closest.transform.position);
                    Debug.Log(!closest.isConquerorIn);
                }
                    
                else
                    MoveTowards(target.transform.position);
            }
            else
                MoveTowards(target.transform.position);
        }
        else
        {
            if(closest!=null)
                MoveTowards(closest.transform.position);
            else
                MoveTowards(target.transform.position);
        }
    }

    public Dominate FindClosestPlace()
    {
        Dominate closest = null;
        float closestSqrDistance = float.MaxValue;

        foreach (Dominate place in PlaceManager.Instance.allPlaces)
        {
            float sqrDistance = (transform.position - place.transform.position).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                if(!place.isConquer)
                    closest = place;
            }
        }

        return closest;
    }
    void MoveTowards(Vector2 targetPos)
    {
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        if (rb != null)
            rb.linearVelocity = direction * moveSpeed;
    }

}
