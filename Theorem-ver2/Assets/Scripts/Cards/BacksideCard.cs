using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;

public class BacksideCard : NetworkBehaviour
{

    //attack of the card
    public int attack;
    public int originalAttack;

    //defense of the card
    public int defense;
    public int originalDefense;

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

    //List of Tokens
    LinkedList<CardToken> tokens = new LinkedList<CardToken>();

    //List of Standard Effects
    private List<StandardEffects> standardEffects;

    public void init()
    {
        //insert fight token to tokens
        CardToken fightToken = new CardToken(0, 0, gameObject);
        tokens.AddFirst(fightToken);
    }

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
            int value = defense + tokens.First.Value.getDefenseChange();
            return value;
        }
        else
        {
            int value = attack + tokens.First.Value.getAttackChange();
            return value;
        }
    }

    public void takeDamage(int value)
    {
        if (inDefenseMode)
        {
            //defense -= value;
            int currDValue = tokens.First.Value.getDefenseChange();
            int currAValue = tokens.First.Value.getAttackChange();
            tokens.First.Value.setTokenValues(currAValue, currDValue - value);
        }
        else
        {
            //attack -= value;
            int currAValue = tokens.First.Value.getAttackChange();
            int currDValue = tokens.First.Value.getDefenseChange();
            tokens.First.Value.setTokenValues(currAValue - value, currDValue);
        }
    }

    public void resetFightDamage()
    {
        tokens.First.Value.setTokenValues(0, 0);
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
            gameObject.transform.Find("stance").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/icons/schild");
            //gameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 1f, 1f);
        }
        else
        {
            gameObject.transform.Find("stance").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/icons/schwert");
            //gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
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

    public bool hasEffect(StandardEffects effect)
    {
        return standardEffects.Contains(effect);
    }

    public void removeEffect(StandardEffects effect)
    {
        standardEffects.Remove(effect);
    }

    public List<StandardEffects> getStandardEffects()
    {
        return this.standardEffects;
    }

    public void setStandardEffects(List<StandardEffects> list)
    {
        this.standardEffects = list;
    }

    public void removeAllEffects()
    {
        if (hasEffect(StandardEffects.CAGE))
        {
            standardEffects = new List<StandardEffects>();
            standardEffects.Add(StandardEffects.CAGE);
        }
        else
        {
            standardEffects = new List<StandardEffects>();
        }
    }

}
