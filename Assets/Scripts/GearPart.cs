using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
using NUnit.Framework;
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

    private bool isMoving = false;

    List<GameObject> assemblyLine = new List<GameObject>();

    
    void Start()
    {
        grid = FindFirstObjectByType<GridSetup>();
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
            assemblyLine.Clear();
            for (int i = 0; i < grid.width; i++)
            {
                assemblyLine.Add(grid.allGearParts[i, this.gameObject.GetComponent<GridItem>().row]);

            }
            StartCoroutine(StartMovingPieces(Vector2.right));
        }

        // Up swipe
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < grid.height - 1)
        {
            assemblyLine.Clear();
            for (int i = 0; i < grid.height; i++)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);

            }
            StartCoroutine(StartMovingPieces(Vector2.up));
        }

        // left swipe
        else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            assemblyLine.Clear();
            for (int i = grid.width - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[i, this.gameObject.GetComponent<GridItem>().row]);


            }
            StartCoroutine(StartMovingPieces(Vector2.left));
        }

        // Down swipe
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            assemblyLine.Clear();
            for (int i = grid.height - 1; i >= 0; i--)
            {
                assemblyLine.Add(grid.allGearParts[this.gameObject.GetComponent<GridItem>().col, i]);
                

            }
            StartCoroutine(StartMovingPieces(Vector2.down));

        }

     

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
       isMoving = true;

        // Time taken to move one piece to the next position
        float moveDuration = 0.5f;
        float elapsedTime = 0f;
        Renderer renderer = boundary.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        GameObject last = assemblyLine[assemblyLine.Count - 1];


        Vector3 startPosition = last.transform.position;
        Quaternion startRotation = last.transform.rotation;


        Vector2 targetPosition = Vector2.zero;
        Vector2 firstItemPos = assemblyLine[0].transform.position; 
        Quaternion targetRotation = assemblyLine[0].transform.rotation;

        List<Vector2> startPositions = new List<Vector2>();
        List<Quaternion> startRotations = new List<Quaternion>();

        List<Vector2> targetPositions = new List<Vector2>();
        List<Quaternion> targetRotations = new List<Quaternion>();

        if (swipeDirection == Vector2.up)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.max.y);
        }

        else if (swipeDirection == Vector2.down)
        {
            targetPosition = new Vector2(assemblyLine[0].transform.position.x, bounds.min.y);

        }

        else if (swipeDirection == Vector2.left)
        {
            targetPosition = new Vector2(bounds.min.x, assemblyLine[0].transform.position.y);
        }

        else if (swipeDirection == Vector2.right)
        {
            targetPosition = new Vector2(bounds.max.x, assemblyLine[0].transform.position.y);
        }

        for (int x = 0; x < assemblyLine.Count - 1; x++)
        {
            startPositions.Add(assemblyLine[x].transform.position);
            startRotations.Add(assemblyLine[x].transform.rotation);

            targetPositions.Add(assemblyLine[x + 1].transform.position);
            targetRotations.Add(assemblyLine[x + 1].transform.rotation);

            if (x == assemblyLine.Count - 2)
            {
                targetPositions.Add(last.transform.position);
                targetRotations.Add(last.transform.rotation);
            }

        }

       
        //initial while loop to make gear move to end of screen
        while (elapsedTime < moveDuration)
        {

            
            elapsedTime += Time.deltaTime;


            last.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / moveDuration);
            last.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / moveDuration);
            SetNewPositionLastElement(bounds, last);

            if (Vector2.Distance(last.transform.position, new Vector2(firstItemPos.x, firstItemPos.y)) < 1f)
            {
                
                last.transform.position = Vector2.Lerp(startPosition, firstItemPos, elapsedTime / moveDuration);
                last.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / moveDuration);

            }

        
          
            for (int y = 0; y < assemblyLine.Count - 1; y++)
            {
                assemblyLine[y].transform.position = Vector2.Lerp(startPositions[y], targetPositions[y], elapsedTime / moveDuration);
                assemblyLine[y].transform.rotation = Quaternion.Lerp(startRotations[y], targetRotations[y], elapsedTime / moveDuration);
          
            }



            yield return null;
        }


        //Update new gear positions to original list
        UpdateGrid(swipeDirection);
        SyncGrid();
        isMoving = false;

    }

    void SyncGrid()
    {
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (grid.allGearParts[x, y] != null)
                {
                    var gridItem = grid.allGearParts[x, y].GetComponent<GridItem>();
                    gridItem.col = x;
                    gridItem.row = y;
                    grid.allGearParts[x, y].name = $"({x}, {y})"; // Update name
                }
            }
        }
    }

    private void UpdateGrid(Vector2 swipeDirection)
    {
        
        foreach (GameObject gearPart in assemblyLine)
        {
            GridItem gridItem = gearPart.GetComponent<GridItem>();

            // Current position
            int currentCol = gridItem.col;
            int currentRow = gridItem.row;

            int newCol = currentCol;
            int newRow = currentRow;

            if (swipeDirection == Vector2.right)
            {
                newCol = (currentCol + 1) % grid.width; // Wrap around horizontally
            }
            else if (swipeDirection == Vector2.left)
            {
                newCol = (currentCol - 1 + grid.width) % grid.width; // Wrap around horizontally
            }
            else if (swipeDirection == Vector2.up)
            {
                newRow = (currentRow + 1) % grid.height; // Wrap around vertically
            }
            else if (swipeDirection == Vector2.down)
            {
                newRow = (currentRow - 1 + grid.height) % grid.height; // Wrap around vertically
            }


            gridItem.UpdateGridPosition(newCol, newRow);
            grid.allGearParts[gridItem.col, gridItem.row] = gearPart;
        }
    }



    private static void SetNewPositionLastElement(Bounds bounds, GameObject last)
    {
        
       
        Vector3 position = last.transform.position;

        // Wrap X and Y axes
        position.x = WrapAxis(position.x, bounds.min.x, bounds.max.x);
        position.y = WrapAxis(position.y, bounds.min.y, bounds.max.y);

        
        last.transform.position = position;
    }

    private static float WrapAxis(float value, float min, float max)
    {
        if (value == max) return min;
        if (value == min) return max;
        return value;
    }


    private void OnMouseDown()
    {
        if (isMoving) return; // Ignore input while moving

        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (isMoving) return; // Ignore input while moving

        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
    }

    void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
        
        MovePieces();
    }
}
