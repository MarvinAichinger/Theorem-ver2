using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class playerGameField : NetworkBehaviour
{

    //player for player Stats
    private PlayerStats playerStats;

    //Positions on Game Field
    private LinkedList<Position> positions;

    //x-values of all positions
    [SerializeField]
    private float[] xValues;

    //y-values of all positions
    [SerializeField]
    private float[] yValues;

    //rotations of all positions
    [SerializeField]
    private float[] rotations;

    public void Start()
    {
        positions = new LinkedList<Position>();

        this.playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();

        if (xValues == null || yValues == null || rotations == null || yValues.Length != xValues.Length || xValues.Length != rotations.Length || xValues.Length < 1)
        {
            //default positions
            positions.AddFirst(new Position(new Vector3(4, gameObject.transform.position.y, 0), new Vector3(0, 0, 0)));
            positions.AddFirst(new Position(new Vector3(2, gameObject.transform.position.y, 0), new Vector3(0, 0, 0)));
            positions.AddFirst(new Position(new Vector3(0, gameObject.transform.position.y, 0), new Vector3(0, 0, 0)));
            positions.AddFirst(new Position(new Vector3(-2, gameObject.transform.position.y, 0), new Vector3(0, 0, 0)));
            positions.AddFirst(new Position(new Vector3(-4, gameObject.transform.position.y, 0), new Vector3(0, 0, 0)));
        }

        //positions from serialized field
        for (int i = 0; i < xValues.Length; i++)
        {
            positions.AddFirst(new Position(new Vector3(xValues[i], yValues[i], 0), new Vector3(0, 0, rotations[i])));
        }

    }

    //find the nearest free position of the gameField
    private Position GetNearestFreePosition(Vector3 currPosition)
    {
        Position nearestPosition = null;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < positions.Count; i++)
        {
            Position positionAtIndex = positions.ElementAt(i);
            float distance = Vector3.Distance(positionAtIndex.GetCoord(), currPosition);
            if (distance < minDistance && !positionAtIndex.GetUsed())
            {
                nearestPosition = positionAtIndex;
                minDistance = distance;
            }
        }

        return nearestPosition;
    }

    public bool PlayCard(GameObject card, Vector3 currPosition)
    {
        //get nearest free position if none is free than no card can be played
        Position position = GetNearestFreePosition(currPosition);
        if (position == null)
        {
            return false;
        }

        //check if player is in play game status
        if (playerStats.getGameStatus() != "play")
        {
            return false;
        }

        //check if player has enough mana
        if (card.GetComponent<Card>().manaCost > playerStats.getMana())
        {
            return false;
        }else
        {
            //subtract the manacosts of the card from the players mana
            playerStats.setMana(playerStats.getMana() - card.GetComponent<Card>().manaCost);
        }

        position.SetCard(card);
        position.SetUsed(true);

        Card cardScript = card.GetComponent<Card>();

        PlayerManager player = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        //TODO inform server about played card (strength, ...)
        player.cardPlayedCmd(card.gameObject.name, position.GetCoord(), cardScript.attack, cardScript.defense, cardScript.inHiddenDefense);

        if (cardScript.inHiddenDefense)
        {
            cardScript.setInDefenseMode(true);
            cardScript.canAttack = false;
        }else
        {
            cardScript.canAttack = true;
        }

        cardScript.moveCardTo(position.GetCoord(), position.GetRotation());

        checkIfUserIsAttackable();

        return true;
    }

    //remove Card from GameField
    public void RemoveCard(GameObject card)
    {
        foreach (Position pos in positions)
        {
            if (pos.GetCard() == card)
            {
                pos.SetUsed(false);
                checkIfUserIsAttackable();
                return;
            }
        }
    }

    //Check if the user is Attackable and if yes than put the user in attackable state
    public void checkIfUserIsAttackable()
    {
        PlayerUser playerUser = GameObject.Find("playerUser").GetComponent<PlayerUser>();
        int posEmptyCount = 0;
        foreach (Position pos in positions)
        {
            if (!pos.GetUsed() || pos.GetCard().GetComponent<Card>().didCardAttack || !pos.GetCard().GetComponent<Card>().inDefenseMode)
            {
                posEmptyCount++;
            }
        }

        if (posEmptyCount == positions.Count)
        {
            playerUser.setAttackable(true);
        }else
        {
            playerUser.setAttackable(false);
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
                pos.GetCard().GetComponent<Card>().setDidCardAttack(false);
            }
        }
    }

}
