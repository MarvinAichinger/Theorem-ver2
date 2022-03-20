using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardToken
{

    private int attackChange = 0;
    private int defenseChange = 0;

    GameObject card;

    public CardToken(int a, int d, GameObject card)
    {
        this.card = card;
        setTokenValues(a, d);
    }

    public void setTokenValues(int a, int d)
    {
        //Debug.Log(card.transform.Find("token").gameObject);
        if (a != 0 || d != 0)
        {
            card.transform.Find("token").gameObject.GetComponent<TextMeshPro>().text = a + " / " + d;
        }else
        {
            card.transform.Find("token").gameObject.GetComponent<TextMeshPro>().text = "";
        }
        this.attackChange = a;
        this.defenseChange = d;
    }

    public int getAttackChange()
    {
        return this.attackChange;
    }

    public int getDefenseChange()
    {
        return this.defenseChange;
    }
}
