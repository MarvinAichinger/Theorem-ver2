using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardPreview : MonoBehaviour
{

    private bool mouseOver = false;

    private GameObject preview;

    Vector3 startPos;

    void Start()
    {
        preview = GameObject.Find("CardPreview");
    }

    void Update()
    {
        if (Input.GetMouseButtonUp(1))
        {
            if (gameObject.GetComponent<Card>() == null)
            {
                if (mouseOver)
                {
                    preview.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
                    preview.transform.DOMoveX(-7.5f, 1f);
                }
            }
            else
            {
                if (mouseOver && !gameObject.GetComponent<Card>().getIsDragging())
                {
                    preview.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<SpriteRenderer>().sprite;
                    preview.transform.DOMoveX(-7.5f, 1f);
                }
            }
        }
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            preview.transform.DOMoveX(-11f, 0.2f);
            preview.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    private void OnMouseEnter()
    {
        mouseOver = true;
        startPos = transform.position;
        transform.position = new Vector3(startPos.x, startPos.y, -1);
        transform.DOScale(new Vector3(0.6f, 0.6f, 0.6f), 0.2f);
    }

    private void OnMouseExit()
    {
        mouseOver = false;
        transform.position = startPos;
        transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.2f);
    }
}
