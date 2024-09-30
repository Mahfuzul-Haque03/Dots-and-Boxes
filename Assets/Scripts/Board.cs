using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    /* Board Data */
    private int boardSize;
    // increment value is for drawing the graph.scale value is for character scaling
    private float beginPoint, incrementValue, scaleValue;
    public int numberOfRemainingLines;
    /* Colors */
    private Color32 selectedColor = new Color32(64, 128, 192, 255);
    private Color32 pointPrefabColor = new Color32(171, 231, 171, 255);
    /* Selected Point */
    public GameObject beginDot, endDot;
    public GameObject fillerPrefab; // for filling cell with initial
    /* Board Structure Dictionary - currently not needed*/
    private Dictionary<int, (int gridSize, float beginPoint, float incrementValue, float scaleValue)> boardStructure = new
        Dictionary<int, (int gridSize, float beginPoint, float incrementValue, float scaleValue)>()
    { {0, (3, -52.5f, 35f, 7f)}, {1, (5, -75f, 30f, 6f)} , {2, (7, -87.5f, 25f, 5f)} , {3, (9, -99f, 22f, 4f)}  };
    private Dictionary<Vector3, (int x, int y)> pointMap;
    private Dictionary<(int x1, int y1, int x2, int y2), bool> LineDrawn;
    public Sprite filler;
    private List<GameObject> pointPool;
    /* Data for AI Implementations */
    private Dictionary<int, (int x1, int y1, int x2, int y2)> dpLineMap;
    private Dictionary<(int x1, int y1, int x2, int y2), int> dpLineReverseMap;
    private Dictionary<(int x1, int y1), GameObject> dpPointPool;
    private int[] dpState, visState, nextState;
    private int currentMask, totalLine;
    public int max_depth,depth_id;
    private int[] depths = { 2, 3, 4,6,7 };
    public Board(int id)
    {
        beginDot = null;
        endDot = null;
        boardSize = boardStructure[id].gridSize;
        beginPoint = boardStructure[id].beginPoint;
        incrementValue = boardStructure[id].incrementValue;
        scaleValue = boardStructure[id].scaleValue;
        pointMap = new Dictionary<Vector3, (int x, int y)>();
        LineDrawn = new Dictionary<(int x1, int y1, int x2, int y2), bool>();
        numberOfRemainingLines = 2 * (boardSize * boardSize + boardSize);
        totalLine = numberOfRemainingLines;
        pointPool = new List<GameObject>();
        dpLineMap = new Dictionary<int, (int x1, int y1, int x2, int y2)>();
        dpLineReverseMap = new Dictionary<(int x1, int y1, int x2, int y2), int>();
        dpPointPool = new Dictionary<(int x1, int y1), GameObject>();
        currentMask = 0;
        dpState = new int[1 << (1 + numberOfRemainingLines)];
        visState = new int[1 << (1 + numberOfRemainingLines)];
        nextState = new int[1 << (1 + numberOfRemainingLines)];
    }

    public void createBoard(GameObject pointPrefab)
    {
        //Creating Board in display.
        int counter = 1;
        max_depth = depths[depth_id];
        for (float i = beginPoint, n = 0; n <= boardSize; i += incrementValue, n++)
        {
            for (float j = beginPoint, m = 0; m <= boardSize; j += incrementValue, m++)
            {
                Vector3 gridPoint = new Vector3(i, j, -1f);
                GameObject point = Instantiate(pointPrefab, gridPoint, Quaternion.identity);
                pointPool.Add(point);
                pointMap[gridPoint] = ((int)n, (int)m);
                dpPointPool[((int)n, (int)m)] = point;

            }
        }
        //horizontal line creation.
        for (int i = 0; i <= boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                dpLineMap[counter] = (i, j, i, j + 1);//assigning each line a value.and storing it in backward too.
                dpLineReverseMap[(i, j, i, j + 1)] = counter;
                counter++;
            }
        }
        //vertical line creation.
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j <= boardSize; j++)
            {
                dpLineMap[counter] = (i, j, i + 1, j);
                dpLineReverseMap[(i, j, i + 1, j)] = counter;
                counter++;
            }
        }
    }
    public bool twoPointSelected()
    {
        if (beginDot == null || endDot == null) return false;
        else return true;
    }
    public bool checkValidity()
    {
        // line validity - line is horizontal or vertical. it has not been drawn before.
        (int x, int y) dot1 = pointMap[beginDot.transform.position];
        (int x, int y) dot2 = pointMap[endDot.transform.position];
        if (LineDrawn.ContainsKey((dot1.x, dot1.y, dot2.x, dot2.y))) return false;
        if ((dot1.x == dot2.x && Math.Abs(dot1.y - dot2.y) == 1)
                || (dot1.y == dot2.y && Math.Abs(dot1.x - dot2.x) == 1))
        {
            LineDrawn[(dot1.x, dot1.y, dot2.x, dot2.y)] = true;
            LineDrawn[(dot2.x, dot2.y, dot1.x, dot1.y)] = true;
            return true;
        }
        return false;
    }
    LineRenderer lineRenderer;
    public void DrawLine(LineRenderer LineDrawer)
    {
        //Line drawing helper function.
        lineRenderer = Instantiate(LineDrawer);
        lineRenderer.SetPosition(0, new Vector3(beginDot.transform.position.x, beginDot.transform.position.y, 0));
        lineRenderer.SetPosition(1, new Vector3(endDot.transform.position.x, endDot.transform.position.y, 0));
        (int x1, int y1) = pointMap[beginDot.transform.position];
        (int x2, int y2) = pointMap[endDot.transform.position];
        if (x1 > x2 || (x1 == x2 && y1 > y2))
        {
            (x1, x2) = (x2, x1);
            (y1, y2) = (y2, y1);
        }
        int lineMask = dpLineReverseMap[(x1, y1, x2, y2)];
        currentMask |= (1 << lineMask);
    }
    public void selectDot(GameObject Dot)
    {
        //two dot selection.
        if (beginDot == Dot) resetDot(ref beginDot);
        else if (beginDot == null) setDot(ref beginDot, Dot);
        else setDot(ref endDot, Dot);
    }
    public void setDot(ref GameObject GameDot, GameObject selectedDot)
    {
        GameDot = selectedDot;
        GameDot.GetComponent<SpriteRenderer>().color = selectedColor;
        return;
    }
    public void resetDot(ref GameObject GameDot)
    {
        GameDot.GetComponent<SpriteRenderer>().color = pointPrefabColor;
        GameDot = null;
        return;

    }
    public bool isSquare(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
    {
        // check if a line creates a square.
        if (LineDrawn.ContainsKey((x1, y1, x2, y2)) && LineDrawn.ContainsKey((x1, y1, x4, y4)) && LineDrawn.ContainsKey((x2, y2, x3, y3)) && LineDrawn.ContainsKey((x3, y3, x4, y4)))
        {
            fillCell(x1, y1, x2, y2, x3, y3, x4, y4);
            return true;
        }
        return false;
    }
    public int pointGained()
    {
        Vector3 startGridPoint = beginDot.transform.position;
        Vector3 endGridPoint = endDot.transform.position;
        (int x, int y) point1 = pointMap[startGridPoint];
        (int x, int y) point2 = pointMap[endGridPoint];
        int x1 = point1.x, y1 = point1.y;
        int x2 = point2.x, y2 = point2.y;
        if (x1 != x2) return horizontalSquareCreated(x1, y1, x2, y2);
        else return verticalSquareCreated(x1, y1, x2, y2);
    }
    public int horizontalSquareCreated(int x1, int y1, int x2, int y2)
    {
        int result = 0;
        if (y2 + 1 <= boardSize && isSquare(x1, y1, x2, y2, x2, y2 + 1, x1, y1 + 1))
        {
            result++;
        }
        if (y2 - 1 >= 0 && isSquare(x1, y1, x2, y2, x2, y2 - 1, x1, y1 - 1))
        {
            result++;
        }
        return result;
    }
    public int verticalSquareCreated(int x1, int y1, int x2, int y2)
    {
        int result = 0;
        if (x1 + 1 <= boardSize && isSquare(x1, y1, x1 + 1, y1, x2 + 1, y2, x2, y2))
        {
            result++;
        }
        if (x1 - 1 >= 0 && isSquare(x1, y1, x1 - 1, y1, x2 - 1, y2, x2, y2))
        {
            result++;
        }
        return result;
    }

    void fillCell(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4)
    {
        Debug.Log("Filling Cell");
        int min_x = new[] { x1, x2, x3, x4 }.Min();
        int max_x = new[] { x1, x2, x3, x4 }.Max();
        int min_y = new[] { y1, y2, y3, y4 }.Min();
        int max_y = new[] { y1, y2, y3, y4 }.Max();
        float X1 = beginPoint + incrementValue * min_x;
        float X2 = beginPoint + incrementValue * max_x;
        float Y1 = beginPoint + incrementValue * min_y;
        float Y2 = beginPoint + incrementValue * max_y;
        float mid_x = (X1 + X2) / 2;
        float mid_y = (Y1 + Y2) / 2;
        GameObject letter = Instantiate(fillerPrefab, new Vector3(mid_x, mid_y, -1), Quaternion.identity);
        letter.GetComponent<SpriteRenderer>().sprite = filler;
        letter.transform.localScale = new Vector3(scaleValue, scaleValue, 1);
    }

    public void randomMove()
    {
        Debug.Log(pointPool.Count);
        for (int i = 0; i < pointPool.Count; i++)
        {
            for (int j = i + 1; j < pointPool.Count; j++)
            {
                selectDot(pointPool[i]);
                selectDot(pointPool[j]);
                Debug.Log(pointPool[i]);
                if (checkValidity()) return;
                else
                {
                    resetDot(ref beginDot);
                    resetDot(ref endDot);
                }

            }
        }
    }
    public void DPmove()
    {
        visState = new int[1 << (totalLine + 1)];
        heuristic(currentMask, 0, 0,0);
        visState = null;
        int robotMove = nextState[currentMask];
        (int x1, int y1, int x2, int y2) = dpLineMap[robotMove];
        GameObject point1 = dpPointPool[(x1, y1)];
        GameObject point2 = dpPointPool[(x2, y2)];
        selectDot(point1);
        selectDot(point2);
        checkValidity();
        return;

    }

  
   //Main heuristic function for game.
    int heuristic(int mask, int depth, int minimax,int p_dif)
    {   
        if (mask == ((1 << totalLine) - 1) * 2)
        {
            return 0;
        }
        if (visState[mask] > 0) return dpState[mask];
        visState[mask] = 1;
        int maxGain = 0;
        int nextMove = -1;
        for (int i = 1; i <= totalLine; i++)
        {
            
            if ((mask & (1 << i)) > 0) continue;
            (int x1, int y1, int x2, int y2) = dpLineMap[i];
            int pointGained = 0;
            if (x1 == x2)
            {
                if (x1 + 1 <= boardSize)
                {
                    int b = dpLineReverseMap[(x1, y1, x1 + 1, y1)];
                    int c = dpLineReverseMap[(x1 + 1, y1, x1 + 1, y2)];
                    int d = dpLineReverseMap[(x1, y2, x1 + 1, y2)];
                    if (((mask & (1 << b)) > 0) && ((mask & (1 << c)) > 0) && ((mask & (1 << d)) > 0))
                    {
                        pointGained++;
                    }
                }
                if (x1 > 0)
                {
                    int b = dpLineReverseMap[(x1 - 1, y1, x1, y1)];
                    int c = dpLineReverseMap[(x1 - 1, y1, x1 - 1, y2)];
                    int d = dpLineReverseMap[(x1 - 1, y2, x1, y2)];
                    if (((mask & (1 << b)) > 0) && ((mask & (1 << c)) > 0) && ((mask & (1 << d)) > 0))
                    {
                        pointGained++;
                    }
                }
            }
            else
            {
                if (y1 + 1 <= boardSize)
                {
                    int b = dpLineReverseMap[(x1, y1, x1, y1 + 1)];
                    int c = dpLineReverseMap[(x1, y1 + 1, x2, y1 + 1)];
                    int d = dpLineReverseMap[(x2, y1, x2, y1 + 1)];
                    if (((mask & (1 << b)) > 0) && ((mask & (1 << c)) > 0) && ((mask & (1 << d)) > 0))
                    {
                        pointGained++;
                    }
                }
                if (y1 > 0)
                {
                    int b = dpLineReverseMap[(x1, y1 - 1, x1, y1)];
                    int c = dpLineReverseMap[(x1, y1 - 1, x2, y1 - 1)];
                    int d = dpLineReverseMap[(x2, y1 - 1, x2, y1)];
                    if (((mask & (1 << b)) > 0) && ((mask & (1 << c)) > 0) && ((mask & (1 << d)) > 0))
                    {
                        pointGained++;
                    }
                }
            }
            int newGain = 0;
            int newMini = minimax;
            if (pointGained == 0) newMini ^= 1;
            int np_dif = p_dif;
            if (minimax == 1) np_dif -= pointGained;
            else np_dif += pointGained;
            if (max_depth >= depth) newGain = heuristic(mask | (1 << i), depth + 1,newMini,np_dif);
            if (pointGained > 0) newGain += pointGained;
            else newGain = pointGained - newGain;
            if (newGain > maxGain || nextMove == -1)
            {
                nextMove = i;
                maxGain = newGain;
            }
        }
        dpState[mask] = maxGain;
        nextState[mask] = nextMove;
        return maxGain;
    }
}
