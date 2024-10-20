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
        

        assemblyLine.Clear();

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

        
        Vector3 targetPosition = assemblyLine[0].transform.position;
        Quaternion targetRotation = assemblyLine[0].transform.rotation;

        assemblyLine.RemoveAt(assemblyLine.Count - 1);
        assemblyLine.Insert(0, last);

        if(swipeDirection == Vector2.up)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.max.y);
        }

        else if(swipeDirection == Vector2.down)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.min.y);
            
        }

        else if(swipeDirection == Vector2.left)
        {
            targetPosition = new Vector2(bounds.min.x, assemblyLine[0].transform.position.y);
        }

        else if(swipeDirection == Vector2.right)
        {
            targetPosition = new Vector2(bounds.max.x, assemblyLine[0].transform.position.y);
        }

        while (elapsedTime < moveDuration)
        {
            if (last.transform.position == bounds.max)
            {
                targetPosition = 
            }
            elapsedTime += Time.deltaTime;

            
            last.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            last.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / moveDuration);

            

            yield return null; 
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
