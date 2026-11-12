using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

[Serializable]
public class TankTypeSet
{
    public TankType tankType;
    public MuzzleHandler handler;
}

public static class TankTypeExt
{
    public static TankType[] GetAvailableNextTypes(this TankType tankType)
    {
        TankType[] result = null;

        if (tankType == TankType.Simple)
        {
            result = new[] 
            { 
                TankType.Twin, 
                TankType.Trooper, 
                TankType.Guard 
            };
        }

        if (tankType == TankType.Twin)
        {
            result = new[] { TankType.TripleShot };
        }

        if (tankType == TankType.Trooper)
        {
            result = new[] { TankType.DualTrooper };
        }

        if (tankType == TankType.Guard)
        {
            result = new[] { TankType.QuadGuard };
        }
        //---------------------------------------
        if (tankType == TankType.QuadGuard)
        {
            result = new[] { TankType.Octo };
        }
        if (tankType == TankType.DualTrooper)
        {
            result = new[] { TankType.StormTrooper };
        }
        if (tankType == TankType.TripleShot)
        {
            result = new[] { TankType.Triplet };
        }

        return result;
    }
}

public enum TankType
{
    Simple = 0,
    Twin = 1,
    Trooper = 2,
    Guard = 3,

    TripleShot = 10,

    DualTrooper = 20,

    QuadGuard = 30,

    Octo = 50,
    StormTrooper = 51,
    Triplet = 52,
}
