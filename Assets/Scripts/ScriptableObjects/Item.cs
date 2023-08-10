using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 *  ScriptableObject representing a game item. Can be either consumable or gear and have
 *  varrying effects on player's stats. Unequip method does nothing if item is consumable, since they cannot be equiped
 */

[CreateAssetMenu]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemDesc;
    public enum type
    {
        CONSUMABLE,
        GEAR
    }
    public type itemType;

    [Header("Gear Effects")]
    public float attackModifier;
    public float maxHPModifier;
    public float maxDefModifier;

    [Header("Consumable Effects")]
    public float hpRecovery;
    public void initEffect(PlayerID id)
    {
        switch (itemType)
        {
            case type.CONSUMABLE:
                //DO EFFECT
                id.playerSO.health.Value += hpRecovery;
                break;

            case type.GEAR:
                id.playerSO.attack.Value += attackModifier;
                id.playerSO.defaultHealth.Value += maxHPModifier;
                id.playerSO.defense.Value += maxDefModifier;
                break;
        }

    }

    public void unequipItem(PlayerID id)
    {
        switch (itemType)
        {
            case type.CONSUMABLE:
                //DO NOTHING
                break;

            case type.GEAR:
                id.playerSO.attack.Value -= attackModifier;
                id.playerSO.defaultHealth.Value -= maxHPModifier;
                id.playerSO.defense.Value -= maxDefModifier;
                break;
        }
    }

}
