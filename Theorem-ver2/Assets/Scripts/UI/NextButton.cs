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

    private bool isDisabled = false;

    private void Start()
    {
        setIsDisabled(true);
    }

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
        if (!isDisabled)
        {
            GameObject.Find("PlayerStats").GetComponent<PlayerStats>().nextGameStatus();
        }
    }

    public void setIsDisabled(bool value)
    {
        this.isDisabled = value;
        if (isDisabled)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

}
