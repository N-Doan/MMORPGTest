using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * GameObject container for player ID placed at root of player prefab
 */
public class Player : NetworkBehaviour
{
    public PlayerID id;

    private void Awake()
    {
        id = ScriptableObject.CreateInstance<PlayerID>();
        id.playerInventory = ScriptableObject.CreateInstance<PlayerInventory>();
        id.playerSO = ScriptableObject.CreateInstance<playerSO>();
    }
}
