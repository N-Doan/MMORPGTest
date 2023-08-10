using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
/*
 * Network object sript representing a dropped item in the overworld. Disappears when interacted with by a player
 * Item drops can be randomized from a set list of possible items, useful for enemy drop pools
 * 
 */
public class DropItem : NetworkBehaviour
{
    [SerializeField]
    private Item item;

    [SerializeField]
    private bool randomized;

    [SerializeField]
    private List<Item> possibleItems;

    public override void OnNetworkSpawn()
    {
        if (randomized)
        {
            item = possibleItems[(int)Random.Range(0, possibleItems.Count)];
        }
    }

    public Item pickupItem()
    {
        despawnObjectServerRPC();
        return item;
    }

    [ServerRpc(RequireOwnership = false)]
    private void despawnObjectServerRPC()
    {
        GetComponent<NetworkObject>().Despawn(true);
    }
}
