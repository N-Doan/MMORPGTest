using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * Enemy movement and attacking logic. Enemy chases player caught within sight radius and attacks in their direction when
 * near. Enemy will be disabled upon losing all health and drop an item.
 * 
 */

public class EnemyController : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float followDist = 1.5f;
    [SerializeField]
    private float followRange = 0.1f;
    [SerializeField]
    private EnemyAttack atk;

    [SerializeField]
    private float defaultHealth = 50.0f;
    public NetworkVariable<float> health = new NetworkVariable<float>(50.0f);

    private Vector3 moveDir;
    public Vector3 getMoveDir() { return moveDir; }

    [SerializeField]
    private float atkDamage = 5.0f;
    public float getAtkDamage() { return atkDamage; }

    private Transform target;
    private Rigidbody2D rb;

    [SerializeField]
    private GameObject dropPref;

    private NetworkVariable<bool> dead = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        health.OnValueChanged += checkHealth;
        //check if enemy is already dead (when client joins after enemy is killed by host)
        if (dead.Value)
        {
            gameObject.SetActive(false);
        }
    }

    //set target
    public void playerSpotted(Transform spotted)
    {
        target = spotted;
    }

    //update enemy's health
    [ServerRpc(RequireOwnership = false)]
    public void updateHealthServerRPC(float damageDealt)
    {
        health.Value = health.Value - damageDealt;
        Debug.Log(health.Value);
    }

    //Event to check when the enemy is dead
    public void checkHealth(float prev, float cur)
    {
        if(health.Value <= 0)
        {
            Debug.Log("ENEMY DIED!");
            rb.isKinematic = true;
            dead.Value = true;
            spawnDropServerRPC();
            disableEnemyServerRPC();
        }
    }

    [ServerRpc]
    private void spawnDropServerRPC()
    {
        GameObject dropGO = Instantiate(dropPref);
        dropGO.transform.position = gameObject.transform.position;
        dropGO.GetComponent<NetworkObject>().Spawn(true);
    }

    //disable enemy on server and clients
    [ServerRpc]
    private void disableEnemyServerRPC()
    {
        gameObject.SetActive(false);
        disableEnemyClientRPC();
    }
    [ClientRpc]
    private void disableEnemyClientRPC()
    {
        gameObject.SetActive(false);
    }

    //move towards target if noth within follow distance, if within distance attack instead
    private void FixedUpdate()
    {
        if (dead.Value) { return; }
        if (target != null)
        {
            if((target.position - transform.position).magnitude > followDist+followRange)
            {
                moveDir = (target.position - transform.position).normalized;
                rb.MovePosition(transform.position + moveDir * moveSpeed * Time.fixedDeltaTime);
            }
            else
            {
                if (atk.attacking) { return; }
                atk.enableDisableAttackServerRPC(true);
            }
        }
    }
}
