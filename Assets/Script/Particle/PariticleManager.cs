using UnityEngine;

public class PariticleManager : MonoBehaviour
{
    [SerializeField] private ParticleSystem damageParticle;
    public static PariticleManager instance;

    private ParticleSystem damageParticleIns;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnDamagePariticle(Vector2 pos)
    {
        damageParticleIns = Instantiate(damageParticle, pos, Quaternion.identity);
    }
}
