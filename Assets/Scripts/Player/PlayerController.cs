using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;
/*
 * Player controller handling player inputs from inputSystem. Also resets player when they respawn after dying
 */
public class PlayerController : NetworkBehaviour
{

    private PlayerID playerID;
    private PlayerInput input = null;
    private Rigidbody2D rb;
    private CircleCollider2D interactRadius;
    [SerializeField]
    private ContactFilter2D interactableFilter;
    [SerializeField]
    private UIManager ui;

    [SerializeField]
    private GameObject attackGO;

    public override void OnNetworkSpawn()
    {
        playerID = gameObject.GetComponent<Player>().id;
        playerID.playerSO.health.OnValueChanged += checkHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        interactRadius = gameObject.GetComponentInChildren<CircleCollider2D>();
    }
    private void Awake()
    {
        input = new PlayerInput();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.Movement.performed += onMovement;
        input.Player.Movement.canceled += onMovementCancelled;
        input.Player.Select.performed += onSelect;
        input.Player.BackMenuToggle.performed += onBack;
        input.Player.Attack.performed += onAttack;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.Movement.performed -= onMovement;
        input.Player.Select.performed -= onSelect;
        input.Player.BackMenuToggle.performed -= onBack;
    }

    private void onMovement(InputAction.CallbackContext c)
    {
        if (!IsOwner) { return; }
        playerID.playerSO.moveDir = c.ReadValue<Vector2>();
        playerID.playerSO.lastMoveDir = playerID.playerSO.moveDir;
    }
    private void onMovementCancelled(InputAction.CallbackContext c)
    {
        if (!IsOwner) { return; }
        playerID.playerSO.moveDir = Vector2.zero;
    }

    private void onSelect(InputAction.CallbackContext c)
    {
        if (!IsOwner) { return; }
        List<Collider2D> results = new List<Collider2D>();
        interactRadius.OverlapCollider(interactableFilter, results);
        if(results.Count > 0)
        {
            playerID.playerInventory.addItemServerRPC(results[0].gameObject.GetComponent<DropItem>().pickupItem());
        }
    }
    private void onBack(InputAction.CallbackContext c)
    {
        if (!IsOwner) { return; }
        ui.toggleMenu();
    }

    private void onAttack(InputAction.CallbackContext c)
    {
        if(!IsOwner) { return; }
        attackGO.GetComponent<PlayerAttack>().enableDisableAttackServerRPC(true);

    }

    private void checkHealth(float prevVal, float newVal)
    {
        if(newVal <= 0)
        {
            Debug.Log("PLAYER DIED");
            input.Disable();
        }
    }

    //Respawn player after they press UI button
    [ServerRpc]
    public void respawnServerRPC()
    {
        playerID.playerSO.health.Value = playerID.playerSO.defaultHealth.Value;
        transform.position = Vector3.zero;
        input.Enable();
        respawnClientRPC();
    }

    //Reset health, move player back to spawn location, re-enable inputs
    [ClientRpc]
    private void respawnClientRPC()
    {
        playerID.playerSO.health.Value = playerID.playerSO.defaultHealth.Value;
        transform.position = Vector3.zero;
        input.Enable();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) { return; }
        rb.velocity = playerID.playerSO.moveDir * playerID.playerSO.moveSpeed * Time.fixedDeltaTime;
    }
}
