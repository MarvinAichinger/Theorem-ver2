using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using Unity.Mathematics;

public class playerDeck : NetworkBehaviour
{

    public GameObject player;
    public PlayerManager playerManager;
    public List<GameObject> cards;

    [SerializeField]
    private List<GameObject> deck;

    public void Start()
    {
        var rand = new System.Random();
        deck = deck.OrderBy(x => rand.Next()).ToList();
    }

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
            }

            playerStats.setGameStatus("play");

        }else if (playerStats.gameStatus == "draw")
        {
            //draw one card
            drawCard();
            playerStats.nextGameStatus();

            //GameObject.Find("playerGameField").GetComponent<playerGameField>().resetAttackedCards();
        }
    }

    //draw Card
    public void drawCard()
    {
        /*int index = Random.Range(0, cards.Count);
        GameObject card = cards.ToArray()[index];*/
        if (deck.Count >= 1)
        {
            GameObject card = deck.First();
            deck.RemoveAt(0);
            GameObject newCard = Instantiate(card, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, card.transform.position.z), Quaternion.identity);
            GameObject.Find("playerHand").GetComponent<playerHand>().AddCard(newCard, newCard.transform.position);

            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.cardDrawCmd();
        }
    }
}
