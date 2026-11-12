using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class EventsHolder
{

    public class PlayerSpawned : UnityEvent<Player> { }

    public static PlayerSpawned playerSpawnedMine = new();

    //-----------------------------------------------------------------------

    public class EnemyDestroyed : UnityEvent<Transform, int> { }

    public static EnemyDestroyed enemyDestroyed = new EnemyDestroyed();

    //-----------------------------------------------------------------------

    public class PlayerSpawnedAny : UnityEvent<Player> { }

    public static PlayerSpawnedAny playerSpawnedAny = new PlayerSpawnedAny();

    //-----------------------------------------------------------------------

    public class LeftJoystickMoved : UnityEvent<Vector2> { }

    public static LeftJoystickMoved leftJoystickMoved = new LeftJoystickMoved();

    //-----------------------------------------------------------------------

    public class RightJoystickMoved : UnityEvent<Vector2> { }

    public static RightJoystickMoved rightJoystickMoved = new RightJoystickMoved();

    //-----------------------------------------------------------------------

    public class RightJoystickUp : UnityEvent { }

    public static RightJoystickUp rightJoystickUp = new RightJoystickUp();

    //-----------------------------------------------------------------------

    public class JumpClicked : UnityEvent { }

    public static JumpClicked jumpClicked = new JumpClicked();

    //-----------------------------------------------------------------------

    public class PlayerAnniged : UnityEvent<Player> { }

    public static PlayerAnniged playerAnniged = new ();

    ////-----------------------------------------------------------------------

    public class CreepSpawned : UnityEvent<HealthComponent> { }

    public static CreepSpawned creepSpawned = new();

    ////-----------------------------------------------------------------------

    public class CreepAnniged : UnityEvent<HealthComponent> { }

    public static CreepAnniged creepAnniged = new();

    ////-----------------------------------------------------------------------

    public class TeamPicked : UnityEvent<HealthComponent> { }

    public static TeamPicked teamPicked = new();

    ////-----------------------------------------------------------------------

    public class MineTeamPicked : UnityEvent<Team> { }

    public static MineTeamPicked mineTeamPicked = new();

    ////-----------------------------------------------------------------------

    public class ClientGameCompleted : UnityEvent { }

    public static ClientGameCompleted clientGameCompleted = new();

    ////-----------------------------------------------------------------------

    public class CrepochekDamage : UnityEvent<HealthComponent> { }

    public static CrepochekDamage onCrepochekDamage = new();

    ////-----------------------------------------------------------------------

    public class ProfileSeted : UnityEvent<AIProfile, Player> { }

    public static ProfileSeted profileSeted = new ();

    ////-----------------------------------------------------------------------

    public class OnVictory : UnityEvent<Team> { }

    public static OnVictory onVictory = new();

    //-----------------------------------------------------------------------

    public class OnDefeat : UnityEvent<Team> { }

    public static OnDefeat onDefeat = new();

    //-----------------------------------------------------------------------

    public class ClientGameCompleteDataReceived : UnityEvent { }

    public static ClientGameCompleteDataReceived clientCompleteDataReceived = new();

    ////-----------------------------------------------------------------------

    
    public class CamClicked : UnityEvent { }

    public static CamClicked onCamClicked = new();

    ////-----------------------------------------------------------------------

    public class RuneSpawned : UnityEvent<Rune> { }

    public static RuneSpawned runeSpawned = new();

    ////-----------------------------------------------------------------------
    
    public class RuneTaked : UnityEvent<Player> { }

    public static RuneTaked runeTaked = new();

    ////-----------------------------------------------------------------------

    public class RuneEnded : UnityEvent<Player> { }

    public static RuneEnded runeEnded = new();

    ////-----------------------------------------------------------------------
    ///
    public class NextStageAvailable : UnityEvent<Player> { }

    public static NextStageAvailable nextStageAvailable = new();

    ////-----------------------------------------------------------------------

}
