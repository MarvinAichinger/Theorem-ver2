using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class enemyHand : NetworkBehaviour
{

    //Positions on Hand
    private LinkedList<Position> positions;

    //start position of hand
    [SerializeField]
    private float xStart;

    //end position of hand
    [SerializeField]
    private float xEnd;

    //y-position of hand
    [SerializeField]
    private float y;

    //count of cards in hand
    private int countCards = 0;

    private Vector3 size;

    public LinkedList<Position> GetPositions()
    {
        return this.positions;
    }

    public void Start()
    {
        positions = new LinkedList<Position>();
    }

    //add a card to the hand
    public void AddCard(GameObject card, Vector3 currPosition)
    {

        countCards = positions.Count;
        countCards++;

        if (size.x == 0)
        {
            this.size = card.GetComponent<SpriteRenderer>().bounds.size;
        }

        if ((countCards - 1) * size.x > xEnd - xStart)
        {
            float spaceBetweenCards = (xEnd - xStart) / (countCards + 1);

            repositionCards(spaceBetweenCards, 0);

            Position pos = new Position(new Vector3(xStart + size.x / 2, y, 0), new Vector3(0, 0, 0));
            pos.SetCard(card);
            pos.SetUsed(true);
            positions.AddFirst(pos);
            card.GetComponent<BacksideCard>().moveCardTo(pos.GetCoord(), pos.GetRotation());
        }
        else
        {
            float spaceLeft = ((xEnd - xStart) - countCards * size.x) / 2;

            repositionCards(size.x, spaceLeft);

            Position pos = new Position(new Vector3(spaceLeft + xStart + size.x / 2, y, 0), new Vector3(0, 0, 0));
            pos.SetCard(card);
            pos.SetUsed(true);
            positions.AddFirst(pos);
            card.GetComponent<BacksideCard>().moveCardTo(pos.GetCoord(), pos.GetRotation());
        }
    }

    //remove a card from the hand
    public void RemoveCard(GameObject card)
    {
        foreach (Position pos in positions)
        {
            if (pos.GetCard() == card)
            {
                positions.Remove(pos);
                break;
            }
        }

        //dont touch this - dont know why it works but it does
        if ((positions.Count - 1) * size.x > xEnd - xStart)
        {
            float spaceBetweenCards = (xEnd - xStart) / (positions.Count + 3);
            repositionCards(spaceBetweenCards, 0);
        }
        else
        {
            float spaceLeft = ((xEnd - xStart) - (positions.Count + 2) * size.x) / 2;
            repositionCards(size.x, spaceLeft);
        }
    }

    //repositen the cards in the hand
    private void repositionCards(float distanceCard, float displacement)
    {
        int count = 1;
        foreach (Position temp in positions)
        {
            temp.SetCoord(new Vector3(count * distanceCard + displacement + xStart + size.x / 2, y, 0));
            count++;
            temp.GetCard().GetComponent<BacksideCard>().moveCardTo(temp.GetCoord(), temp.GetRotation());
        }
    }

}
