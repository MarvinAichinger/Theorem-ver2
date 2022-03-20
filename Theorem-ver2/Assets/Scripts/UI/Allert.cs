using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class Allert : MonoBehaviour
{

    Color bgHide = new Color(0f, 0f, 0f, 0f);
    Color bgVisible = new Color(0f, 0f, 0f, 0.5f);
    Color textHide = new Color(1f, 1f, 1f, 0f);
    Color textVisible = new Color(1f, 1f, 1f, 1f);

    public void allert(string text, float seconds)
    {
        StartCoroutine(allertAnimation(text, seconds));
    }

    private IEnumerator allertAnimation(string text, float seconds)
    {
        TextMeshPro tmp = transform.Find("text").GetComponent<TextMeshPro>();
        tmp.text = text;
        tmp.color = textVisible;
        gameObject.GetComponent<SpriteRenderer>().color = bgVisible;
        yield return new WaitForSeconds(seconds);
        tmp.color = textHide;
        gameObject.GetComponent<SpriteRenderer>().color = bgHide;
    }
}
