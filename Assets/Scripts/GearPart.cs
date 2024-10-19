using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GearPart : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;

    private GridSetup grid;
    private GridItem gridItem;

    private GameObject otherGearPart;

    public int column;
    public int row;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    public float swipeAngle = 0;
    


    List<GameObject> assemblyLine = new List<GameObject>();

    void Start()
    {
        grid = FindObjectOfType<GridSetup>();
        gridItem = GetComponent<GridItem>();
        column = gridItem.col;
        row = gridItem.row;
        assemblyLine.Clear();
    }


    public void MovePieces()
    {
        // right swipe
        if (swipeAngle > -45 && swipeAngle <= 45 && column < grid.width - 1)
        {
            for (int i = 0; i < grid.width; i++)
            {
                assemblyLine.Add(grid.allGearParts[i, this.gameObject.GetComponent<GridItem>().row]);

            }
        }

        // Up swipe
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < grid.height - 1)
        {
            for (int i = 0; i < grid.height - 1; i++)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);

            }
        }

        // left swipe
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            for (int i = grid.width - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[i, this.gameObject.GetComponent<GridItem>().row]);
                

            }
        }

        // Down swipe
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            for (int i = grid.height - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);
               
            }

        }


        StartMoving();
        assemblyLine.Clear();
    }


    IEnumerator StartMovingPieces()
    {
        yield return new Null();
    }

    void StartMoving()
    {
        GameObject last = assemblyLine[assemblyLine.Count - 1];

        Vector3 lastPos = assemblyLine[assemblyLine.Count - 1].transform.position;
        Quaternion lastRot = assemblyLine[assemblyLine.Count - 1].transform.rotation;

        // Shift all elements one step forward 
        for (int i = 0; i < assemblyLine.Count - 1; i++)
        {
            assemblyLine[i].transform.position = assemblyLine[i + 1].transform.position;
            assemblyLine[i].transform.rotation = assemblyLine[i + 1].transform.rotation;
        }

        // Move the last element to the first element's position
        assemblyLine[0].transform.position = lastPos;
        assemblyLine[0].transform.rotation = lastRot;

        assemblyLine.RemoveAt(assemblyLine.Count - 1);
        assemblyLine.Insert(0, last);
    }





    private void OnMouseDown()
    {
        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       

    }

    private void OnMouseUp()
    {
        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       
        CalculateAngle();
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        
        MovePieces();
    }
}
