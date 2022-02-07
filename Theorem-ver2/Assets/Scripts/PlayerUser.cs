using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class PlayerUser : NetworkBehaviour
{

    //is the User attackable
    public bool isAttackable = true;

    //attackable Position
    [SerializeField]
    private Vector3 attackablePos;

    //safe Position
    [SerializeField]
    private Vector3 safePos;

    public void setAttackable(bool value)
    {
        PlayerManager playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();

        if (GameObject.Find("PlayerStats").GetComponent<PlayerStats>().roundCounter != 0)
        {
            this.isAttackable = value;
            if (isAttackable)
            {
                transform.DOMove(attackablePos, 0.5f);
            }
            else
            {
                transform.DOMove(safePos, 0.5f);
            }
            playerManager.setEnemyAttackableCmd(isAttackable);
        }
    }

}
