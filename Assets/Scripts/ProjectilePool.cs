using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] Projectile prefab;

    static ProjectilePool Instance;

    [SerializeField]
    List<Projectile> pool = new();

    private void Awake()
    {
        Instance = this;
    }

    public static Projectile Get(Vector3 pos)
    {
        if (Instance.pool.Count > 0)
        {
            var p = Instance.pool[0];
            p.gameObject.SetActive(true);
            p.transform.position = pos;
            Instance.pool.Remove(p);
            return p;
        }
        else
        {
            var p = Instantiate(Instance.prefab, pos, Quaternion.identity);
            return p;
        }
    }

    public static void Set(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.lofetime = 0;
        Instance.pool.Add(projectile);
    }
}