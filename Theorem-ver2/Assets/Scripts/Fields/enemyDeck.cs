using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class enemyDeck : NetworkBehaviour
{

    public GameObject card;

    //draw Card
    public void drawCard()
    {
        GameObject newCard = Instantiate(card, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, card.transform.position.z), Quaternion.identity);
        GameObject.Find("enemyHand").GetComponent<enemyHand>().AddCard(newCard, newCard.transform.position);

        GameObject.Find("enemyGameField").GetComponent<enemyGameField>().resetAttackedCards();
    }

}
