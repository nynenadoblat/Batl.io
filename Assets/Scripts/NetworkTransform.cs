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
        if (IsOwner)
        {
            if (Vector3.Distance(transform.position, netPosition.Value) > 0.005f)
            {
                netPosition.Value = transform.position;
            }

            if (Vector3.Distance(transform.rotation.eulerAngles, netRotation.Value) > 0.1f)
            {
                netRotation.Value = transform.rotation.eulerAngles;
            }
        }
        else
        {
            float syncPosSpeed = Mathf.Clamp(Vector3.Distance(transform.position, netPosition.Value), minSyncPosSpeed, float.MaxValue);
            transform.position = Vector3.MoveTowards(transform.position, netPosition.Value, Time.deltaTime * syncPosSpeed);

            float syncRotSpeed = Mathf.Clamp(Vector3.Distance(transform.rotation.eulerAngles, netRotation.Value), minSyncRotSpeed, float.MaxValue);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(netRotation.Value), Time.deltaTime * syncRotSpeed);
        }
    }
}