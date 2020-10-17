using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class CellularGenerator : MonoBehaviour
{
    //Tiles
    public Tile groundTile;
    public Tile wallTile;
    public Tile edgeTile;
    public Tile leftEdgeTile;
    public Tile rightEdgeTile;
    public Tile leftCornerTile;
    public Tile rightCornerTile;
    public Tile leftPointTile;
    public Tile rightPointTile;
    public Tile leftWallTile;
    public Tile rightWallTile;
    //Tilemaps
    public Tilemap groundMap;
    public Tilemap outerWallMap;
    public Tilemap innerWallMap;
    public Tilemap edgeMap;
    // Variables
    private bool[,] map;
    public int width;
    public int height;
    public double setChance;
    public int deathLimit;
    public int birthLimit;
    public int simulationSteps;
    public int fillSteps;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
        EmptyEdges();
        FillTiles();
    }

    void GenerateMap()
    {
        map = new bool[width,height];
        RandomiseCells(new Random());
        for (int i = 0; i < simulationSteps; i++)
        {
            map = DoSimulationStep(map);
        }
        for (int i = 0; i < fillSteps; i++)
        {
            SmoothEdges();
        }
    }

    void RandomiseCells(Random random)
    {
        for(int x=0; x<width; x++){
            for(int y=0; y<height; y++)
            {
                double chance = random.NextDouble();
                if(chance < setChance)
                {
                    map[x,y] = true;
                }
                else
                {
                    Vector3Int pos = new Vector3Int(x,y,0);
                    groundMap.SetTile(pos,groundTile);
                }
            }
        }
    }
    
    int CountAliveNeighbours(bool[,] map, int x, int y)
    {
        int count = 0;
        for(int i=-1; i<2; i++)
        {
            for(int j=-1; j<2; j++)
            {
                int neighbourX = x+i;
                int neighbourY = y+j;
                
                if(i == 0 && j == 0)
                {
                    ;
                }
                else if(neighbourX < 0 || neighbourY < 0 || neighbourX >= map.GetLength(0) || neighbourY >= map.GetLength(1)) 
                {
                    count = count + 1;
                }
                else if(map[neighbourX, neighbourY])
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    public bool[,] DoSimulationStep(bool[,] oldMap)
    {
        bool[,] newMap = new bool[width,height];
        
        //Loop over each row and column of the map
        for(int x=0; x<oldMap.GetLength(0); x++)
        {
            for(int y=0; y<oldMap.GetLength(1); y++)
            {
                
                int neighbours = CountAliveNeighbours(oldMap,x,y);
                
                //If a cell is alive but has too few neighbours, kill it.
                if(oldMap[x,y])
                {
                    if(neighbours < deathLimit)
                    {
                        newMap[x,y] = false;
                        Vector3Int pos = new Vector3Int(x,y,0);
                        groundMap.SetTile(pos,groundTile);
                    }
                    else
                    {
                        newMap[x,y] = true;
                        Vector3Int pos = new Vector3Int(x,y,0);
                        groundMap.SetTile(pos,null);
                    }
                } 
                //Otherwise, if the cell is dead, check if it has the right number of neighbours to be 'born'
                else
                {
                    if(neighbours > birthLimit)
                    {
                        newMap[x,y] = true;
                        Vector3Int pos = new Vector3Int(x,y,0);
                        groundMap.SetTile(pos,null);
                    }
                    else
                    {
                        newMap[x,y] = false;
                        Vector3Int pos = new Vector3Int(x,y,0);
                        groundMap.SetTile(pos,groundTile);
                    }
                }
            }
        }
        return newMap;
    }

    void SmoothEdges()
    {
        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                try
                {
                    if (!map[x + 1, y] && !map[x - 1, y])
                    {
                        map[x, y] = false;
                        groundMap.SetTile(pos, groundTile);
                    }
                    else if (!map[x, y + 1] && !map[x, y - 1])
                    {
                        map[x, y] = false;
                        groundMap.SetTile(pos, groundTile);
                    }
                    else if (!map[x + 1, y + 1] && !map[x - 1, y - 1])
                    {
                        map[x, y] = false;
                        groundMap.SetTile(pos, groundTile);
                    }
                    else if (!map[x + 1, y - 1] && !map[x - 1, y + 1])
                    {
                        map[x, y] = false;
                        groundMap.SetTile(pos, groundTile);
                    }
                } catch { Exception exception; }
            }
        }
    }
    
    private void EmptyEdges()
    {
        for (int x = 0; x < width; x++)
        {
            map[x, 0] = true;
            map[x, map.GetLength(1)-1] = true;
            groundMap.SetTile(new Vector3Int(x,0,0),null);
            groundMap.SetTile(new Vector3Int(x,map.GetLength(1)-1,0),null);
        }
        for (int y = 0; y < height; y++)
        {
            map[0, y] = true;
            map[map.GetLength(0)-1, y] = true;
            groundMap.SetTile(new Vector3Int(0,y,0),null);
            groundMap.SetTile(new Vector3Int(map.GetLength(0)-1,y,0),null);
        }
    }

    private void FillTiles()
    {
        for(int x=0; x<width; x++)
        {
            for(int y=0; y<height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Vector3Int posLower = new Vector3Int(x, y - 1, 0);
                Vector3Int posUpper = new Vector3Int(x, y + 1, 0);

                if (groundMap.GetTile(pos) == null)
                {
                    //If it is a top wall
                    if (groundMap.GetTile(posLower) != null)
                    {
                        //Fill in a wall and edge
                        outerWallMap.SetTile(pos,wallTile);
                        edgeMap.SetTile(posUpper,edgeTile);
                        
                    }
                    //If it is a bottom wall
                    else if (groundMap.GetTile(posUpper) != null)
                    {
                        //Fill in a wall and edge
                        outerWallMap.SetTile(pos,wallTile);
                        edgeMap.SetTile(posUpper,edgeTile);
                    }
                }
            }
        }
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                Vector3Int posLower = new Vector3Int(x, y - 1, 0);
                Vector3Int posUpper = new Vector3Int(x, y + 1, 0);
                Vector3Int posLeft = new Vector3Int(x - 1, y, 0);
                Vector3Int posRight = new Vector3Int(x + 1, y, 0);
                Vector3Int posUpperLeft = new Vector3Int(x - 1, y + 1, 0);
                Vector3Int posUpperRight = new Vector3Int(x + 1, y + 1, 0);
                Vector3Int posLowerLeft = new Vector3Int(x - 1, y - 1, 0);
                Vector3Int posLowerRight = new Vector3Int(x + 1, y - 1, 0);

                if (groundMap.GetTile(pos) != null)
                {
                    //It it is a left edge
                    if (groundMap.GetTile(posLeft) == null && outerWallMap.GetTile(posLeft) == null)
                    {
                        //If it is a top left corner
                        if (outerWallMap.GetTile(posUpper) != null)
                        {
                            //Fill in a top left corner
                            edgeMap.SetTile(pos,leftEdgeTile);
                            edgeMap.SetTile(posUpper,leftEdgeTile);
                        }
                        //If it is a bottom left corner
                        else if (outerWallMap.GetTile(posLower) != null)
                        {
                            //Fill in a bottom left corner
                            edgeMap.SetTile(pos,leftCornerTile);
                        }
                        else
                        {
                            //Fill in a left edge
                            edgeMap.SetTile(pos,leftEdgeTile);
                        }
                    }
                    
                    //If it is a right edge
                    if (groundMap.GetTile(posRight) == null && outerWallMap.GetTile(posRight) == null)
                    {
                        //If it is a top right corner
                        if (outerWallMap.GetTile(posUpper) != null)
                        {
                            //Fill in a top right corner
                            edgeMap.SetTile(pos,rightEdgeTile);
                            edgeMap.SetTile(posUpper,rightEdgeTile);
                        }
                        //If it is a bottom right corner
                        else if (outerWallMap.GetTile(posLower) != null)
                        {
                            //Fill in a bottom right corner
                            edgeMap.SetTile(pos,rightCornerTile);
                        }
                        else
                        {
                            //Fill in a right edge
                            edgeMap.SetTile(pos,rightEdgeTile);
                        }
                    }
                    
                    //If a left edge appears by a wall
                    if (outerWallMap.GetTile(posLeft) != null && groundMap.GetTile(posRight) != null && groundMap.GetTile(posUpper) != null && groundMap.GetTile(posLowerLeft) == null)
                        edgeMap.SetTile(pos,leftEdgeTile);
                    //If a right edge appears by a wall
                    if (outerWallMap.GetTile(posRight) != null && groundMap.GetTile(posLeft) != null && groundMap.GetTile(posUpper) != null && groundMap.GetTile(posLowerRight) == null)
                        edgeMap.SetTile(pos,rightEdgeTile);
                    
                    //If two walls appear diagonally
                    if (outerWallMap.GetTile(posLeft) != null && outerWallMap.GetTile(posUpper) != null)
                    {
                        edgeMap.SetTile(posUpperLeft, rightCornerTile);
                        edgeMap.SetTile(new Vector3Int(x - 1, y + 2, 0),leftPointTile);
                    }

                    if (outerWallMap.GetTile(posRight) != null && outerWallMap.GetTile(posUpper) != null)
                    {
                        edgeMap.SetTile(posUpperRight, leftCornerTile);
                        edgeMap.SetTile(new Vector3Int(x + 1, y + 2, 0),rightPointTile);
                    }

                    if (outerWallMap.GetTile(posLeft) != null && outerWallMap.GetTile(posLower) != null)
                    {
                        edgeMap.SetTile(pos,leftCornerTile);
                        edgeMap.SetTile(posUpper,rightPointTile);
                    }
                    if (outerWallMap.GetTile(posRight) != null && outerWallMap.GetTile(posLower) != null)
                    {
                        edgeMap.SetTile(pos,rightCornerTile);
                        edgeMap.SetTile(posUpper,leftPointTile);
                    }
                }

                if (outerWallMap.GetTile(pos) != null)
                {
                    //If a wall comes to a point
                    if (groundMap.GetTile(posLeft) != null && groundMap.GetTile(posUpper) != null)
                        edgeMap.SetTile(posUpperLeft,leftPointTile);
                    if (groundMap.GetTile(posRight) != null && groundMap.GetTile(posUpper) != null)
                        edgeMap.SetTile(posUpperRight,rightPointTile);
                    
                    //If a wall extends with the edge
                    if (groundMap.GetTile(posLeft) != null && edgeMap.GetTile(posLeft) == null && edgeMap.GetTile(posUpper) != null && outerWallMap.GetTile(posUpperLeft) == null)
                    {
                        innerWallMap.SetTile(posLeft,leftWallTile);
                    }
                    if (groundMap.GetTile(posRight) != null && edgeMap.GetTile(posRight) == null && edgeMap.GetTile(posUpper) != null && outerWallMap.GetTile(posUpperRight) == null)
                    {
                        innerWallMap.SetTile(posRight,rightWallTile);
                    }
                }
            }
        }
    }

}
