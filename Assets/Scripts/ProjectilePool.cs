using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private Projectile prefab;
    [SerializeField] private int defaultCapacity = 20;
    [SerializeField] private int maxSize = 100;

    public static ProjectilePool Instance { get; private set; }

    private ObjectPool<Projectile> pool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        pool = new ObjectPool<Projectile>(
            createFunc: CreateProjectile,
            actionOnGet: OnGetProjectile,
            actionOnRelease: OnReleaseProjectile,
            actionOnDestroy: OnDestroyProjectile,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: maxSize
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            pool?.Dispose();
            Instance = null;
        }
    }

    private Projectile CreateProjectile()
    {
        return Instantiate(prefab, transform);
    }

    private void OnGetProjectile(Projectile p)
    {
        p.gameObject.SetActive(true);
    }

    private void OnReleaseProjectile(Projectile p)
    {
        p.gameObject.SetActive(false);
    }

    private void OnDestroyProjectile(Projectile p)
    {
        if (p != null)
        {
            Destroy(p.gameObject);
        }
    }

    public static Projectile Get(Vector3 pos)
    {
        if (Instance == null || Instance.pool == null)
        {
            return null;
        }

        var p = Instance.pool.Get();
        p.transform.position = pos;
        return p;
    }

    public static void Set(Projectile projectile)
    {
        if (Instance == null || Instance.pool == null || projectile == null) return;

        if (projectile.gameObject.activeSelf)
        {
            projectile.lofetime = 0;
            Instance.pool.Release(projectile);
        }
    }
}
