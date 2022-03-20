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

    private Slider slider;
    private Text manaText;
    bool convertPossible = true;

    public void Start()
    {
        manaText = GameObject.Find("manaTimer").GetComponent<Text>();
        slider = GameObject.Find("healthToMana").GetComponent<Slider>();
        slider.onValueChanged.AddListener(delegate { sliderValueChange(); });
    }

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
            int value = Mathf.RoundToInt(slider.value);
            this.roundOfLastConvert = playerStats.roundCounter;
            this.valueOfLastConvert = value;
            playerStats.takeDamage(value, true);
            playerStats.setMana(playerStats.getMana() + value);
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            convertPossible = false;
            manaText.text = "Convert possible in " + (roundOfLastConvert + valueOfLastConvert - playerStats.roundCounter + 1) + " rounds.";
        }
    }

    public void updateButton()
    {
        PlayerStats playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();
        if (playerStats.roundCounter > roundOfLastConvert + valueOfLastConvert)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            convertPossible = true;
            sliderValueChange();
        }
        if (!convertPossible)
        {
            manaText.text = "Convert possible in " + (roundOfLastConvert + valueOfLastConvert - playerStats.roundCounter + 1) + " rounds.";
        }
    }

    public void sliderValueChange()
    {
        PlayerStats playerStats = GameObject.Find("PlayerStats").GetComponent<PlayerStats>();
        if (convertPossible)
        {
            manaText.text = slider.value + "";
        }
    }

}
