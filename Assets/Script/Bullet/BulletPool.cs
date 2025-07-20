using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class BulletPool : MonoBehaviour
{
    public static BulletPool instance;

    public GameObject bulletPrefab;
    public int defaultCapacity = 10;
    public int maxPoolSize = 15;

    public IObjectPool<GameObject> Pool { get; private set; }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        Init();
    }

    private void Init()
    {
        Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, defaultCapacity, maxPoolSize);

        
        for (int i = 0; i < defaultCapacity; i++)
        {
            BulletSys bullet = CreatePooledItem().GetComponent<BulletSys>();
            bullet.Pool.Release(bullet.gameObject);
        }
    }

    private GameObject CreatePooledItem()
    {
        GameObject poolGo = Instantiate(bulletPrefab);
        poolGo.GetComponent<BulletSys>().Pool = this.Pool;
        return poolGo;
    }

    
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
}