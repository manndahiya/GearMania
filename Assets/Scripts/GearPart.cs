using System.Collections;
using System.Collections.Generic;
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

       // PrintGears();

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
        float moveDuration = 0.5f;
        float elapsedTime = 0f;
        Renderer renderer = boundary.GetComponent<Renderer>();
        Bounds bounds = renderer.bounds;

        GameObject last = assemblyLine[assemblyLine.Count - 1];


        Vector3 startPosition = last.transform.position;
        Quaternion startRotation = last.transform.rotation;


        Vector2 targetPosition = Vector2.zero;
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

           
            if (Vector2.Distance(last.transform.position, new Vector2(assemblyLine[0].transform.position.x, assemblyLine[0].transform.position.y)) < 1f)
            {
                //elapsedTime = 0f;
                startPosition = last.transform.position;
                startRotation = last.transform.rotation;
                targetPosition = assemblyLine[0].transform.position;
                targetRotation = assemblyLine[0].transform.rotation;

            }
            for (int y = 0; y < assemblyLine.Count - 1; y++)
            {
                assemblyLine[y].transform.position = Vector2.Lerp(startPositions[y], targetPositions[y], elapsedTime / moveDuration);
                assemblyLine[y].transform.rotation = Quaternion.Lerp(startRotations[y], targetRotations[y], elapsedTime / moveDuration);
          
            }


            yield return null;
        }

 

        //second loop to make it go towards another item's pos
        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
         
            last.transform.position = Vector2.Lerp(startPosition, targetPosition, 1f);
            last.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, 1f);
            last.transform.rotation = targetRotation;

            yield return null;
        }

        Debug.Log($"Target Position: {targetPosition}");
        Debug.Log($"First Element Position: {assemblyLine[0].transform.position}");

        RotateList(last);
    }

    private void RotateList(GameObject last)
    {
        for (int i = assemblyLine.Count - 1; i > 0; i--)
        {

            assemblyLine[i] = assemblyLine[i - 1];

        }

        assemblyLine.RemoveAt(assemblyLine.Count - 1);
        assemblyLine.Insert(0, last);
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

    private static float Mod(float a , float b)
    {
        return a - b * Mathf.Floor(a / b);
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
