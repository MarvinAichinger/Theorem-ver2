using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    
    //Client started
    public override void OnStartClient()
    {
        base.OnStartClient();
        playerReadyCmd();
    }

    //A Client Connected to the game and is ready
    [Command]
    public void playerReadyCmd()
    {

        ServerStats server = GameObject.Find("ServerStats").GetComponent<ServerStats>();

        if (!server.isSecondPlayer)
        {
            server.player1 = connectionToClient;
            server.isSecondPlayer = true;
        }else
        {
            server.player2 = connectionToClient;

            int rnd = Random.Range(0, 2);
            if (rnd == 0)
            {

                server.player1.identity.GetComponent<PlayerManager>().playerStartedRpc();
            }else
            {
                server.player2.identity.GetComponent<PlayerManager>().playerStartedRpc();
            }
        }
    }


    //Inform Server that the player started the game
    [Command]
    public void playerStartedCmd()
    {
        playerStartedRpc();
    }
    //Inform Client that the other player started the game
    [ClientRpc]
    public void playerStartedRpc()
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("PlayerStats").GetComponent<PlayerStats>().setGameStatus("wait");
        }
    }

    //Inform Server that a card is drawn
    [Command]
    public void cardDrawCmd()
    {
        cardDrawRpc();
    }
    //Inform Client that enemy drew a card
    [ClientRpc]
    public void cardDrawRpc()
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("enemyDeck").GetComponent<enemyDeck>().drawCard();
        }
    }

    //Inform Server that a card is played
    [Command]
    public void cardPlayedCmd(string cardId, Vector3 position, int attack, int defense, bool inHiddenDefense)
    {
        cardPlayedRpc(cardId, position, attack, defense, inHiddenDefense);
    }
    //Inform Client that enemy played a card
    [ClientRpc]
    public void cardPlayedRpc(string cardId, Vector3 position, int attack, int defense, bool inHiddenDefense)
    {
        if (!isLocalPlayer)
        {
            enemyHand enemyHand = GameObject.Find("enemyHand").GetComponent<enemyHand>();
            GameObject cardInHand = enemyHand.GetPositions().First.Value.GetCard();
            enemyHand.RemoveCard(cardInHand);
            cardInHand.GetComponent<BacksideCard>().attack = attack;
            cardInHand.GetComponent<BacksideCard>().defense = defense;
            GameObject.Find("enemyGameField").GetComponent<enemyGameField>().PlayCard(cardId, cardInHand, position, inHiddenDefense);
        }
    }

    //inform server that the player ended his turn and that the next player can play
    [Command]
    public void playerEndedTurnCmd()
    {
        playerEndedTurnRpc();
    }
    //Inform client that he can start playing now
    [ClientRpc]
    public void playerEndedTurnRpc()
    {
        if (!isLocalPlayer)
        {
            PlayerStats playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();
            if (playerStats.roundCounter == 0)
            {
                playerStats.setGameStatus("start");
            }else
            {
                playerStats.setGameStatus("draw");
            }
        }
    }

    //Inform Server about the fighting Cards
    [Command]
    public void fightingCardsCmd(Vector3[] attackingCardsPos, Vector3[] attackedCardsPos, Vector3[] attackingUserCardsPos)
    {
        fightingCardsRpc(attackingCardsPos, attackedCardsPos, attackingUserCardsPos);
    }
    //Inform Client about the fighting cards
    [ClientRpc]
    public void fightingCardsRpc(Vector3[] attackingCardsPos, Vector3[] attackedCardsPos, Vector3[] attackingUserCardsPos)
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("PlayerStats").GetComponent<PlayerStats>().showCardsFighting(attackingCardsPos, attackedCardsPos, attackingUserCardsPos);
        }
    }

    //Tell Server that other player has to take damage
    [Command]
    public void takeDamageCmd(int diff)
    {
        takeDamageRpc(diff);
    }
    //Tell Client that he has to take damage
    [ClientRpc]
    public void takeDamageRpc(int diff)
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("PlayerStats").GetComponent<PlayerStats>().takeDamage(diff, false);
        }
    }

    //Tell Server that this player takes Damage
    [Command]
    public void enemyTakesDamageCmd(int diff)
    {
        enemyTakesDamageRpc(diff);
    }
    //Tell Client that the other player took damage
    [ClientRpc]
    public void enemyTakesDamageRpc(int diff)
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("PlayerStats").GetComponent<PlayerStats>().enemyTakesDamage(diff, false);
        }
    }

    //Inform Server about the new attackable status
    [Command]
    public void setEnemyAttackableCmd(bool value)
    {
        setEnemyAttackableRpc(value);
    }
    //Tell Client about the enemys new attackable status
    [ClientRpc]
    public void setEnemyAttackableRpc(bool value)
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("enemyUser").GetComponent<EnemyUser>().setAttackable(value);
        }
    }

    //check if the player is attackable
    [Command]
    public void checkIfPlayerIsAttackableCmd()
    {
        checkIfPlayerIsAttackableRpc();
    }
    [ClientRpc]
    public void checkIfPlayerIsAttackableRpc()
    {
        if (!isLocalPlayer)
        {
            GameObject.Find("playerGameField").GetComponent<playerGameField>().checkIfUserIsAttackable();
        }
    }

    //toggle the defense of the card at the position to the value
    [Command]
    public void toggleDefenseModeCmd(Vector3 pos, bool inDefenseMode)
    {
        toggleDefenseModeRpc(pos, inDefenseMode);
    }
    [ClientRpc]
    public void toggleDefenseModeRpc(Vector3 pos, bool inDefenseMode)
    {
        if (!isLocalPlayer)
        {
            enemyGameField gameField = GameObject.Find("enemyGameField").GetComponent<enemyGameField>();
            BacksideCard card = gameField.GetNearestPosition(pos).GetCard().GetComponent<BacksideCard>();
            card.setInDefenseMode(inDefenseMode);
        }
    }
}
