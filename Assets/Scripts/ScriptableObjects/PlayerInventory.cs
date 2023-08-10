using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * ScriptableObject representation of inventory system. Contains two lists: one for inventory and the other for currently
 * active equips. Size limits are placed on both lists since most inventories have limited space.
 */
[CreateAssetMenu]
public class PlayerInventory : ScriptableObject
{
    public int inventorySize = 32;
    public List<Item> inventory = new List<Item>();

    [ServerRpc]
    public bool addItemServerRPC(Item item)
    {
        if (inventory.Count < 32)
        {
            Debug.Log(item.name + " Was added to player's inventory!");
            inventory.Add(item);
            return true;
        }
        else
        {
            Debug.Log("No room in player's Inventory!");
            return false;
        }
    }

    public int maxEquip = 5;
    public List<Item> equipment = new List<Item>();
    [ServerRpc]
    public bool equipItemServerRPC(Item item)
    {
        if(item.itemType == Item.type.GEAR)
        {
            if(equipment.Count < 5)
            {
                equipment.Add(item);
                inventory.Remove(item);
                Debug.Log(item.name + " Was added to player's equipment slot!");
                return true;
            }
        }
        Debug.Log("Equip failed!");
        return false;
    }

    [ServerRpc]
    public bool unequipItemServerRPC(Item item)
    {
        if (equipment.Contains(item))
        {
            equipment.Remove(item);
            inventory.Add(item);
            return true;
        }
        Debug.Log("Unequip Failed");
        return false;
    }

    [ServerRpc]
    public bool throwAwayItemServerRPC(Item item)
    {
        return inventory.Remove(item);
    }

}
