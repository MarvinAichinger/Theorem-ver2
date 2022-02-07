using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ConvertButton : NetworkBehaviour
{
    [SerializeField]
    private Sprite normalSprite;

    [SerializeField]
    private Sprite hoverSprite;

    public int roundOfLastConvert = 0;

    public int valueOfLastConvert = 0;

    public void OnMouseEnter()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = hoverSprite;
    }

    public void OnMouseExit()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = normalSprite;
    }

    public void OnMouseDown()
    {
        PlayerStats playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();
        if (playerStats.roundCounter > roundOfLastConvert + valueOfLastConvert && playerStats.getGameStatus() == "play")
        {
            int value = Mathf.RoundToInt(GameObject.Find("healthToMana").GetComponent<Slider>().value);
            this.roundOfLastConvert = playerStats.roundCounter;
            this.valueOfLastConvert = value;
            playerStats.takeDamage(value, true);
            playerStats.setMana(playerStats.getMana() + value);
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
    }

    public void updateButton()
    {
        PlayerStats playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();
        if (playerStats.roundCounter > roundOfLastConvert + valueOfLastConvert)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
    }
}
