using UnityEngine;

public class EnemyFOV : EnemyAi
{
    CircleCollider2D cd;
    EnemyAi ea;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.position = transform.parent.position;
        ea = transform.parent.GetComponent<EnemyAi>();

        cd = gameObject.GetComponent<CircleCollider2D>();
        cd.isTrigger = true;
        cd.radius = 4f;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ea.isIn = true;
            ea.target = other.gameObject;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ea.isIn = false;
        }
    }
}
