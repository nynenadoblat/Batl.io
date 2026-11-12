using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Projectile : MonoBehaviourPun
{
    [SerializeField] LayerMask collisionMask;
    [SerializeField] float speed = 15f;

    [Space]

    [SerializeField] Color friendly;
    [SerializeField] Color enemy;

    MeshRenderer mesh;

    public int OwnerId { get; set; }

    [field: SerializeField]
    public Team Team { get; set; }

    Vector3 prevPoint, curPoint;
    Vector3 moveDir;
    [HideInInspector]
    public float lofetime;
    int dmg;
    ProjectileData projectileData;

    public void Init(ProjectileData data)
    {
        mesh = GetComponent<MeshRenderer>();

        prevPoint = transform.position;
        curPoint = transform.position;

        projectileData = data;

        dmg = data.baseDamege;
        moveDir = data.direction;
        OwnerId = data.ownerID;
        Team = data.team;
        speed = data.speed;

        if (data.mat)
        {
            mesh.material = data.mat;
        }

        foreach (var item in GetComponentsInChildren<ParticleSystem>())
        {
            var main = item.main;
            main.startColor = GameManager.Instance.MineTeam == Team ? friendly : enemy;
        }
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            transform.position += speed * Time.deltaTime * moveDir;

            lofetime += Time.deltaTime;

            if (lofetime > 1.5f)
            {
                ProjectilePool.Set(this);
            }

            CheckCollision();
        }

    }

    void CheckCollision()
    {
        Vector3 dir = curPoint - prevPoint;

        var hits = Physics.RaycastAll(prevPoint, dir, dir.magnitude * 1.5f, collisionMask);

        hits = hits.ToList().Where(h =>
        {
            var health = h.collider.transform.root.GetComponent<HealthComponent>();

            if (health && health.OwnerID != OwnerId)
                return true;

            return false;

        }).ToArray();

        foreach (var hit in hits)
        {
            CalculateDamage();

            var health = hit.collider.transform.root.GetComponent<HealthComponent>();
            health.Damage(dmg, OwnerId);

            ProjectilePool.Set(this);
            break;
        }

        prevPoint = curPoint;
        curPoint = transform.position;
    }

    void CalculateDamage()
    {
        int critChance = projectileData?.critChance ?? 5;// HOT FIX при стопе игры
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
    public int ownerID;
    public Team team;
    public Material mat;
}
