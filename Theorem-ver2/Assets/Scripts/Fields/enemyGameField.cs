using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;
using System.Linq;

public class enemyGameField : NetworkBehaviour
{

    //position in enemy Game Field
    LinkedList<Position> positions = new LinkedList<Position>();

    //enemy played a card
    public void PlayCard(string cardId, GameObject cardInHand, Vector3 position, bool inHiddenDefense)
    {

        //Delete Taunting Effects if card has taunting
        if (hasTauntingCard() && cardInHand.GetComponent<BacksideCard>().hasEffect(StandardEffects.TAUNTING))
        {
            foreach (Position pos in positions) {
                if (pos.GetUsed())
                {
                    if (pos.GetCard().GetComponent<BacksideCard>().hasEffect(StandardEffects.TAUNTING))
                    {
                        pos.GetCard().GetComponent<BacksideCard>().removeEffect(StandardEffects.TAUNTING);
                    }
                }
            }
        }

        //create and set position
        Position newPosition = new Position(new Vector3(position.x, gameObject.transform.position.y, position.z), Vector3.zero);
        newPosition.SetCard(cardInHand);
        newPosition.SetUsed(true);
        positions.AddFirst(newPosition);

        //move card and change the sprite
        BacksideCard cardScript = cardInHand.GetComponent<BacksideCard>();
        cardScript.moveCardTo(newPosition.GetCoord(), new Vector3(180, 0, 0));
        cardScript.inGameField = true;
        Sprite sprite = Resources.Load<Sprite>("Images/Cards/" + cardId.Remove(5));
        cardScript.sprite = sprite;
        cardScript.setInDefenseMode(false);
        if (inHiddenDefense)
        {
            cardScript.setInHiddenDefense(inHiddenDefense);
            cardScript.setInDefenseMode(true);
            cardScript.removeAllEffects();
            Sprite backside = Resources.Load<Sprite>("Images/Cards/C0000");
            StartCoroutine(WaitCoroutine(backside, cardInHand));
        }else
        {
            StartCoroutine(WaitCoroutine(sprite, cardInHand));
        }
    }

    //wait 0.5 sec and change sprite
    IEnumerator WaitCoroutine(Sprite sprite, GameObject cardInHand)
    {
        yield return new WaitForSeconds(0.45f);
        yield return cardInHand.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    //remove Card from GameField
    public void RemoveCard(GameObject card)
    {
        foreach (Position pos in positions)
        {
            if (pos.GetCard() == card)
            {
                pos.SetUsed(false);
                return;
            }
        }
    }

    //Get the position nearest to the vector3 param
    public Position GetNearestPosition(Vector3 currPosition)
    {
        Position nearestPosition = null;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < positions.Count; i++)
        {
            Position positionAtIndex = positions.ElementAt(i);
            float distance = Vector3.Distance(positionAtIndex.GetCoord(), currPosition);
            if (distance < minDistance)
            {
                nearestPosition = positionAtIndex;
                minDistance = distance;
            }
        }
        return nearestPosition;
    }

    public void resetAttackedCards()
    {
        foreach (Position pos in positions)
        {
            if (pos.GetUsed())
            {
                pos.GetCard().GetComponent<BacksideCard>().setDidCardAttack(false);
            }
        }
    }

    public void resetCards()
    {
        foreach (Position pos in positions)
        {
            if (pos.GetUsed())
            {
                pos.GetCard().GetComponent<BacksideCard>().resetFightDamage();
                //c.attack = c.originalAttack;
                //c.defense = c.originalDefense;
            }
        }
    }

    public bool hasTauntingCard()
    {
        foreach (Position pos in positions)
        {
            if (pos.GetUsed())
            {
                if (pos.GetCard().GetComponent<BacksideCard>().hasEffect(StandardEffects.TAUNTING))
                {
                    return true;
                }
            }
        }
        return false;
    }

}
