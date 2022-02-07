using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class BacksideCard : NetworkBehaviour
{

    //attack of the card
    public int attack;

    //defense of the card
    public int defense;

    //is card currently in defense mode
    public bool inDefenseMode = false;

    //is the mouse Over the card
    private bool mouseOver = false;

    //is in GameField
    public bool inGameField = false;

    //did card attack in attack phase
    public bool didCardAttack = false;

    //is the card in hiddendefense mode
    public bool inHiddenDefense = false;

    //Sprite of the card
    public Sprite sprite;

    //moves and rotates the card
    public void moveCardTo(Vector3 position, Vector3 rotation)
    {
        transform.DOMove(position, 1f).SetEase(Ease.InOutQuint);
        transform.DORotate(rotation, 1f);
    }

    public void moveCardTo(Vector3 position)
    {
        transform.DOMove(position, 1f).SetEase(Ease.InOutQuint);
    }

    public void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (mouseOver && inGameField)
            {
                GameObject.Find("PlayerStats").GetComponent<PlayerStats>().addAttackedCard(gameObject);
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

    public int getFightValue()
    {
        if (inDefenseMode)
        {
            return defense;
        }else
        {
            return attack;
        }
    }

    public void setDidCardAttack(bool value)
    {
        this.didCardAttack = value;
        if (didCardAttack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            if (!inDefenseMode)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    public void setInDefenseMode(bool value)
    {
        inDefenseMode = value;
        if (inDefenseMode)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 1f, 1f);
        }else
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }

    IEnumerator flipCard()
    {
        if (inHiddenDefense)
        {
            //yield return gameObject.transform.DORotate(new Vector3(0, 180, 0), 1f);
            yield return new WaitForSeconds(0.1f);
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/Cards/C0000");
        }
        else
        {
            //gameObject.transform.DORotate(new Vector3(0, 0, 0), 1f);
            yield return new WaitForSeconds(0.1f);
            gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    public void setInHiddenDefense(bool value)
    {
        this.inHiddenDefense = value;
        StartCoroutine(flipCard());
    }

}
