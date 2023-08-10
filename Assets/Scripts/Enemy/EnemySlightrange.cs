using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySlightrange : MonoBehaviour
{
    [SerializeField]
    private EnemyController controller;
    [SerializeField]
    private ContactFilter2D contactFilter;


    CircleCollider2D col;
    private void Start()
    {
        col = gameObject.GetComponent<CircleCollider2D>();
    }

    private void FixedUpdate()
    {
        List<Collider2D> overlaps = new List<Collider2D>();
        col.OverlapCollider(contactFilter, overlaps);
        foreach (Collider2D col in overlaps)
        {
            if (col.CompareTag("Player"))
            {
                controller.playerSpotted(col.gameObject.transform);
            }
        }
        }
}
