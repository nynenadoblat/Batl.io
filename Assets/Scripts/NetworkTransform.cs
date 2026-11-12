using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class NetworkTransform : NetworkBehaviour
{
    [SerializeField]
    private float minSyncPosSpeed = 10f;
    [SerializeField]
    private float minSyncRotSpeed = 70f;

    private readonly NetworkVariable<Vector3> netPosition = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private readonly NetworkVariable<Vector3> netRotation = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            netPosition.Value = transform.position;
            netRotation.Value = transform.rotation.eulerAngles;
        }
        else
        {
            if (netPosition.Value != default)
            {
                transform.position = netPosition.Value;
                transform.rotation = Quaternion.Euler(netRotation.Value);
            }
        }
    }

    private void Update()
    {
        if (!IsSpawned)
            return;

        if (IsOwner)
        {
            if (Vector3.Distance(transform.position, netPosition.Value) > 0.005f)
            {
                netPosition.Value = transform.position;
            }

            if (Quaternion.Angle(transform.rotation, Quaternion.Euler(netRotation.Value)) > 0.5f)
            {
                netRotation.Value = transform.rotation.eulerAngles;
            }
        }
        else
        {
            float dist = Vector3.Distance(transform.position, netPosition.Value);
            float syncPosSpeed = Mathf.Clamp(dist * 8f, minSyncPosSpeed, float.MaxValue);
            transform.position = Vector3.MoveTowards(transform.position, netPosition.Value, Time.deltaTime * syncPosSpeed);

            float angleDiff = Quaternion.Angle(transform.rotation, Quaternion.Euler(netRotation.Value));
            float syncRotSpeed = Mathf.Clamp(angleDiff * 8f, minSyncRotSpeed, float.MaxValue);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(netRotation.Value), Time.deltaTime * syncRotSpeed);
        }
    }
}