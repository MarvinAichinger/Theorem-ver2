using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class PlayerStats : NetworkBehaviour
{
    //current status of the game
    public string gameStatus = "start";

    //text where game status is displayed
    GameObject gameStatusText;

    //player Manager
    PlayerManager playerManager;

    //attacking cards
    LinkedList<GameObject> attackingCards;

    //attacked cards
    LinkedList<GameObject> attackedCards;

    //Cards attacking Enemy directly
    LinkedList<GameObject> attackingEnemy;

    private void Start()
    {
        this.roundCounter = 0;
        this.gameStatusText = GameObject.Find("gameStatus");

        this.attackedCards = new LinkedList<GameObject>();
        this.attackingCards = new LinkedList<GameObject>();
        this.attackingEnemy = new LinkedList<GameObject>();
    }

    public void setGameStatus (string gameStatus)
    {
        if (gameStatus != "start" && gameStatus != "play" && gameStatus != "wait" && gameStatus != "draw" && gameStatus != "attack")
        {
            Debug.Log("Tried to set game status which doesnt exist!");
        }

        //inkrement round counter and set new mana if going into play mode
        if (gameStatus == "play")
        {
            this.roundCounter = this.roundCounter + 1;
            if (this.roundCounter > 7)
            {
                setMana(7);
            }else
            {
                setMana(this.roundCounter);
            }

            GameObject.Find("ConvertButton").GetComponent<ConvertButton>().updateButton();

        }

        GameObject.Find("playerGameField").GetComponent<playerGameField>().checkIfUserIsAttackable();
        playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        playerManager.checkIfPlayerIsAttackableCmd();

        this.gameStatus = gameStatus;
        gameStatusText.GetComponent<Text>().text = gameStatus;
    }

    public string getGameStatus()
    {
        return this.gameStatus;
    }

    public void nextGameStatus()
    {
        //put player into next game state
        if (gameStatus == "draw")
        {
            setGameStatus("play");
        }else if (gameStatus == "play")
        {
            setGameStatus("attack");
        }else if (gameStatus == "attack")
        {
            setGameStatus("wait");


            /*for (int i = 0; i < attackingCards.Count; i++)
            {
                Debug.Log(attackingCards.ElementAt(i));
                Debug.Log(attackedCards.ElementAt(i));
            }*/

            if (attackingCards.Count == attackedCards.Count && (attackedCards.Count > 0 || attackingEnemy.Count > 0))
            {
                StartCoroutine(FightEvaluation(true));
            }else
            {
                //inform server that the player ended his turn
                playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
                playerManager.playerEndedTurnCmd();
            }

        }
    }

    IEnumerator FightEvaluation(bool isAttacking)
    {

        if (isAttacking)
        {
            Vector3[] attackingCardsPositions = new Vector3[attackingCards.Count];
            Vector3[] attackedCardsPositions = new Vector3[attackedCards.Count];
            Vector3[] attackingUserCardsPos = new Vector3[attackingEnemy.Count];
            for (int i = 0; i < attackingCards.Count; i++)
            {
                GameObject playerCard = attackingCards.ElementAt(i);
                GameObject enemyCard = attackedCards.ElementAt(i);

                Vector3 playerCardPos = playerCard.transform.position;
                Vector3 enemyCardPos = enemyCard.transform.position;

                attackingCardsPositions[i] = playerCardPos;
                attackedCardsPositions[i] = enemyCardPos;
            }
            for (int i = 0; i < attackingEnemy.Count; i++)
            {
                GameObject playerCard = attackingEnemy.ElementAt(i);
                Vector3 playerCardPos = playerCard.transform.position;
                attackingUserCardsPos[i] = playerCardPos;
            }

            //inform server about the fighting cards
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.fightingCardsCmd(attackingCardsPositions, attackedCardsPositions, attackingUserCardsPos);

        }

        for (int i = 0; i < attackingCards.Count; i++)
        {
            GameObject playerCard = attackingCards.ElementAt(i);
            GameObject enemyCard = attackedCards.ElementAt(i);

            Vector3 playerCardStartPos = playerCard.transform.position;
            Vector3 enemyCardStartPos = enemyCard.transform.position;

            Card playerCardScript = playerCard.GetComponent<Card>();
            BacksideCard enemyCardScript = enemyCard.GetComponent<BacksideCard>();
            if (enemyCardScript.inHiddenDefense)
            {
                enemyCardScript.setInHiddenDefense(false);
            }
            yield return new WaitForSeconds(1f);
            playerCard.GetComponent<LineRenderer>().positionCount = 0;
            playerCardScript.moveCardTo(new Vector3(0, -1, -2), Vector3.zero);
            enemyCardScript.moveCardTo(new Vector3(0, 1, -2));
            yield return new WaitForSeconds(2f);
            if (playerCardScript.getFightValue() == enemyCardScript.getFightValue() && playerCardScript.getFightValue() != 0)
            {
                playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                playerCardScript.setDraggable(false);
                playerCardScript.canAttack = false;
                GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                enemyCardScript.inGameField = false;
            } else if (playerCardScript.getFightValue() > enemyCardScript.getFightValue())
            {
                playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                enemyCardScript.inGameField = false;

                if (isAttacking && !enemyCardScript.inDefenseMode)
                {
                    int diff = playerCardScript.getFightValue() - enemyCardScript.getFightValue();
                    enemyTakesDamage(diff, true);
                }

            }else if (playerCardScript.getFightValue() < enemyCardScript.getFightValue())
            {
                enemyCardScript.moveCardTo(enemyCardStartPos);
                playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                playerCardScript.setDraggable(false);
                playerCardScript.canAttack = false;

                if (isAttacking && !playerCardScript.inDefenseMode)
                {
                    int diff = enemyCardScript.getFightValue() - playerCardScript.getFightValue();
                    takeDamage(diff, true);
                }
            }
            else
            {
                playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                enemyCardScript.moveCardTo(enemyCardStartPos);
            }

            if (isAttacking)
            {
                playerCardScript.setDidCardAttack(true);
            }
        }
        attackedCards = new LinkedList<GameObject>();
        attackingCards = new LinkedList<GameObject>();


        if (isAttacking)
        {
            foreach (GameObject card in attackingEnemy)
            {
                yield return new WaitForSeconds(1f);
                Card cardScript = card.GetComponent<Card>();
                card.GetComponent<LineRenderer>().positionCount = 0;
                Vector3 startPos = card.transform.position;
                GameObject enemyUser = GameObject.Find("enemyUser");
                card.transform.DOMove(new Vector3(enemyUser.transform.position.x, enemyUser.transform.position.y - 0.7f, startPos.z), 0.3f);
                yield return new WaitForSeconds(0.3f);
                enemyTakesDamage(cardScript.getFightValue(), true);
                cardScript.moveCardTo(startPos, Vector3.zero);

                cardScript.setDidCardAttack(true);
            }
        } else
        {
            foreach (GameObject card in attackingEnemy)
            {
                yield return new WaitForSeconds(1f);
                BacksideCard cardScript = card.GetComponent<BacksideCard>();
                Vector3 startPos = card.transform.position;
                GameObject user = GameObject.Find("playerUser");
                card.transform.DOMove(new Vector3(user.transform.position.x, user.transform.position.y + 0.7f, startPos.z), 0.3f);
                yield return new WaitForSeconds(0.3f);
                cardScript.moveCardTo(startPos);
            }
        }
        attackingEnemy = new LinkedList<GameObject>();

        if (isAttacking)
        {
            //inform server that the player ended his turn
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.playerEndedTurnCmd();
        }
    }

    public void showCardsFighting(Vector3[] attackingCardsPos, Vector3[] attackedCardsPos, Vector3[] attackingUserCardsPos)
    {
        for (int i = 0; i < attackingCardsPos.Length; i++)
        {
            Vector3 attackingCardPos = new Vector3 (attackingCardsPos[i].x, attackingCardsPos[i].y * (-1), attackingCardsPos[i].z);
            Vector3 attackedCardPos = new Vector3(attackedCardsPos[i].x, attackedCardsPos[i].y * (-1), attackedCardsPos[i].z);
            GameObject backsideCard = GameObject.Find("enemyGameField").GetComponent<enemyGameField>().GetNearestPosition(attackingCardPos).GetCard();
            GameObject card = GameObject.Find("playerGameField").GetComponent<playerGameField>().GetNearestPosition(attackedCardPos).GetCard();
            attackingCards.AddLast(card);
            attackedCards.AddLast(backsideCard);

            backsideCard.GetComponent<BacksideCard>().setDidCardAttack(true);

            if (card.GetComponent<Card>().inHiddenDefense)
            {
                card.GetComponent<Card>().setInHiddenDefense(false);
            }
        }

        for (int i = 0; i < attackingUserCardsPos.Length; i++)
        {
            Vector3 attackingCardPos = new Vector3(attackingUserCardsPos[i].x, attackingUserCardsPos[i].y * (-1), attackingUserCardsPos[i].z);
            GameObject backsideCard = GameObject.Find("enemyGameField").GetComponent<enemyGameField>().GetNearestPosition(attackingCardPos).GetCard();
            attackingEnemy.AddLast(backsideCard);

            backsideCard.GetComponent<BacksideCard>().setDidCardAttack(true);
        }

        StartCoroutine(FightEvaluation(false));
    }

    //health of the player
    public int health = 20;

    //health of the enemy
    public int enemyHealth = 20;

    //mana of the player
    public int mana = 0;

    //player takes damage
    public void takeDamage(int value, bool shouldInformServer)
    {
        this.health = this.health - value;
        GameObject.Find("health").GetComponent<Text>().text = health > 9 ? health + "" : "0" + health;
        if (shouldInformServer)
        {
            playerManager.enemyTakesDamageCmd(value);
        }

        StartCoroutine(showDamage(GameObject.Find("playerHand").GetComponent<SpriteRenderer>()));
        StartCoroutine(showDamage(GameObject.Find("playerGameField").GetComponent<SpriteRenderer>()));

        if (this.health <= 0)
        {
            //TODO lose game
            Debug.Log("Player lost the game!");
            SceneManager.LoadScene("LostScreen");
        }

    }

    public void enemyTakesDamage(int value, bool shouldInformServer)
    {
        this.enemyHealth = this.enemyHealth - value;
        GameObject.Find("enemyHealth").GetComponent<Text>().text = enemyHealth > 9 ? enemyHealth + "" : "0" + enemyHealth;
        if (shouldInformServer)
        {
            playerManager.takeDamageCmd(value);
        }

        StartCoroutine(showDamage(GameObject.Find("enemyHand").GetComponent<SpriteRenderer>()));
        StartCoroutine(showDamage(GameObject.Find("enemyGameField").GetComponent<SpriteRenderer>()));

        if (this.enemyHealth <= 0)
        {
            //TODO enemy lost game
            Debug.Log("Enemy lost the game!");
            SceneManager.LoadScene("WonScreen");
        }
    }

    IEnumerator showDamage(SpriteRenderer renderer)
    {
        Color temp = renderer.color;
        renderer.color = new Color(0.5f, 0.1f, 0f, 1f);
        yield return new WaitForSeconds(0.2f);
        renderer.color = temp;
    }

    public int getMana() 
    { 
        return this.mana; 
    }
    //set mana and change the text of the ui text element
    public void setMana(int mana) 
    {
        this.mana = mana;
        GameObject.Find("mana").GetComponent<Text>().text = mana > 9 ? mana + "" : "0" + mana;
    }

    //current round of the player
    public int roundCounter = 0;

    public void addAttackingCard(GameObject card)
    {
        if (attackedCards.Count == attackedCards.Count)
        {
            if (attackingEnemy.Contains(card))
            {
                attackingEnemy.Remove(card);
            }
            if (attackingCards.Contains(card))
            {
                int index = attackingCards.ToList<GameObject>().IndexOf(card);
                attackingCards.Remove(card);
                attackedCards.Remove(attackedCards.ElementAt(index));
            }
            this.attackingCards.AddFirst(card);
        }
    }

    public void addAttackedCard(GameObject card)
    {
        if (attackingCards.Count - 1 == attackedCards.Count)
        {
            if (!attackedCards.Contains(card))
            {
                this.attackedCards.AddFirst(card);
            }
        }
    }

    public bool checkAttackPartner(GameObject card)
    {
        if (attackingEnemy.Contains(card))
        {
            card.GetComponent<Card>().drawLineTo(GameObject.Find("enemyUser").transform.position);
            return true;
        }else if (attackedCards.Count == attackingCards.Count)
        {
            card.GetComponent<Card>().drawLineTo(attackedCards.First.Value.transform.position);
            return true;
        }else
        {
            this.attackingCards.Remove(card);
            return false;
        }
    }

    public void cardIsAttackingUser()
    {
        if (attackingCards.Count - 1 == attackedCards.Count)
        {
            GameObject card = attackingCards.First.Value;
            attackingCards.RemoveFirst();
            attackingEnemy.AddFirst(card);
        }
    }

}
