using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffectPool : MonoBehaviour
{
    static MuzzleEffectPool Instance;

    [SerializeField]
    List<GameObject> poolEnemy = new();
    [SerializeField]
    List<GameObject> poolFriendly = new();

    [SerializeField]
    List<EffectHandler> effects = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        List<EffectHandler> toRemove = new();

        foreach (var item in effects)
        {
            if (!item.effect)
            {
                toRemove.Add(item);
                continue;
            }

            item.lifetime += Time.deltaTime;

            if(item.lifetime > 0.5f)
            {
                toRemove.Add(item);
                item.effect.SetActive(false);

                if (item.effect.CompareTag("Friendly"))
                {
                    Instance.poolFriendly.Add(item.effect);
                }
                else if (item.effect.CompareTag("Enemy"))
                {
                    Instance.poolEnemy.Add(item.effect);
                }
            }
        }

        foreach (var item in toRemove)
        {
            effects.Remove(item);
        }

        toRemove.Clear();
    }

    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        List<GameObject> pool = null;

        if (prefab.CompareTag("Friendly"))
        {
            pool = Instance.poolFriendly;
        }
        else if (prefab.CompareTag("Enemy"))
        {
            pool = Instance.poolEnemy;
        }
            
        if (pool.Count > 0 && pool[0])
        {
            var p = pool[0];
            p.transform.parent = parent;
            p.transform.localPosition = Vector3.zero;
            p.SetActive(true);

            foreach (var particle in p.GetComponentsInChildren<ParticleSystem>())
            {
                particle.Play();
            }

            Instance.effects.Add(new(p));

            pool.Remove(p);
            return p;
        }
        else 
        {
            if (pool.Count > 0 && !pool[0])
            {
                pool.RemoveAt(0);
            }

            var effect = Instantiate(prefab, parent);
            Instance.effects.Add(new(effect));

            return effect;
        }
       
        
    }

    public static void Set(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
        projectile.lofetime = 0;
        //Instance.pool.Add(projectile);
    }

    [System.Serializable]
    public class EffectHandler
    {
        public GameObject effect;
        public float lifetime;

        public EffectHandler(GameObject effect)
        {
            this.effect = effect;
            lifetime = 0;
        }
    }
}
