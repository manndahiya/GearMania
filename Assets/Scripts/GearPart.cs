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
    [SerializeField] private float swipeThreshold = 0.5f; // Minimum distance for a swipe

    private GridSetup grid;
    private GridItem gridItem;

    private List<GameObject> assemblyLine = new List<GameObject>();

    public int column;
    public int row;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;

    public float swipeAngle = 0;

    private bool isMoving = false;
    private bool canReceiveInput = true;

 

    
    void Start()
    {
        grid = FindFirstObjectByType<GridSetup>();
        gridItem = GetComponent<GridItem>();
        column = gridItem.col;
        row = gridItem.row;
        assemblyLine.Clear();

    }

    private void PrintGears()
    {
        for (int j = 0; j < assemblyLine.Count; j++)
        {
            Debug.Log(assemblyLine[j]);
        }
    }

    public void MovePieces()
    {
        if (isMoving) return;

        Debug.Log("Trying to move");
        assemblyLine.Clear();
        

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
            
            for (int i = 0; i < grid.height; i++)
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

     

    }

 

    IEnumerator StartMovingPieces(Vector2 swipeDirection)
    {
        if (isMoving) yield break;
        isMoving = true;

        Debug.Log("Started moving pieces");

        float moveDuration = 0.5f;
        float elapsedTime = 0f;
        Renderer renderer = boundary.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        GameObject last = assemblyLine[assemblyLine.Count - 1];
        Vector3 lastStartPosition = last.transform.position;
        Quaternion lastStartRotation = last.transform.rotation;

        Vector2 firstItemPos = assemblyLine[0].transform.position;
        Quaternion firstItemRotation = assemblyLine[0].transform.rotation;

        Vector2 boundaryTarget = GetTargetPosition(swipeDirection, bounds, firstItemPos);

        // Store starting positions and rotations for all gears
        List<Vector3> startPositions = new List<Vector3>();
        List<Quaternion> startRotations = new List<Quaternion>();
        List<Vector3> targetPositions = new List<Vector3>();
        List<Quaternion> targetRotations = new List<Quaternion>();

        // Set up targets: each gear moves to the next gear's position, except the last one
        for (int i = 0; i < assemblyLine.Count - 1; i++)
        {
            startPositions.Add(assemblyLine[i].transform.position);
            startRotations.Add(assemblyLine[i].transform.rotation);
            targetPositions.Add(assemblyLine[i + 1].transform.position);
            targetRotations.Add(assemblyLine[i + 1].transform.rotation);
        }
        // Last gear moves to boundary
        startPositions.Add(lastStartPosition);
        startRotations.Add(lastStartRotation);
        targetPositions.Add(boundaryTarget);
        targetRotations.Add(firstItemRotation); // Will teleport to first position, so use its rotation

        // Animate all gears moving
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveDuration;

            for (int i = 0; i < assemblyLine.Count; i++)
            {
                assemblyLine[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                assemblyLine[i].transform.rotation = Quaternion.Lerp(startRotations[i], targetRotations[i], t);
            }
            yield return null;
        }

        // Snap last gear to first position after reaching boundary
        last.transform.position = firstItemPos;
        last.transform.rotation = firstItemRotation;

        // Update grid and sync
        UpdateGrid(swipeDirection);
        SyncGrid();
        isMoving = false;
    }

    private Vector2 GetTargetPosition(Vector2 swipeDirection, Bounds bounds, Vector2 firstItemPos)
    {
        if (swipeDirection == Vector2.up)
            return new Vector2(firstItemPos.x, bounds.max.y);
        else if (swipeDirection == Vector2.down)
            return new Vector2(firstItemPos.x, bounds.min.y);
        else if (swipeDirection == Vector2.left)
            return new Vector2(bounds.min.x, firstItemPos.y);
        else // right
            return new Vector2(bounds.max.x, firstItemPos.y);
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
        // Temporary array to store new positions
        GameObject[,] tempGrid = new GameObject[grid.width, grid.height];

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
            tempGrid[newCol, newRow] = gearPart;
        }

        // Update the main grid
        for (int x = 0; x < grid.width; x++)
        {
            for (int y = 0; y < grid.height; y++)
            {
                if (tempGrid[x, y] != null)
                    grid.allGearParts[x, y] = tempGrid[x, y];
            }
        }
    }


    private void OnMouseDown()
    {
        if (!canReceiveInput) return; // Ignore input while moving

        firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (!canReceiveInput) return; // Ignore input while moving

        finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CalculateAngle();
        StartCoroutine(BlockInputForDuration(1f));
    }

    void CalculateAngle()
    {
        float swipeDistance = Vector2.Distance(finalTouchPosition, firstTouchPosition);

        if (swipeDistance < swipeThreshold)
        {
            Debug.Log("Less swipe power, Rejected");
            return; // Treat this as a click, not a swipe
        }

        // If the swipe is valid, calculate the angle
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y,
                                 finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;

        MovePieces();
    }

    IEnumerator BlockInputForDuration(float duration)
    {
        canReceiveInput = false;
        yield return new WaitForSeconds(duration);
        canReceiveInput = true;
    }

   
}
