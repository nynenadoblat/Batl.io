using UnityEngine;
using Unity.Netcode;

public class NetworkCitadel : NetworkBehaviour
{
    Citadel citadel;

    private void Awake()
    {
        citadel = GetComponent<Citadel>();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer && Time.time > 7)
        {
            citadel.Health.Annigilaion();
        }
    }
}