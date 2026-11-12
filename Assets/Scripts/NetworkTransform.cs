using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


[RequireComponent(typeof(PhotonView))]
public class NetworkTransform : MonoBehaviour, IPunObservable
{
    [SerializeField]
    private float minSyncPosSpeed = 10;
    [SerializeField]
    private float minSyncRotSpeed = 70;

    private new PhotonView networkView;

    private Vector3 receivedPos;
    private Vector3 receivedRot;

    private void Start()
    {
        networkView = GetComponent<PhotonView>();

        receivedPos = transform.position;
        receivedRot = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (!networkView.IsMine)
        {
            float syncPosSpeed = Mathf.Clamp(Vector3.Distance(transform.position, receivedPos), minSyncPosSpeed, float.MaxValue);
            transform.position = Vector3.MoveTowards(transform.position, receivedPos, Time.deltaTime * syncPosSpeed);

            float syncRotSpeed = Mathf.Clamp(Vector3.Distance(transform.rotation.eulerAngles, receivedRot), minSyncRotSpeed, float.MaxValue);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(receivedRot), Time.deltaTime * syncRotSpeed);
        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation.eulerAngles);
        }
        else
        {
            receivedPos = (Vector3)stream.ReceiveNext();
            receivedRot = (Vector3)stream.ReceiveNext();
        }
    }
}
