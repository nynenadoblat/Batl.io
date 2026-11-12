using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public static class MultiplayerExtensions
{
    /// <summary>
    /// Кароче этой хуйнёй вызываешь событие у Photon View
    /// </summary>
    public static void RaiseEvent(this PhotonView photonView, EventCode eventCode)
    {
        if (photonView.Owner != null && photonView.IsMine)
            MultiplayerManager.RaiseEvent(photonView, eventCode);
    }

    public static void RaiseEvent<T>(this PhotonView photonView, EventCode eventCode, T data, bool targetActor = false)
    {
        if (photonView.Owner != null && photonView.IsMine)
        {
            if (targetActor)
            {
                MultiplayerManager.RaiseEvent(photonView, eventCode, data);
            }
            else
            {
                MultiplayerManager.RaiseEvent(eventCode, data);
            }
        }
    }

    public static void RaiseEvent<T>(this PhotonView photonView, EventCode eventCode, T data, int targetActor)
    {
        if (photonView.Owner != null && photonView.IsMine)
        {
            MultiplayerManager.RaiseEvent(eventCode, data, targetActor);
        }
    }

    /// <summary>
    /// позволяет вызвать сетевое событие у НЕ MINE фотон вьюва
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="photonView"></param>
    /// <param name="eventCode"></param>
    /// <param name="data"></param>
    public static void RaiseEventAll<T>(this PhotonView photonView, EventCode eventCode, T data)
    {
        if (photonView.Owner != null)
        {
            //if (targetActor)
            //{
            //    MultiplayerManager.RaiseEvent(photonView, eventCode, data);
            //}
            //else
            //{
                MultiplayerManager.RaiseEvent(eventCode, data);
            //}
        }
    }

    public static void RaiseEvent<T>(this PhotonView photonView, EventCode eventCode, T data)
    {
        if (photonView.Owner != null && photonView.IsMine)
            MultiplayerManager.RaiseEvent(eventCode, data);
    }

    public static void RegisterMethod(this PhotonView photonView, EventCode eventCode, Action method)
    {
        if (photonView.Owner != null)
            MultiplayerManager.RegisterMethod(photonView, eventCode, method);
    }

    public static void RegisterMethod(this PhotonView photonView, EventCode eventCode, Action<object> method, bool allActors = false)
    {
        if (photonView.Owner != null)
            MultiplayerManager.RegisterMethod(photonView, eventCode, method, allActors);
    }

    public static void RegisterMethod<T>(this PhotonView photonView, EventCode eventCode, Action<T> method, bool allActors = false)
    {
        if (photonView.Owner != null)
            MultiplayerManager.RegisterMethod(photonView, eventCode, method, allActors);
    }

    public static bool IsMine(this MonoBehaviour component)
    {
        return component.GetComponent<PhotonView>().IsMine;
    }

    public static PhotonView GetView(this MonoBehaviour component)
    {
        return component.GetComponent<PhotonView>();
    }
}
