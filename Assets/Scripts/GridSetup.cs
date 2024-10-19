using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridSetup : MonoBehaviour
{

    [SerializeField] private Transform Boundary;
    public GameObject tile;
    GameObject Tile;
    public GameObject[] gearComponents;

    Vector2 boundaryBottomLeftPos;
    Vector3 rotation;


    private int[,] gridArray;
    public GameObject[,] allGearParts;


    public int width;
    public int height;
    private float tileWidth;
    private float tileHeight;
    private int rows = 6, cols = 8;
    
    

    private void Start()
    {
        boundaryBottomLeftPos = Boundary.GetComponent<Renderer>().bounds.min;
        gridArray = new int[width, height];
        allGearParts = new GameObject[width, height];
        SetUp();
        

    }
    public Vector3 GetWorldPosition(int col, int row)
    {
        // Assuming your grid's center is at (0, 0), adjust if it's offset
        float x = col * 1f;
        float y = row * 1f;
        return new Vector3(x, y, 0); // Z position is usually 0 for 2D games
    }

    private void AdjustPositionAndScale()
    {
        
        Vector3 boundarySize = Boundary.GetComponent<Renderer>().bounds.size;

     
        tileWidth = boundarySize.x / rows;
        tileHeight = boundarySize.y / cols;
    }

    private void SetUp()
    {
        AdjustPositionAndScale();

        boundaryBottomLeftPos = Boundary.GetComponent<Renderer>().bounds.min;
        float spacingOffset = 0.1f;  

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Vector2 tempPosition = boundaryBottomLeftPos + new Vector2(x * tileWidth, y * tileHeight);
                tempPosition += new Vector2(tileWidth / 2, tileHeight / 2); // Center the tile in its grid space

                // offset to make the gears appear visually closer
                tempPosition.x += (x % 2 == 0) ? spacingOffset : -spacingOffset;
                tempPosition.y += (y % 2 == 0) ? spacingOffset : -spacingOffset;

                int selected = Random.Range(0, gearComponents.Length);
                Tile = Instantiate(gearComponents[selected], tempPosition, Quaternion.identity);

               
                if (x % 2 == 0 && y % 2 == 0)
                {
                    // Top-left of the group (no rotation)
                    rotation = new Vector3(0, 0, 90);
                }
                else if (x % 2 == 0 && y % 2 == 1)
                {
                    // Top-right of the group (90 degrees)
                    rotation = new Vector3(0, 0, 0);
                }
                else if (x % 2 == 1 && y % 2 == 0)
                {
                    // Bottom-left of the group (270 degrees)
                    rotation = new Vector3(0, 0, 180);
                }
                else if (x % 2 == 1 && y % 2 == 1)
                {
                    // Bottom-right of the group (180 degrees)
                    rotation = new Vector3(0, 0, 270);
                }
                
  

                Tile.transform.rotation = Quaternion.Euler(rotation);
                Tile.transform.localScale = new Vector2(tileWidth, tileHeight);
                Tile.transform.parent = this.transform;
                Tile.name = "(" + x + ", " + y + ")";
                allGearParts[x,y] = Tile;
               
            }
        }
    }

}
