using UnityEngine;
using UnityEngine.Pool;

public class BulletSys : MonoBehaviour
{
    
    [SerializeField] private GameObject bullet;

    public IObjectPool<GameObject> Pool { get; set; }

    private Rigidbody2D rb;
    private float speed = 40;

    public void Fire(Vector2 startPos,Vector2 direction)
    {
        transform.position = startPos;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * speed; 
    }

    public void Update()
    {
        if (Mathf.Abs(transform.position.x) > 20f || Mathf.Abs(transform.position.y) > 20f)
        {
            Pool.Release(this.gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            PariticleManager.instance.SpawnDamagePariticle(transform.position);
            Pool.Release(this.gameObject);
        }
    }

    

}
