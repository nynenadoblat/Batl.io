using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private static readonly RaycastHit[] s_HitBuffer = new RaycastHit[16];

    [SerializeField] LayerMask collisionMask;
    [SerializeField] float speed = 15f;

    [Space]

    [SerializeField] Color friendly;
    [SerializeField] Color enemy;

    MeshRenderer mesh;
    ParticleSystem[] particles;
    TrailRenderer trail;
    Vector3 baseScale;

    public ulong OwnerId { get; set; }

    [field: SerializeField]
    public Team Team { get; set; }

    Vector3 prevPoint;
    Vector3 moveDir;
    [HideInInspector]
    public float lofetime;
    int dmg;
    ProjectileData projectileData;

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        particles = GetComponentsInChildren<ParticleSystem>();
        trail = GetComponentInChildren<TrailRenderer>();
        baseScale = transform.localScale;
    }

    public void Init(ProjectileData data)
    {
        if (mesh == null)
        {
            mesh = GetComponent<MeshRenderer>();
        }

        if (particles == null || particles.Length == 0)
        {
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        if (trail == null)
        {
            trail = GetComponentInChildren<TrailRenderer>();
        }

        if (trail != null)
        {
            trail.Clear();
        }

        prevPoint = transform.position;

        projectileData = data;

        dmg = data.baseDamege;
        moveDir = data.direction;
        OwnerId = data.ownerID;
        Team = data.team;
        speed = data.speed;
        lofetime = 0f;

        transform.localScale = baseScale * data.scale;

        if (moveDir != Vector3.zero)
        {
            transform.forward = moveDir;
        }

        if (data.mat != null && mesh != null)
        {
            mesh.material = data.mat;
        }

        Color targetColor = (GameManager.Instance != null && GameManager.Instance.MineTeam == Team) ? friendly : enemy;
        if (particles != null)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                main.startColor = targetColor;
                particles[i].Clear();
                particles[i].Play();
            }
        }
    }

    private void Update()
    {
        Vector3 curPos = transform.position + (speed * Time.deltaTime * moveDir);
        transform.position = curPos;

        lofetime += Time.deltaTime;

        if (lofetime > 1.5f)
        {
            ProjectilePool.Set(this);
            return;
        }

        CheckCollision();
    }

    void CheckCollision()
    {
        Vector3 curPos = transform.position;
        Vector3 displacement = curPos - prevPoint;
        float distance = displacement.magnitude;

        if (distance > 0.0001f)
        {
            Vector3 direction = displacement / distance;
            int hitCount = Physics.RaycastNonAlloc(prevPoint, direction, s_HitBuffer, distance * 1.1f, collisionMask);

            HealthComponent targetHealth = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                var hit = s_HitBuffer[i];
                if (hit.collider == null) continue;

                var health = hit.collider.transform.root.GetComponent<HealthComponent>();
                if (health != null && health.OwnerID != OwnerId && health.Team != Team)
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        targetHealth = health;
                    }
                }
            }

            if (targetHealth != null)
            {
                if (projectileData == null || !projectileData.isCosmetic)
                {
                    CalculateDamage();
                    targetHealth.Damage(dmg, OwnerId);
                }

                ProjectilePool.Set(this);
                return;
            }
        }

        prevPoint = curPos;
    }

    void CalculateDamage()
    {
        int critChance = projectileData?.critChance ?? 5;
        if (Random.Range(0, 100) < critChance)
        {
            dmg *= 2;
        }
    }
}

public class ProjectileData
{
    public Vector3 direction;
    public int baseDamege = 1;
    public int critChance = 5;
    public int speed;
    public ulong ownerID;
    public Team team;
    public Material mat;
    public bool isCosmetic;
    public float scale = 1f;
}
