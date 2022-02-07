using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class EnemyUser : NetworkBehaviour
{
    //is the mouse Over the card
    private bool mouseOver = false;

    //is the User attackable
    public bool isAttackable = true;

    //attackable Position
    [SerializeField]
    private Vector3 attackablePos;

    //safe Position
    [SerializeField]
    private Vector3 safePos;

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mouseOver && isAttackable)
            {
                GameObject.Find("PlayerStats").GetComponent<PlayerStats>().cardIsAttackingUser();
            }
        }
    }

    public void OnMouseEnter()
    {
        mouseOver = true;
    }

    public void OnMouseExit()
    {
        mouseOver = false;
    }

    public void setAttackable(bool value)
    {
        this.isAttackable = value;
        if (isAttackable)
        {
            transform.DOMove(attackablePos, 0.5f);
        }else
        {
            transform.DOMove(safePos, 0.5f);
        }
    }
}
