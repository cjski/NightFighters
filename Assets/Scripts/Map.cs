using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Node
{
    public enum Connection
    {
        Empty,
        Wall,
        Light,
        Passage
    }

    public Connection
        r = Connection.Empty,
        l = Connection.Empty,
        u = Connection.Empty,
        d = Connection.Empty;
}
public class Map
{
    private static GameObject wallPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/WallPrefab.prefab", typeof(GameObject));
    private static GameObject lightPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/LightPrefab.prefab", typeof(GameObject));

    private int y, x;
    private float unitSize;
    private Node[,] map;

    public Map(int cols, int rows, float sizeOfUnit)
    {
        y = rows;
        x = cols;
        unitSize = sizeOfUnit;
        map = new Node[cols, rows];
        Generate();
    }

    public void Generate()
    {
        // Initialize all nodes - all will be used later
        for(int i=0;i< x;++i)
        {
            for (int j = 0; j < y; ++j)
            {
                map[i, j] = new Node();
            }
        }

        //Generate lower border walls
        for(int i=0;i < x; ++i)
        {
            map[i, 0].d = Node.Connection.Wall;
            Object.Instantiate(wallPrefab, new Vector2(i+0.5f,0)*unitSize, Quaternion.identity);
        }

        //Generate upper border walls
        for (int i = 0; i < x; ++i)
        {
            map[i, y-1].u = Node.Connection.Wall;
            Object.Instantiate(wallPrefab, new Vector2((i + 0.5f), y)*unitSize, Quaternion.identity);
        }

        //Generate left border walls
        for (int i = 0; i < y; ++i)
        {
            map[0, i].l = Node.Connection.Wall;
            Object.Instantiate(wallPrefab, new Vector2(0, i + 0.5f)*unitSize, Quaternion.Euler(0, 0, 90));
        }

        //Generate right border walls
        for (int i = 0; i < y; ++i)
        {
            map[x-1, i].r = Node.Connection.Wall;
            Object.Instantiate(wallPrefab, new Vector2(x, i + 0.5f)*unitSize, Quaternion.Euler(0, 0, 90));
        }

        // Generate random walls in the map
        int walls = 8;
        int wallX, wallY, wallR;
        int iterations = 0;
        while(walls > 0 && iterations < 10000)
        {
            ++iterations;
            wallX = Random.Range(0, x);
            wallY = Random.Range(0, y);
            wallR = Random.Range(0, 4);

            Node node = map[wallX, wallY], other;
            // We set which of the 4 sides of the node we want here
            // In each check, the empty check is done because if we hit a node on the border, then it's guaranteed
            // to have a wall there. We'll hit that wall and drop out of the if safely, and will therefore never
            // try and set our "other" node to something that isn't in the array, letting us skip a check for that.
            if (wallR == 0 && node.u == Node.Connection.Empty)
            {
                other = map[wallX, wallY + 1];
                if (GetMaxConnections(node, other) < 2)
                {
                    node.u = Node.Connection.Wall;
                    other.d = Node.Connection.Wall;
                    Object.Instantiate(wallPrefab, new Vector2(wallX + 0.5f, wallY + 1) * unitSize, Quaternion.Euler(0, 0, 0));
                    --walls;
                }
            }
            else if (wallR == 1 && node.l == Node.Connection.Empty)
            {
                other = map[wallX - 1, wallY];
                if (GetMaxConnections(node, other) < 2)
                {
                    node.l = Node.Connection.Wall;
                    other.r = Node.Connection.Wall;
                    Object.Instantiate(wallPrefab, new Vector2(wallX, wallY + 0.5f) * unitSize, Quaternion.Euler(0, 0, 90));
                    --walls;
                }
            }
            else if (wallR == 2 && map[wallX, wallY].r == Node.Connection.Empty)
            {
                other = map[wallX + 1, wallY];
                if (GetMaxConnections(node, other) < 2)
                {
                    node.r = Node.Connection.Wall;
                    other.l = Node.Connection.Wall;
                    Object.Instantiate(wallPrefab, new Vector2(wallX + 1, wallY + 0.5f) * unitSize, Quaternion.Euler(0, 0, 90));
                    --walls;
                }
            }
            else if(map[wallX, wallY].d == Node.Connection.Empty)
            {
                other = map[wallX, wallY - 1];
                if (GetMaxConnections(node, other) < 2)
                {
                    node.d = Node.Connection.Wall;
                    other.u = Node.Connection.Wall;
                    Object.Instantiate(wallPrefab, new Vector2(wallX + 0.5f, wallY) * unitSize, Quaternion.Euler(0, 0, 0));
                    --walls;
                }
            }
           
        }

        // Generate random lights in the map
        int lights = 4;
        int lightX, lightY, lightR;
        iterations = 0;
        while (lights > 0 && iterations < 10000)
        {
            ++iterations;
            lightX = Random.Range(0, x);
            lightY = Random.Range(0, y);
            lightR = Random.Range(0, 4);

            Node node = map[lightX, lightY], other;
            // We set which of the 4 sides of the node we want here
            // In each check, the empty check is done for the same reason as the wall one - we'll catch cases
            // of being on the border right away.
            if (lightR == 0 && node.u == Node.Connection.Empty)
            {
                other = map[lightX, lightY + 1];
                node.u = Node.Connection.Light;
                other.d = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX + 0.5f, lightY + 1) * unitSize, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (lightR == 1 && node.l == Node.Connection.Empty)
            {
                other = map[lightX - 1, lightY];
                node.l = Node.Connection.Light;
                other.r = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX, lightY + 0.5f) * unitSize, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (lightR == 2 && node.r == Node.Connection.Empty)
            {
                other = map[lightX + 1, lightY];
                node.r = Node.Connection.Light;
                other.l = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX + 1, lightY + 0.5f) * unitSize, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (node.d == Node.Connection.Empty)
            {
                other = map[lightX, lightY - 1];
                node.d = Node.Connection.Light;
                other.u = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX + 0.5f, lightY) * unitSize, Quaternion.Euler(0, 0, 0));
                --lights;
            }
        }
    }

    private int GetMaxConnections(Node a, Node b)
    {
        // Check if the node has 2 walls connected already. If so, don't attach another wall because this makes a dead end
        int aConnections = 0;
        if (a.u == Node.Connection.Wall) ++aConnections;
        if (a.d == Node.Connection.Wall) ++aConnections;
        if (a.l == Node.Connection.Wall) ++aConnections;
        if (a.r == Node.Connection.Wall) ++aConnections;

        int bConnections = 0;
        if (b.u == Node.Connection.Wall) ++bConnections;
        if (b.d == Node.Connection.Wall) ++bConnections;
        if (b.l == Node.Connection.Wall) ++bConnections;
        if (b.r == Node.Connection.Wall) ++bConnections;

        return Mathf.Max(aConnections, bConnections);
    }
}
