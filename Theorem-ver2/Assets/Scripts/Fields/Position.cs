using UnityEngine;

public class Position
{

    //is Position already in use
    private bool used;

    //coordinates of the position
    private Vector3 coord;

    //rotation of the position
    private Vector3 rotation;

    //card on the position
    private GameObject card;

    public Position()
    {
    }

    public Position(Vector3 coord, Vector3 rotation)
    {
        this.used = false;
        this.coord = coord;
        this.rotation = rotation;
    }

    public bool GetUsed()
    {
        return this.used;
    }

    public Vector3 GetCoord()
    {
        return this.coord;
    }

    public Vector3 GetRotation()
    {
        return this.rotation;
    }

    public void SetUsed(bool used)
    {
        this.used = used;
    }

    public GameObject GetCard()
    {
        return this.card;
    }

    public void SetCard(GameObject card)
    {
        this.card = card;
    }

    public void SetCoord(Vector3 coord)
    {
        this.coord = coord;
    }

}

