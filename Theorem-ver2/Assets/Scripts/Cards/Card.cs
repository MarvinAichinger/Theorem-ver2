using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using DG.Tweening;
using System.Linq;

public class Card : NetworkBehaviour
{

    //is the card currently draggable
    private bool isDraggable = false;

    //was already in hand
    private bool cardWasInHand = false;

    //is the card able to attack
    public bool canAttack = false;

    //Is the card being dragged
    private bool dragging = false;

    //is the card currently attacking
    private bool attacking = false;

    //Vector between middle of the card and mouse
    private Vector3 pointerDisplacement = Vector3.zero;

    //position where the dragging started
    private Vector3 startPosition = Vector3.zero;

    //is the card currently in the game field
    private bool inGameField = false;

    //gameField where card can be played
    public string gameFieldName;
    private GameObject gameField;

    //hand where the drawn cards are
    public string handName;
    private GameObject hand;

    //manaCost of Card
    public int manaCost;

    //attack of the card
    public int attack;
    public int originalAttack;

    //defense of the card
    public int defense;
    public int originalDefense;

    //is card currently in defense mode
    public bool inDefenseMode = false;

    //did card attack in attack phase
    public bool didCardAttack = false;

    //sprite of the card
    private Sprite sprite;

    //player game Stats
    private PlayerStats playerStats;

    //is the card in hidden defense mode
    public bool inHiddenDefense = false;

    //is mouse currently over the card
    private bool mouseOver = false;

    //List of Tokens
    LinkedList<CardToken> tokens = new LinkedList<CardToken>();

    //List of Standard Effects
    [SerializeField]
    private List<StandardEffects> standardEffects;

    //Is Card Trapped
    private bool trapped = false;

    public void Start()
    {
        hand = GameObject.Find(handName);
        gameField = GameObject.Find(gameFieldName);
        this.sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
        this.playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();

        //save attack and defense values
        this.originalAttack = attack;
        this.originalDefense = defense;

        //insert fight token to tokens
        CardToken fightToken = new CardToken(0, 0, gameObject);
        tokens.AddFirst(fightToken);
    }

    public void Update()
    {
        //when dragging update the position of card to the mouse
        if (dragging)
        {
            Vector3 mousePos = MouseWorldCoord();
            transform.position = new Vector3(mousePos.x - pointerDisplacement.x, mousePos.y - pointerDisplacement.y, transform.position.z);

            if (Input.GetMouseButtonUp(1))
            {
                if (inHiddenDefense)
                {
                    setInHiddenDefense(false);
                }else
                {
                    setInHiddenDefense(true);
                }
            }
        }

        //when attacking draw line
        if(attacking)
        {
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.SetPosition(1, MouseWorldCoord());
        }
    }

    public void OnMouseDown()
    {
        //when card is draggable, then put in in drag state
        if (isDraggable)
        {
            dragging = true;
            transform.DORotate(new Vector3(0, 0, 5), 1f);

            //calculate differnece of card center and mouse
            //startPosition = transform.position;
            startPosition = hand.GetComponent<playerHand>().getPositionOfCard(gameObject).GetCoord();
            pointerDisplacement = MouseWorldCoord() - transform.position;
        }

        if (playerStats.getGameStatus() == "attack" && canAttack && !didCardAttack)
        {
            this.attacking = true;
            LineRenderer lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);

            playerStats.addAttackingCard(gameObject);
        }

    }

    public void OnMouseUp()
    {

        if (playerStats.gameStatus == "play" && inGameField && !dragging && cardWasInHand && !inHiddenDefense && mouseOver)
        {
            if (inDefenseMode)
            {
                setInDefenseMode(false);
            }
            else
            {
                setInDefenseMode(true);
            }
        }

        if (dragging)
        {
            //put card out of drag state
            dragging = false;
            transform.DORotate(new Vector3(0, 0, 0), 1f);


            if (inGameField)
            {
                //if card is inside gamefield than play that card
                bool succes = gameField.GetComponent<playerGameField>().PlayCard(gameObject, gameObject.transform.position);

                if (!succes)
                {
                    //if the card cant be played return it to original position
                    transform.DOMove(startPosition, 1f).SetEase(Ease.InOutQuint);

                    if (inHiddenDefense)
                    {
                        setInHiddenDefense(false);
                    }

                } else
                {
                    //if it can be played than remove it from the hand
                    hand.GetComponent<playerHand>().RemoveCard(gameObject);

                    //set card to not be dragable anymore
                    this.isDraggable = false;
                }
            }
            else
            {
                //return card to original position
                transform.DOMove(startPosition, 1f).SetEase(Ease.InOutQuint);
            }

        }

        if (attacking)
        {
            attacking = false;
            LineRenderer lineRenderer = GetComponent<LineRenderer>();

            StartCoroutine(checkPartner(lineRenderer));
        }

    }

    IEnumerator checkPartner(LineRenderer lineRenderer)
    {
        yield return new WaitForSeconds(0.2f);
        bool succes = playerStats.checkAttackPartner(gameObject);
        if (!succes)
        {
            lineRenderer.positionCount = 0;
        }
    }

    //returns mouse position in world coordinates
    private Vector3 MouseWorldCoord()
    {
        Vector3 screenMousePos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(screenMousePos);
    }

    //moves and rotates the card
    public void moveCardTo(Vector3 position, Vector3 rotation)
    {
        //StartCoroutine(blockDrag());
        transform.DOMove(position, 1f).SetEase(Ease.InOutQuint);
        transform.DORotate(rotation, 1f);
    }

    IEnumerator blockDrag()
    {
        this.isDraggable = false;
        yield return new WaitForSeconds(1f);
        this.isDraggable = true;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        //when collision with game field put card inGameField status
        if (collision.gameObject == gameField)
        {
            inGameField = true;
        }

        //make Card draggable if it is in the hand
        if (collision.gameObject == hand)
        {
            if (!cardWasInHand)
            {
                isDraggable = true;
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        //when collision with game field put card out of inGameField status
        if (collision.gameObject == gameField)
        {
            inGameField = false;
            cardWasInHand = true;
        }
    }

    public Sprite getSprite()
    {
        return this.sprite;
    }

    public void setDraggable(bool value)
    {
        this.isDraggable = value;
    }

    public void drawLineTo(Vector3 pos)
    {
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer.positionCount == 2)
        {
            lineRenderer.SetPosition(1, pos);
        }
    }

    public int getFightValue()
    {
        if (inDefenseMode)
        {
            int value = defense + tokens.First.Value.getDefenseChange();
            return value;
        }
        else
        {
            int value = attack + tokens.First.Value.getAttackChange();
            return value;
        }
    }

    public void takeDamage(int value)
    {
        if (inDefenseMode)
        {
            //defense -= value;
            int currDValue = tokens.First.Value.getDefenseChange();
            int currAValue = tokens.First.Value.getAttackChange();
            tokens.First.Value.setTokenValues(currAValue, currDValue - value);
        }
        else
        {
            //attack -= value;
            int currAValue = tokens.First.Value.getAttackChange();
            int currDValue = tokens.First.Value.getDefenseChange();
            tokens.First.Value.setTokenValues(currAValue - value, currDValue);
        }
    }

    public void resetFightDamage()
    {
        tokens.First.Value.setTokenValues(0, 0);
    }

    public void setDidCardAttack(bool value)
    {
        this.didCardAttack = value;
        if (didCardAttack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }else
        {
            if (!inDefenseMode)
            {
                gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    IEnumerator flipCard()
    {
        if (inHiddenDefense)
        {
            //yield return gameObject.transform.DORotate(new Vector3(0, 180, 0), 1f);
            yield return new WaitForSeconds(0.1f);
            gameObject.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/Cards/C0000");
        }
        else
        {
            //gameObject.transform.DORotate(new Vector3(0, 0, 0), 1f);
            yield return new WaitForSeconds(0.1f);
            gameObject.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    public void setInHiddenDefense(bool value)
    {
        this.inHiddenDefense = value;
        StartCoroutine(flipCard());
    }

    public void setInDefenseMode(bool value)
    {
        inDefenseMode = value;
        canAttack = !value;
        if (!trapped)
        {
            if (inDefenseMode)
            {
                gameObject.transform.Find("stance").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/icons/schild");
                //gameObject.GetComponent<SpriteRenderer>().color = new Color(0.5f, 1f, 1f, 1f);
            }
            else
            {
                gameObject.transform.Find("stance").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/icons/schwert");
                //gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
        }

        PlayerManager playerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        playerManager.toggleDefenseModeCmd(gameObject.transform.position, inDefenseMode);
    }

    public void OnMouseEnter()
    {
        this.mouseOver = true;
    }

    public void OnMouseExit()
    {
        this.mouseOver = false;
    }

    public bool getIsDragging()
    {
        return dragging;
    }

    public bool hasEffect(StandardEffects effect)
    {
        return standardEffects.Contains(effect);
    }

    public void removeEffect(StandardEffects effect)
    {
        standardEffects.Remove(effect);
    }

    public List<StandardEffects> getStandardEffects()
    {
        return this.standardEffects;
    }

    public void setStandardEffects(List<StandardEffects> list)
    {
        this.standardEffects = list;
    }

    public void removeAllEffects()
    {
        if (hasEffect(StandardEffects.CAGE))
        {
            standardEffects = new List<StandardEffects>();
            standardEffects.Add(StandardEffects.CAGE);
        }else
        {
            standardEffects = new List<StandardEffects>();
        }
    }

    public bool getTrapped()
    {
        return this.trapped;
    }

    public void setTrapped(bool value)
    {
        this.trapped = value;
    }
}
