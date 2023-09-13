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

    public int x, y;
    public int[,] distances;

    public Node(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
    }

    public void InitDistanceMap(int xMax, int yMax)
    {
        distances = new int[xMax, yMax];
        for(int i = 0; i< xMax; ++i)
        {
            for (int j = 0; j < yMax; ++j)
            {
                distances[i, j] = xMax * yMax;
            }
        }
    }
}

public class Map
{
    private static GameObject[] horizontalWallPrefabs =
    {
        (GameObject)Resources.Load<GameObject>("Prefabs/WallHorizontalPrefab0"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallHorizontalPrefab1"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallHorizontalPrefab2"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallHorizontalPrefab3"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallHorizontalPrefab4")
    };

    private static GameObject[] verticalWallPrefabs =
    {
        (GameObject)Resources.Load<GameObject>("Prefabs/WallVerticalPrefab0"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallVerticalPrefab1"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallVerticalPrefab2"),
        (GameObject)Resources.Load<GameObject>("Prefabs/WallVerticalPrefab3")
    };
    private static Quaternion[] horizontalRotations =
    {
        Quaternion.Euler(0, 0, 0),
    };
    private static Quaternion[] verticalRotations =
    {
        Quaternion.Euler(0, 0, 0),
    };
    private static GameObject lightPrefab = (GameObject)Resources.Load<GameObject>("Prefabs/LightPrefab");

    public int y, x;
    public float unitSize;
    public float unitSizeInverse;
    public Vector2 offset;
    private Node[,] map;

    public Map(int cols, int rows, float sizeOfUnit, Vector2 newOffset)
    {
        y = rows;
        x = cols;
        unitSize = sizeOfUnit;
        unitSizeInverse = 1 / unitSize;
        offset = newOffset;
        map = new Node[cols, rows];
    }

    public Node GetNode(int xIndex, int yIndex)
    {
        return map[xIndex, yIndex];
    }

    public Node GetNode(Vector2 position)
    {
        position = (position - offset) * unitSizeInverse;
        return map[(int)position.x, (int)position.y];
    }

    public Vector2 GetRealNodePosition(int xPos, int yPos)
    {
        return new Vector2(xPos + 0.5f, yPos + 0.5f)*unitSize + offset;
    }

    public void Generate()
    {
        // Initialize all nodes - all will be used later
        for(int i=0;i< x;++i)
        {
            for (int j = 0; j < y; ++j)
            {
                map[i, j] = new Node(i, j);
            }
        }

        RegenerateWalls(8);
        while (!EntirelyConnected())
        {
            // Reset the Node Map so nothing is connected
            for (int i = 0; i < x; ++i)
            {
                for (int j = 0; j < y; ++j)
                {
                    map[i, j] = new Node(i, j);
                }
            }
            RegenerateWalls(8);
        }

        RegenerateLights(4);

        GenerateDistances();
    }

    public void ClearAll()
    {
        GameObject[] gameWalls = GameObject.FindGameObjectsWithTag("Wall");
        for (int i = 0; i < gameWalls.Length; ++i)
        {
            Object.Destroy(gameWalls[i]);
        }

        GameObject[] gameLights = GameObject.FindGameObjectsWithTag("Light");
        for (int i = 0; i < gameLights.Length; ++i)
        {
            Object.Destroy(gameLights[i]);
        }
    }

    private void RegenerateWalls(int walls)
    {
        GameObject[] gameWalls = GameObject.FindGameObjectsWithTag("Wall");
        for (int i = 0; i < gameWalls.Length; ++i)
        {
            Object.Destroy(gameWalls[i]);
        }

        //Generate lower border walls
        for (int i = 0; i < x; ++i)
        {
            map[i, 0].d = Node.Connection.Wall;
            GameObject wall = Object.Instantiate(horizontalWallPrefabs[Random.Range(0, horizontalWallPrefabs.Length)], new Vector2(i + 0.5f, 0) * unitSize + offset, horizontalRotations[Random.Range(0, horizontalRotations.Length)]);
            wall.GetComponent<SpriteRenderer>().sortingOrder = 0;
        }

        //Generate upper border walls
        for (int i = 0; i < x; ++i)
        {
            map[i, y - 1].u = Node.Connection.Wall;
            GameObject wall = Object.Instantiate(horizontalWallPrefabs[Random.Range(0, horizontalWallPrefabs.Length)], new Vector2((i + 0.5f), y) * unitSize + offset, horizontalRotations[Random.Range(0, horizontalRotations.Length)]);
            wall.GetComponent<SpriteRenderer>().sortingOrder = -2 * y; // - 2 * (y-1 + 1)
        }

        //Generate left border walls
        for (int i = 0; i < y; ++i)
        {
            map[0, i].l = Node.Connection.Wall;
            GameObject wall = Object.Instantiate(verticalWallPrefabs[Random.Range(0, verticalWallPrefabs.Length)], new Vector2(0, i + 0.5f) * unitSize + offset, verticalRotations[Random.Range(0, verticalRotations.Length)]);
            wall.GetComponent<SpriteRenderer>().sortingOrder = -2*i - 1;
        }

        //Generate right border walls
        for (int i = 0; i < y; ++i)
        {
            map[x - 1, i].r = Node.Connection.Wall;
            GameObject wall = Object.Instantiate(verticalWallPrefabs[Random.Range(0, verticalWallPrefabs.Length)], new Vector2(x, i + 0.5f) * unitSize + offset, verticalRotations[Random.Range(0, verticalRotations.Length)]);
            wall.GetComponent<SpriteRenderer>().sortingOrder = -2*i - 1;
        }

        // Generate random walls in the map
        int wallX, wallY, wallR;
        int iterations = 0;
        while (walls > 0 && iterations < 10000)
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
                    GameObject wall = Object.Instantiate(horizontalWallPrefabs[Random.Range(0, horizontalWallPrefabs.Length)], new Vector2(wallX + 0.5f, wallY + 1) * unitSize + offset, horizontalRotations[Random.Range(0, horizontalRotations.Length)]);
                    wall.GetComponent<SpriteRenderer>().sortingOrder = -2 * (wallY + 1);
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
                    GameObject wall = Object.Instantiate(verticalWallPrefabs[Random.Range(0, verticalWallPrefabs.Length)], new Vector2(wallX, wallY + 0.5f) * unitSize + offset, verticalRotations[Random.Range(0, verticalRotations.Length)]);
                    wall.GetComponent<SpriteRenderer>().sortingOrder = -(2 * wallY + 1);
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
                    GameObject wall = Object.Instantiate(verticalWallPrefabs[Random.Range(0, verticalWallPrefabs.Length)], new Vector2(wallX + 1, wallY + 0.5f) * unitSize + offset, verticalRotations[Random.Range(0, verticalRotations.Length)]);
                    wall.GetComponent<SpriteRenderer>().sortingOrder = -(2 * wallY + 1);
                    --walls;
                }
            }
            else if (map[wallX, wallY].d == Node.Connection.Empty)
            {
                other = map[wallX, wallY - 1];
                if (GetMaxConnections(node, other) < 2)
                {
                    node.d = Node.Connection.Wall;
                    other.u = Node.Connection.Wall;
                    GameObject wall = Object.Instantiate(horizontalWallPrefabs[Random.Range(0, horizontalWallPrefabs.Length)], new Vector2(wallX + 0.5f, wallY) * unitSize + offset, horizontalRotations[Random.Range(0, horizontalRotations.Length)]);
                    wall.GetComponent<SpriteRenderer>().sortingOrder = -2*wallY;
                    --walls;
                }
            }
        }
    }

    private void RegenerateLights(int lights)
    {
        GameObject[] gameLights = GameObject.FindGameObjectsWithTag("Light");
        for (int i = 0; i < gameLights.Length; ++i)
        {
            Object.Destroy(gameLights[i]);
        }

        // Generate random lights in the map
        int lightX, lightY, lightR;
        int iterations = 0;
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
                Object.Instantiate(lightPrefab, new Vector2(lightX + 0.5f, lightY + 1) * unitSize + offset, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (lightR == 1 && node.l == Node.Connection.Empty)
            {
                other = map[lightX - 1, lightY];
                node.l = Node.Connection.Light;
                other.r = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX, lightY + 0.5f) * unitSize + offset, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (lightR == 2 && node.r == Node.Connection.Empty)
            {
                other = map[lightX + 1, lightY];
                node.r = Node.Connection.Light;
                other.l = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX + 1, lightY + 0.5f) * unitSize + offset, Quaternion.Euler(0, 0, 0));
                --lights;
            }
            else if (node.d == Node.Connection.Empty)
            {
                other = map[lightX, lightY - 1];
                node.d = Node.Connection.Light;
                other.u = Node.Connection.Light;
                Object.Instantiate(lightPrefab, new Vector2(lightX + 0.5f, lightY) * unitSize + offset, Quaternion.Euler(0, 0, 0));
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

    // Check if every tile on the map can reach every other tile
    private bool EntirelyConnected()
    {
        bool[,] visited = new bool[x, y];
        int numVisited = 0;
        Stack<Node> toVisit = new Stack<Node>();
        Node node;
        toVisit.Push(map[0, 0]);
        visited[0, 0] = true;
        
        // Check each node and use the visited array to keep track of who's in the stack or has been in the stack
        // Eventually we'll hit every possible node and then we can check if we visited all of them with numVisited
        while(toVisit.Count > 0)
        {
            node = toVisit.Pop();
            ++numVisited;
            //Checking for a wall first means we won't ever check out of bounds - We'll run into a wall first
            if (node.u != Node.Connection.Wall && !visited[node.x, node.y + 1])
            {
                toVisit.Push(map[node.x, node.y + 1]);
                visited[node.x, node.y + 1] = true;
            }
            if (node.l != Node.Connection.Wall && !visited[node.x - 1, node.y])
            {
                toVisit.Push(map[node.x - 1, node.y]);
                visited[node.x - 1, node.y] = true;
            }
            if (node.r != Node.Connection.Wall && !visited[node.x + 1, node.y])
            {
                toVisit.Push(map[node.x + 1, node.y]);
                visited[node.x + 1, node.y] = true;
            }
            if (node.d != Node.Connection.Wall && !visited[node.x, node.y - 1])
            {
                toVisit.Push(map[node.x, node.y - 1]);
                visited[node.x, node.y - 1] = true;
            }
        }
        // If everything is connected, then we should have gotten x*y nodes visited
        return numVisited == x * y;
    }

    private void GenerateDistances()
    {
        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {
                map[i, j].InitDistanceMap(x, y);
            }
        }

        for (int i = 0; i < x; ++i)
        {
            for (int j = 0; j < y; ++j)
            {
                CalculateDistances(map[i, j]);
            }
        }
    }

    private void CalculateDistances(Node origin)
    {
        Queue<Node> toVisit = new Queue<Node>();
        toVisit.Enqueue(origin);
        origin.distances[origin.x, origin.y] = 0;
        Node node;

        while (toVisit.Count > 0)
        {
            node = toVisit.Dequeue();
            //Checking for a wall first means we won't ever check out of bounds - We'll run into a wall first
            if (node.u != Node.Connection.Wall && origin.distances[node.x, node.y] + 1 <= origin.distances[node.x, node.y + 1])
            {
                origin.distances[node.x, node.y + 1] = origin.distances[node.x, node.y] + 1;
                map[node.x, node.y + 1].distances[origin.x, origin.y] = origin.distances[node.x, node.y] + 1;
                toVisit.Enqueue(map[node.x, node.y + 1]);
            }
            if (node.l != Node.Connection.Wall && origin.distances[node.x, node.y] + 1 <= origin.distances[node.x - 1, node.y])
            {
                origin.distances[node.x - 1, node.y] = origin.distances[node.x, node.y] + 1;
                map[node.x - 1, node.y].distances[origin.x, origin.y] = origin.distances[node.x, node.y] + 1;
                toVisit.Enqueue(map[node.x - 1, node.y]);
            }
            if (node.r != Node.Connection.Wall && origin.distances[node.x, node.y] + 1 <= origin.distances[node.x + 1, node.y])
            {
                origin.distances[node.x + 1, node.y] = origin.distances[node.x, node.y] + 1;
                map[node.x + 1, node.y].distances[origin.x, origin.y] = origin.distances[node.x, node.y] + 1;
                toVisit.Enqueue(map[node.x + 1, node.y]);
            }
            if (node.d != Node.Connection.Wall && origin.distances[node.x, node.y] + 1 <= origin.distances[node.x, node.y - 1])
            {
                origin.distances[node.x, node.y - 1] = origin.distances[node.x, node.y] + 1;
                map[node.x, node.y - 1].distances[origin.x, origin.y] = origin.distances[node.x, node.y] + 1;
                toVisit.Enqueue(map[node.x, node.y - 1]);
            }
        }
    }
}
