using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NextButton : NetworkBehaviour
{

    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite hoverSprite;

    public void OnMouseEnter()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = hoverSprite;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
    }

    public void OnMouseDown()
    {
        GameObject.Find("PlayerStats").GetComponent<PlayerStats>().nextGameStatus();
    }

}
