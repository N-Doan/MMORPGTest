using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyAttack : NetworkBehaviour
{
    private EnemyController controller;
    public bool attacking = false;
    private BoxCollider2D atkCollider;
    [SerializeField]
    private ContactFilter2D contactFilter;

    private void Start()
    {
        atkCollider = gameObject.GetComponent<BoxCollider2D>();
        controller = gameObject.transform.root.GetComponent<EnemyController>();
    }
    private void OnEnable()
    {
        updateAtkRot();
    }

    public override void OnNetworkSpawn()
    {
        gameObject.SetActive(false);
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
        atkCollider.OverlapCollider(contactFilter, overlaps);
        foreach (Collider2D col in overlaps)
        {
            //skip loop iteration if this is the parent
            if (gameObject.transform.IsChildOf(col.gameObject.transform)) { continue; }
            if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy"))
            {
                //DO DAMAGE TO PLAYER
                if (col.gameObject.GetComponent<Player>())
                {
                    Player pHit = col.gameObject.GetComponent<Player>();
                    pHit.id.playerSO.updateHealthServerRPC(controller.getAtkDamage());
                }
                //DO DAMAGE TO ENEMY
                gameObject.GetComponent<Collider2D>().enabled = false;
            }
        }
    }

    private void updateAtkRot()
    {
        if (controller != null && IsOwner)
        {
            //rotate to point at target
            float targetRotation = Mathf.Atan2(controller.getMoveDir().normalized.y, controller.getMoveDir().normalized.x) * Mathf.Rad2Deg;
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
