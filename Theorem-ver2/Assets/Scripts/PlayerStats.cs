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

        if (gameStatus == "draw" || gameStatus == "start")
        {
            GameObject.Find("Allert").GetComponent<Allert>().allert("Your Turn", 2f);
        }

        //inkrement round counter and set new mana if going into play mode & enable next button
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

            //Enable Next button
            GameObject.Find("NextButton").GetComponent<NextButton>().setIsDisabled(false);

        }

        if (gameStatus == "wait")
        {
            //Disable next button
            GameObject.Find("NextButton").GetComponent<NextButton>().setIsDisabled(true);
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
            //Reset Cards
            playerGameField gamefield = GameObject.Find("playerGameField").GetComponent<playerGameField>();
            gamefield.resetAttackedCards();
            gamefield.resetCards();

            setGameStatus("play");
        }else if (gameStatus == "play")
        {
            setGameStatus("attack");
        }else if (gameStatus == "attack")
        {
            setGameStatus("wait");
            GameObject.Find("Allert").GetComponent<Allert>().allert("Enemys Turn", 2f);

            /*for (int i = 0; i < attackingCards.Count; i++)
            {
                Debug.Log(attackingCards.ElementAt(i));
                Debug.Log(attackedCards.ElementAt(i));
            }*/

            /*if (attackingCards.Count == attackedCards.Count && (attackedCards.Count > 0 || attackingEnemy.Count > 0))
            {
                StartCoroutine(FightEvaluation(true));
            }else
            {*/

            //reset enemys cards fight values
            GameObject.Find("enemyGameField").GetComponent<enemyGameField>().resetCards();

                //inform server that the player ended his turn
                playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
                playerManager.playerEndedTurnCmd();
                
                
                
            //}

        }
    }

    IEnumerator FightEvaluation(bool isAttacking)
    {
        //Disable next button
        GameObject.Find("NextButton").GetComponent<NextButton>().setIsDisabled(true);

        //Preparing the fight and send enemy the fighting cards (Positions)
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

                //Prevents dragging while animation
                playerCard.GetComponent<Card>().didCardAttack = true;
            }
            for (int i = 0; i < attackingEnemy.Count; i++)
            {
                GameObject playerCard = attackingEnemy.ElementAt(i);
                Vector3 playerCardPos = playerCard.transform.position;
                attackingUserCardsPos[i] = playerCardPos;

                //Prevents dragging while animation 
                playerCard.GetComponent<Card>().didCardAttack = true;
            }

            //inform server about the fighting cards
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.fightingCardsCmd(attackingCardsPositions, attackedCardsPositions, attackingUserCardsPos);

        }

        //Card vs Cards Fights
        for (int i = 0; i < attackingCards.Count; i++)
        {
            //=========Prepare===============
            //Get the two cards fighting
            GameObject playerCard = attackingCards.ElementAt(i);
            GameObject enemyCard = attackedCards.ElementAt(i);

            //Positions of these cards
            Vector3 playerCardStartPos = playerCard.transform.position;
            Vector3 enemyCardStartPos = enemyCard.transform.position;

            //Scripts of these Cards
            Card playerCardScript = playerCard.GetComponent<Card>();
            BacksideCard enemyCardScript = enemyCard.GetComponent<BacksideCard>();

            

            //=============Hidden Defense Stance==============
            //Put card out of hidden defense stance
            bool wasInHiddenDefenseStance = false;
            if (enemyCardScript.inHiddenDefense)
            {
                wasInHiddenDefenseStance = true;
                enemyCardScript.setInHiddenDefense(false);
            }
            yield return new WaitForSeconds(1f);

            //=========== Start Animation==============
            //Remove Line of the drag
            playerCard.GetComponent<LineRenderer>().positionCount = 0;

            //Move the cards in the middle
            playerCardScript.moveCardTo(new Vector3(0, -1, -2), Vector3.zero);
            enemyCardScript.moveCardTo(new Vector3(0, 1, -2));
            yield return new WaitForSeconds(2f);

            //================Fight==============
            Debug.Log(playerCardScript.hasEffect(StandardEffects.CAGE));
            Debug.Log(enemyCardScript.hasEffect(StandardEffects.CAGE));
            if (enemyCardScript.hasEffect(StandardEffects.CAGE) || playerCardScript.hasEffect(StandardEffects.CAGE))
            {
                Debug.Log("herinnen");
                //Defending Card is Cage
                if (!isAttacking)
                {
                    //Trap Card dies - attacking card moves back
                    enemyCardScript.moveCardTo(enemyCardStartPos);

                    playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                    GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                    playerCardScript.setDraggable(false);
                    playerCardScript.canAttack = false;

                }
                else
                {

                    //Trap Card dies - attacking card moves back
                    playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);

                    enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                    GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                    enemyCardScript.inGameField = false;

                    playerCardScript.setInDefenseMode(true);
                    playerCardScript.setTrapped(true);

                }

            }else if (isAttacking && enemyCardScript.hasEffect(StandardEffects.SHIELD) || !isAttacking && playerCardScript.hasEffect(StandardEffects.SHIELD))
            {
                //Defending Card has Shield effect
                if (isAttacking)
                {
                    if (playerCardScript.getFightValue() > enemyCardScript.getFightValue() || playerCardScript.getFightValue() == enemyCardScript.getFightValue())
                    {
                        //attacking card is stronger or euqal strong then no card dies and attacking card gets damage
                        playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                        enemyCardScript.moveCardTo(enemyCardStartPos);

                        playerCardScript.takeDamage(enemyCardScript.getFightValue());
                    }else
                    {
                        //attacking card is weaker - attacking card dies
                        enemyCardScript.moveCardTo(enemyCardStartPos);

                        playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                        GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                        playerCardScript.setDraggable(false);
                        playerCardScript.canAttack = false;

                    }

                    enemyCardScript.removeEffect(StandardEffects.SHIELD);
                }else
                {
                    if (enemyCardScript.getFightValue() > playerCardScript.getFightValue() || enemyCardScript.getFightValue() == playerCardScript.getFightValue())
                    {
                        //attacking card is stronger or euqal strong then no card dies and attacking card gets damage
                        enemyCardScript.moveCardTo(enemyCardStartPos);
                        playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);

                        enemyCardScript.takeDamage(playerCardScript.getFightValue());
                    }else
                    {
                        //attacking card is weaker - attacking card dies
                        playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);

                        enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                        GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                        enemyCardScript.inGameField = false;
                    }

                    playerCardScript.removeEffect(StandardEffects.SHIELD);
                }

            } else if (playerCardScript.getFightValue() == enemyCardScript.getFightValue() && playerCardScript.getFightValue() != 0)
            {
                //Fight Values are equal and not zero
                if (wasInHiddenDefenseStance)
                {
                    //If card was in hidden defense stance then defense card loses
                    playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                    enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                    GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                    enemyCardScript.inGameField = false;

                    //Winning card takes damage
                    playerCardScript.takeDamage(enemyCardScript.getFightValue());

                    if (playerCardScript.hasEffect(StandardEffects.BOUNTY))
                    {
                        takeDamage(-1, true);
                    }

                }
                else
                {
                    //Both cards die
                    playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                    enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                    GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                    playerCardScript.setDraggable(false);
                    playerCardScript.canAttack = false;
                    GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                    enemyCardScript.inGameField = false;

                    if (playerCardScript.hasEffect(StandardEffects.BOUNTY))
                    {
                        takeDamage(-1, true);
                    }

                }
            } else if (playerCardScript.getFightValue() > enemyCardScript.getFightValue())
            {
                //attacking card wins
                //enemy card dies and trampelschaden if not defense mode
                playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                enemyCardScript.moveCardTo(new Vector3(7.5f, 3.75f, -1));
                GameObject.Find("enemyGameField").GetComponent<enemyGameField>().RemoveCard(enemyCard);
                enemyCardScript.inGameField = false;

                if (isAttacking && !enemyCardScript.inDefenseMode || isAttacking && playerCardScript.hasEffect(StandardEffects.PIERCE))
                {
                    int diff = playerCardScript.getFightValue() - enemyCardScript.getFightValue();
                    enemyTakesDamage(diff, true);
                }

                //Winning card takes damage
                playerCardScript.takeDamage(enemyCardScript.getFightValue());

                if (playerCardScript.hasEffect(StandardEffects.BOUNTY)) {
                    takeDamage(-1, true);
                }

            }else if (playerCardScript.getFightValue() < enemyCardScript.getFightValue())
            {
                //defending card wins
                //player card dies and trampelschaden if not defense mode
                enemyCardScript.moveCardTo(enemyCardStartPos);
                playerCardScript.moveCardTo(new Vector3(-7.5f, -3.75f, -1), Vector3.zero);
                GameObject.Find("playerGameField").GetComponent<playerGameField>().RemoveCard(playerCard);
                playerCardScript.setDraggable(false);
                playerCardScript.canAttack = false;

                if (isAttacking && !playerCardScript.inDefenseMode || isAttacking && enemyCardScript.hasEffect(StandardEffects.PIERCE))
                {
                    int diff = enemyCardScript.getFightValue() - playerCardScript.getFightValue();
                    takeDamage(diff, true);
                }

                //Winning card takes damage
                enemyCardScript.takeDamage(playerCardScript.getFightValue());

            }
            else
            {
                //move the cards back to original positions
                playerCardScript.moveCardTo(playerCardStartPos, Vector3.zero);
                enemyCardScript.moveCardTo(enemyCardStartPos);
            }

            //Set the atracking card on did card attack = true
            if (isAttacking)
            {
                playerCardScript.setDidCardAttack(true);
            }
        }
        //Remove all fighting connections
        attackedCards = new LinkedList<GameObject>();
        attackingCards = new LinkedList<GameObject>();


        //Cards Attacking the user
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

        /*if (isAttacking)
        {
            //inform server that the player ended his turn
            playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
            playerManager.playerEndedTurnCmd();
        }*/

        if (isAttacking)
        {
            //Enable next button
            GameObject.Find("NextButton").GetComponent<NextButton>().setIsDisabled(false);
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
        GameObject.Find("Allert").GetComponent<Allert>().allert(health + " - " + enemyHealth, 2f);

        if (this.health <= 0)
        {
            //you lose game
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
        GameObject.Find("Allert").GetComponent<Allert>().allert(health + " - " + enemyHealth, 2f);

        if (this.enemyHealth <= 0)
        {
            //enemy lost game
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
            StartCoroutine(FightEvaluation(true));
            return true;
        }else if (attackedCards.Count == attackingCards.Count)
        {
            enemyGameField egf = GameObject.Find("enemyGameField").GetComponent<enemyGameField>();
            if (egf.hasTauntingCard())
            {
                if (attackedCards.First.Value.gameObject.GetComponent<BacksideCard>().hasEffect(StandardEffects.TAUNTING))
                {
                    card.GetComponent<Card>().drawLineTo(attackedCards.First.Value.transform.position);
                    StartCoroutine(FightEvaluation(true));
                    return true;
                }else
                {
                    this.attackingCards.Remove(card);
                    this.attackedCards.RemoveFirst();
                    return false;
                }
            }
            card.GetComponent<Card>().drawLineTo(attackedCards.First.Value.transform.position);
            StartCoroutine(FightEvaluation(true));
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
