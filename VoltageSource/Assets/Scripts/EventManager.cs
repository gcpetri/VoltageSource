using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{

    public enum EventCodes
    {
        GeneralEvent,
        ChangeTeamSelection,
        UpdateNamePlate,
        MatchStart,
        StartPreRound,
        EndPreRound,
        StartRound,
        EndRound,
        MatchEnd,
        PlayerDied,
        SpawnGun,
        ChangeCharacterColor1,
        ChangeCharacterColor2
    }
}
