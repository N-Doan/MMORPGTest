using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * animation controller for player. Flips sprite when facing right so animations line up and checks for death
 * in order to play death anim
 * 
 */
public class PlayerAnimationController : NetworkBehaviour
{
    private PlayerID playerID;
    [SerializeField]
    private Animator animator;
    private SpriteRenderer sprite;
    private NetworkVariable<bool> flipSprite = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        sprite = gameObject.GetComponent<SpriteRenderer>();
        playerID = gameObject.transform.parent.GetComponent<Player>().id;
        playerID.playerSO.health.OnValueChanged += healthChanged;
    }

    //get move dir and flip sprite if moving right
    private void Update()
    {
        float xDir = Mathf.Round(playerID.playerSO.moveDir.x);
        float yDir = Mathf.Round(playerID.playerSO.moveDir.y);
        animator.SetInteger("XDir", (int)xDir);
        animator.SetInteger("YDir", (int)yDir);
        if (playerID.playerSO.moveDir.Equals(Vector2.right) || playerID.playerSO.moveDir.Equals(Vector2.zero) && flipSprite.Value == true)
        {
            flipSprite.Value = true;
        }
        else
        {
            flipSprite.Value = false;
        }
        sprite.flipX = flipSprite.Value;
    }

    private void healthChanged(float prev, float cur)
    {
        if (playerID.playerSO.health.Value <= 0)
        {
            animator.SetBool("isDead", true);
        }
        else
        {
            animator.SetBool("isDead", false);
        }
    }
}
