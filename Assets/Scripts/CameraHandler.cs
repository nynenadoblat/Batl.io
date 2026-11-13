using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraHandler : MonoBehaviour
{
    [SerializeField] UniversalRenderPipelineAsset urpSettings;
    [SerializeField] Unity.Cinemachine.CinemachineVirtualCameraBase cam;

    [SerializeField] Vector3[] camOffsets;
    [SerializeField] Vector3[] stagesCamOffests;
    [SerializeField] float[] shadowDistances;

    int idxOffset = 0;

    private void Awake()
    {
        EventsHolder.playerSpawnedMine.AddListener(PlayerMine_Spawned);
        EventsHolder.onCamClicked.AddListener(Cam_Clicked);
        EventsHolder.nextStageAvailable.AddListener(NextStage_Availabled);
    }

    private void NextStage_Availabled(Player player)
    {
        var c = cam as Unity.Cinemachine.CinemachineVirtualCamera;
        var transposer = c.GetCinemachineComponent<Unity.Cinemachine.CinemachineTransposer>();

        var start = transposer.m_FollowOffset;
        LeanTween.value(gameObject, v =>
        {
            transposer.m_FollowOffset = v;
        }, start, stagesCamOffests[player.Stage - 1], 0.5f).setEaseInOutQuad();
        //print("���� �����");
    }

    private void Cam_Clicked()
    {
        if (Screen.height > Screen.width)
        {

        }

        idxOffset++;
        if(idxOffset >= camOffsets.Length)
        {
            idxOffset = 0;
        }

        var c = cam as Unity.Cinemachine.CinemachineVirtualCamera;
        var transposer = c.GetCinemachineComponent<Unity.Cinemachine.CinemachineTransposer>();

        var start = transposer.m_FollowOffset;
        LeanTween.value(gameObject, v =>
        {
            transposer.m_FollowOffset = v;
        }, start, camOffsets[idxOffset], 0.5f).setEaseInOutQuad();

        urpSettings.shadowDistance = shadowDistances[idxOffset];
    }

    private void PlayerMine_Spawned(Player player)
    {
        cam.Follow = player.transform;
        cam.LookAt = player.transform;
    }

    private void OnDestroy()
    {
        EventsHolder.playerSpawnedMine.RemoveListener(PlayerMine_Spawned);
        EventsHolder.onCamClicked.RemoveListener(Cam_Clicked);
        EventsHolder.nextStageAvailable.RemoveListener(NextStage_Availabled);
    }
}
