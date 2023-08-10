using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/*
 * ScriptableObject containing all player game data and relevant getters
 */

[CreateAssetMenu]
public class playerSO : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 200.0f;
    public Vector2 moveDir;
    public Vector2 lastMoveDir = Vector2.down;

    public Vector3 currentPosition = Vector2.zero;

    [Header("CombatStats")]
    public NetworkVariable<float> defaultHealth = new NetworkVariable<float>(100.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<float> health = new NetworkVariable<float>(100.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [ServerRpc]
    public void updateHealthServerRPC(float damageDealt)
    {
        health.Value = health.Value - (damageDealt - defense.Value);
    }


    [SerializeField]
    private NetworkVariable<float> defaultAttack = new NetworkVariable<float>(10.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float getDefaultAttack() { return defaultAttack.Value; }
    public void setDefaultAttack(float newATK) { defaultAttack.Value = newATK; }
    public NetworkVariable<float> attack = new NetworkVariable<float>(10.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField]
    private NetworkVariable<float> defaultDefense = new NetworkVariable<float>(5.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float getDefaultDefense() { return defaultDefense.Value; }
    public void setDefaultDefense(float newDEF) { defaultDefense.Value = newDEF; }

    public NetworkVariable<float> defense = new NetworkVariable<float>(5.0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
}
