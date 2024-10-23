using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GearPart : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private GameObject boundary;

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
            StartCoroutine(StartMovingPieces(Vector2.right));
        }

        // Up swipe
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < grid.height - 1)
        {
            for (int i = 0; i < grid.height - 1; i++)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);

            }
            StartCoroutine(StartMovingPieces(Vector2.up));
        }

        // left swipe
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            for (int i = grid.width - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[i, this.gameObject.GetComponent<GridItem>().row]);


            }
            StartCoroutine(StartMovingPieces(Vector2.left));
        }

        // Down swipe
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            for (int i = grid.height - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);

            }
            StartCoroutine(StartMovingPieces(Vector2.down));

        }

        PrintGears();

       // assemblyLine.Clear();

    }

    private void PrintGears()
    {
        for (int j = 0; j < assemblyLine.Count; j++)
        {
            Debug.Log(assemblyLine[j]);
        }
    }

    IEnumerator StartMovingPieces(Vector2 swipeDirection)
    {
       

        // Time taken to move one piece to the next position
        float moveDuration = 1f;
        float elapsedTime = 0f;
        Renderer renderer = boundary.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        GameObject last = assemblyLine[assemblyLine.Count - 1];


        Vector3 startPosition = last.transform.position;
        Quaternion startRotation = last.transform.rotation;


        Vector2 targetPosition = Vector2.zero ;
        Quaternion targetRotation = assemblyLine[0].transform.rotation;

        

        if (swipeDirection == Vector2.up)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.min.y);
        }

        else if (swipeDirection == Vector2.down)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.max.y);

        }

        else if (swipeDirection == Vector2.left)
        {
            targetPosition = new Vector2(bounds.min.x, assemblyLine[0].transform.position.y);
        }

        else if (swipeDirection == Vector2.right)
        {
            targetPosition = new Vector2(bounds.max.x, assemblyLine[0].transform.position.y);
        }

        assemblyLine.RemoveAt(assemblyLine.Count - 1);
        assemblyLine.Insert(0, last);

        while (elapsedTime < moveDuration)
        {


            elapsedTime += Time.deltaTime;


            last.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            last.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / moveDuration);
            SetNewPositionLastElement(bounds, last);

            if(Vector2.Distance(last.transform.position ,new Vector2(assemblyLine[1].transform.position.x, assemblyLine[1].transform.position.y)) < 1f)
            {
                elapsedTime = 0f;
                startPosition = last.transform.position;
                targetPosition = assemblyLine[1].transform.position;
                targetRotation = assemblyLine[1].transform.rotation;
               
            }

          
            yield return null;
        }

        while(elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime;
            
            last.transform.position = Vector2.Lerp(startPosition, targetPosition, 1f);
            last.transform.rotation = Quaternion.Lerp(startRotation, assemblyLine[0].transform.rotation, 1f);
            last.transform.rotation = assemblyLine[0].transform.rotation;

            yield return null;
        }


    }

    private static void SetNewPositionLastElement(Bounds bounds, GameObject last)
    {
        if (last.transform.position.x == bounds.max.x)
        {
            
            last.transform.position = new Vector2(bounds.min.x, last.transform.position.y);
            
        }

        else if (last.transform.position.x == bounds.min.x)
        {
            last.transform.position = new Vector2(bounds.max.x, last.transform.position.y);
        }

        else if (last.transform.position.y == bounds.max.y)
        {
            last.transform.position = new Vector2(bounds.min.y, last.transform.position.y);
        }

        else if (last.transform.position.x == bounds.min.y)
        {
            last.transform.position = new Vector2(bounds.max.y, last.transform.position.y);
        }
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
