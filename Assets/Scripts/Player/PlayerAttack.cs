using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * Handles collision checking and object rotation for player's attack. Attack must always face direction of last travel
 * 
 */

public class PlayerAttack : NetworkBehaviour
{
    PlayerID id;
    private bool attacking = false;
    private BoxCollider2D atkCollider;
    [SerializeField]
    private ContactFilter2D contactFilter;

    private void Start()
    {
        atkCollider = gameObject.GetComponent<BoxCollider2D>();
        gameObject.SetActive(false);
        id = gameObject.transform.root.GetComponent<Player>().id;
    }
    private void OnEnable()
    {
        attacking = true;
        updateAtkRot();
    }


    //Rotating attack GO to follow current travel direction
    private void FixedUpdate()
    {
        updateAtkRot();
        checkCollisions();
    }

    private void checkCollisions()
    {
        List<Collider2D> overlaps = new List<Collider2D>();
        atkCollider.OverlapCollider(contactFilter ,overlaps);
        foreach(Collider2D col in overlaps)
        {
            //skip loop iteration if this is the parent
            if (gameObject.transform.IsChildOf(col.gameObject.transform)) { continue; }
            if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy"))
            {
                //DO DAMAGE TO PLAYER
                if (col.gameObject.GetComponent<Player>())
                {
                    Player pHit = col.gameObject.GetComponent<Player>();
                    pHit.id.playerSO.updateHealthServerRPC(id.playerSO.getDefaultAttack() + 1000.0f);
                }

                //DO DAMAGE TO ENEMY
                else if (col.gameObject.GetComponent<EnemyController>())
                {
                    EnemyController eHit = col.gameObject.GetComponent<EnemyController>();
                    eHit.updateHealthServerRPC(id.playerSO.getDefaultAttack() + 100.0f);
                }
                gameObject.GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void updateAtkRot()
    {
        if(id != null && IsOwner)
        {
            //Get req angle based of direction of last travel
            float targetRotation = Mathf.Atan2(id.playerSO.lastMoveDir.normalized.y, id.playerSO.lastMoveDir.normalized.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0f, 0f, targetRotation + 90.0f);
            updateAtkRotServerRPC(transform.eulerAngles);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void updateAtkRotServerRPC(Vector3 angles)
    {
        updateAtkRotClientRPC(angles);
    }

    [ClientRpc]
    private void updateAtkRotClientRPC(Vector3 angles)
    {
        transform.eulerAngles = angles;
    }

    [ClientRpc]
    private void enableDisableAttackClientRPC(bool v)
    {
        switch (v)
        {
            case true:
                gameObject.SetActive(true);
                attacking = true;
                StartCoroutine(despawnCountdown());
                break;

            case false:
                gameObject.GetComponent<Collider2D>().enabled = true;
                gameObject.SetActive(false);
                attacking = false;
                break;
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void enableDisableAttackServerRPC(bool v)
    {
        enableDisableAttackClientRPC(v);
    }

    //timeout for attack. Lasts 2 seconds.
    private IEnumerator despawnCountdown()
    {
        yield return new WaitForSeconds(2.0f);
        attacking = false;
        enableDisableAttackServerRPC(false);
        yield return null;
    }
}
