using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/*
 * UI manager for clients. Displays current HP, and handles player input for inventory and gear management.
 * Displays updated stats which change dynamically as the player consumes / equips / unequips items.
 * 
 */

public class UIManager : NetworkBehaviour
{
    [SerializeField]
    private Slider HPSlider;
    [SerializeField]
    private VerticalLayoutGroup itemList;
    [SerializeField]
    private VerticalLayoutGroup equipList;
    [SerializeField]
    private GameObject itemUIPref;
    private PlayerID id;
    [SerializeField]
    private GameObject statsAndInvMenu;
    [SerializeField]
    private GameObject equipmentMenu;
    [SerializeField]
    private GameObject inventoryMenu;
    [SerializeField]
    private Button equipInvUIToggle;

    [SerializeField]
    private GameObject statList;

    [SerializeField]
    private GameObject respawnButton;

    private Text[] stats;
    private List<GameObject> inventoryElements;
    private List<GameObject> equipmentElements;

    [SerializeField]
    private GameObject useItem;

    private Item selectedItem;

    public override void OnNetworkSpawn()
    {
        //disable if they aren't the owner of this player
        if (!IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }
        id = transform.root.GetComponent<Player>().id;
        stats = statList.GetComponentsInChildren<Text>();
        inventoryElements = new List<GameObject>();
        equipmentElements = new List<GameObject>();
        useItem.SetActive(false);
        statsAndInvMenu.SetActive(false);
        respawnButton.SetActive(false);
        equipList.transform.parent.parent.gameObject.SetActive(false);

        id.playerSO.health.OnValueChanged += updateHP;
        id.playerSO.defaultHealth.OnValueChanged += updateHP;
    }

    //update HP slider when either health or default(max) health are changed
    public void updateHP(float prevVal, float newVal)
    {
        if (!IsOwner) return;
        HPSlider.value = id.playerSO.health.Value / id.playerSO.defaultHealth.Value;
        if(id.playerSO.health.Value <= 0)
        {
            //ENABLE RESPAWN BUTTON
            respawnButton.SetActive(true);
        }
    }

    public void toggleMenu()
    {
        if (!IsOwner) return;
        statsAndInvMenu.SetActive(!statsAndInvMenu.activeInHierarchy);
        if (gameObject.activeInHierarchy)
        {
            populateInventoryUI();
            updateStats();
        }
    }

    //switch between inventory and equipment list
    public void toggleInvEquipment()
    {
        if (!IsOwner) return;
        //if currently displaying equipment list
        if (equipmentMenu.gameObject.activeInHierarchy)
        {
            equipmentMenu.SetActive(false);
            inventoryMenu.SetActive(true);
            populateInventoryUI();
        }
        //if displaying inv list
        else
        {
            equipmentMenu.SetActive(true);
            inventoryMenu.SetActive(false);
            populateEquipmentUI();
        }
    }

    //instantiate itemUIPrefs for each item in the player's inventory and set their onclicks to the appropriate method
    private void populateInventoryUI()
    {
        if (!IsOwner) return;
        //CLEAR OLD INVENTORY LIST
        foreach (GameObject g in inventoryElements)
        {
            Destroy(g);
        }
        inventoryElements.Clear();

        foreach(Item item in id.playerInventory.inventory)
        {
            GameObject itemPref = GameObject.Instantiate(itemUIPref, itemList.gameObject.transform);
            ItemUIElement itemUI = itemPref.GetComponent<ItemUIElement>();
            inventoryElements.Add(itemPref);
            itemUI.item = item;
            itemUI.setDispText();

            itemUI.button.onClick.AddListener(() => onInvItemSelected(item));
        }
    }

    //instantiate itemUIPrefs for each gear in the player's equips and set their onclicks to the appropriate method
    private void populateEquipmentUI()
    {
        if (!IsOwner) return;
        //CLEAR OLD EQUIP LIST
        foreach (GameObject g in equipmentElements)
        {
            Destroy(g);
        }
        equipmentElements.Clear();

        foreach (Item item in id.playerInventory.equipment)
        {
            GameObject itemPref = GameObject.Instantiate(itemUIPref, equipList.gameObject.transform);
            ItemUIElement itemUI = itemPref.GetComponent<ItemUIElement>();
            equipmentElements.Add(itemPref);
            itemUI.item = item;
            itemUI.setDispText();

            itemUI.button.onClick.AddListener(() => onEquipItemSelected(item));
        }
    }

    private void onInvItemSelected(Item item)
    {
        equipInvUIToggle.enabled = false;
        useItem.SetActive(!useItem.activeInHierarchy);
        selectedItem = item;
        useItem.GetComponentInChildren<Text>().text = "Use the item " + item.name + "?";
    }

    private void onEquipItemSelected(Item item)
    {
        equipInvUIToggle.enabled = false;
        useItem.SetActive(!useItem.activeInHierarchy);
        selectedItem = item;
        useItem.GetComponentInChildren<Text>().text = "Unequip " + item.name + "?";
    }

    public void onRespawnPressed()
    {
        respawnButton.SetActive(false);
        gameObject.transform.root.GetComponent<PlayerController>().respawnServerRPC();
    }

    public void useItemY()
    {
        if (!IsOwner) return;
        equipInvUIToggle.enabled = true;
        if (selectedItem != null)
        {
            switch (selectedItem.itemType)
            {
                //if consumable throw away item and init effect
                case Item.type.CONSUMABLE:
                    selectedItem.initEffect(id);
                    id.playerInventory.throwAwayItemServerRPC(selectedItem);
                    break;
                case Item.type.GEAR:
                    //If player is viewing the equipMenu
                    if (id.playerInventory.equipment.Contains(selectedItem) && !inventoryMenu.activeInHierarchy)
                    {
                        //UNEQUIP
                        id.playerInventory.unequipItemServerRPC(selectedItem);
                        selectedItem.unequipItem(id);
                        Debug.Log("Removed " + selectedItem.name);
                    }
                    //if player's trying to equip identical (duplicate) item to what they already have in equip slot
                    else if(id.playerInventory.equipment.Contains(selectedItem) && inventoryMenu.activeInHierarchy)
                    {
                        Debug.Log(selectedItem.name+" is already in your Equipment slot!");
                    }
                    else
                    {
                        //EQUIP
                        selectedItem.initEffect(id);
                        id.playerInventory.equipItemServerRPC(selectedItem);
                    }
                    break;
            }
            selectedItem = null;
            populateInventoryUI();
            populateEquipmentUI();
            updateStats();
        }
        useItem.SetActive(false);
    }

    public void useItemN()
    {
        if (!IsOwner) return;
        equipInvUIToggle.enabled = true;
        selectedItem = null;
        useItem.SetActive(false);
    }

    private void updateStats()
    {
        if (!IsOwner) return;
        stats[0].text = "MAX HP: " + id.playerSO.defaultHealth.Value;
        stats[1].text = "ATTACK: " + id.playerSO.attack.Value;
        stats[2].text = "DEF: " + id.playerSO.defense.Value;
    }
}
