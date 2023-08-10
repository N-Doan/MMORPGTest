using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  HUB scriptableobject used for accessing all SOs associated with a specific player
 */

[CreateAssetMenu]
public class PlayerID : ScriptableObject
{
    public playerSO playerSO;
    public PlayerInventory playerInventory;

}
