using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class playerDeck : NetworkBehaviour
{

    public GameObject player;
    public PlayerManager playerManager;
    public List<GameObject> cards;

    public void OnMouseUp()
    {
        var playerStats = player.GetComponent<PlayerStats>();
        if (playerStats.gameStatus == "start")
        {
            //Draw 5 Cards to Start with and inform server
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.playerStartedCmd();

            for (int i = 0; i < 5; i++)
            {
                drawCard();
                playerManager.cardDrawCmd();
            }

            playerStats.setGameStatus("play");

        }else if (playerStats.gameStatus == "draw")
        {
            //draw one card
            drawCard();
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.cardDrawCmd();
            playerStats.nextGameStatus();

            GameObject.Find("playerGameField").GetComponent<playerGameField>().resetAttackedCards();
        }
    }

    //draw Card
    public void drawCard()
    {
        int index = Random.Range(0, cards.Count);
        GameObject card = cards.ToArray()[index];
        GameObject newCard = Instantiate(card, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, card.transform.position.z), Quaternion.identity);
        GameObject.Find("playerHand").GetComponent<playerHand>().AddCard(newCard, newCard.transform.position);
    }
}
