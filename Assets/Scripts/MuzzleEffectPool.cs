using System.Collections.Generic;
using UnityEngine;

public class MuzzleEffectPool : MonoBehaviour
{
    public static MuzzleEffectPool Instance { get; private set; }

    [SerializeField]
    private List<GameObject> poolEnemy = new();
    [SerializeField]
    private List<GameObject> poolFriendly = new();

    [SerializeField]
    private List<EffectHandler> effects = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (effects.Count == 0) return;

        for (int i = effects.Count - 1; i >= 0; i--)
        {
            var item = effects[i];
            if (item == null || item.effect == null)
            {
                effects.RemoveAt(i);
                continue;
            }

            item.lifetime += Time.deltaTime;

            if (item.lifetime > 0.5f)
            {
                item.effect.SetActive(false);

                if (item.effect.CompareTag("Friendly"))
                {
                    poolFriendly.Add(item.effect);
                }
                else if (item.effect.CompareTag("Enemy"))
                {
                    poolEnemy.Add(item.effect);
                }

                effects.RemoveAt(i);
            }
        }
    }

    public static GameObject Spawn(GameObject prefab, Transform parent)
    {
        if (Instance == null || prefab == null) return null;

        List<GameObject> pool = null;

        if (prefab.CompareTag("Friendly"))
        {
            pool = Instance.poolFriendly;
        }
        else if (prefab.CompareTag("Enemy"))
        {
            pool = Instance.poolEnemy;
        }

        if (pool != null && pool.Count > 0)
        {
            while (pool.Count > 0)
            {
                var p = pool[0];
                pool.RemoveAt(0);

                if (p != null)
                {
                    p.transform.SetParent(parent, false);
                    p.transform.localPosition = Vector3.zero;
                    p.SetActive(true);

                    var particles = p.GetComponentsInChildren<ParticleSystem>();
                    for (int i = 0; i < particles.Length; i++)
                    {
                        particles[i].Play();
                    }

                    Instance.effects.Add(new EffectHandler(p));
                    return p;
                }
            }
        }

        var effect = Instantiate(prefab, parent);
        Instance.effects.Add(new EffectHandler(effect));
        return effect;
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
