using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//The class handles everything about games
public class GameManager : MonoBehaviour
{
    // Player class contains the name and score to show in game.
    class Player
    {
        public string Name;
        public int Score;
    }
    /* Prefabs */
    public static bool playingWithAI;
    // pointPrefab, namePrefab and LineDrawerPrefab is designed in unity
    public GameObject pointPrefab, namePrefab;
    public LineRenderer LineDrawerPrefab;
    public Sprite[] Letters; // mapped from unity 
    private Board gameBoard;
    private Player player1, player2;
    private Player currentPlayer;
    public Text playerPoint1, playerPoint2, playerName1, playerName2, topText,winnerText;
    // game over canvas is shown after game is over.
    public Canvas gameOverCanvas, mainCanvas;
    // index of Letters array to fill the cells.
    private int playerInitial1, playerInitial2;
    // Start is called before the first frame update
    void Start()
    {
        gameBoard = new Board(TaskManager.gridSize);
        gameBoard.depth_id = TaskManager.difficultyLevel;
        gameBoard.createBoard(pointPrefab);
        gameBoard.fillerPrefab = namePrefab;
        player1 = new Player();
        player2 = new Player();
        currentPlayer = player1;
        player1.Name = TaskManager.player1Name;
        player2.Name = TaskManager.player2Name;
        playerName1.text = player1.Name;
        playerName2.text = player2.Name;
        playerInitial1 = player1.Name.ToLower()[0]-'a';
        playerInitial2 = player2.Name.ToLower()[0]-'a';
        if (playingWithAI)
        {
            playerInitial2 = 26;
        }
        // The game starts with player 1
        gameBoard.filler = Letters[playerInitial1];
        gameOverCanvas.enabled = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gameBoard.numberOfRemainingLines == 0)
        {
            //When the game finishes the display pauses for .5s before showing game over.
            Thread.Sleep(500);
            GameOver();
        }
        else
        {
            if (playingWithAI && currentPlayer == player2)
            {
                //Cursor is hidden when AI plays the game.
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                gameBoard.DPmove();
                //gameBoard.randomMove();
                Thread.Sleep(400);
                //draw the line and reset the dots
                DrawLine();
                gameBoard.resetDot(ref gameBoard.beginDot);
                gameBoard.resetDot(ref gameBoard.endDot);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0))
            {
                CastRay();
                // if two valid point is selected draw a line
                if (gameBoard.twoPointSelected())
                {
                    bool validDotSelected = gameBoard.checkValidity();
                    if (validDotSelected)
                    {
                        DrawLine();
                    }
                    gameBoard.resetDot(ref gameBoard.beginDot);
                    gameBoard.resetDot(ref gameBoard.endDot);
                }
            }
            playerPoint1.text = player1.Score.ToString();
            playerPoint2.text = player2.Score.ToString();
            topText.text = "<color=#008080ff>" + currentPlayer.Name + "</color> is Playing Now";
        }

    }
    void DrawLine()
    {
        // check if drawing a line increases any point.
        gameBoard.DrawLine(LineDrawerPrefab);
        int obtainedPoint = gameBoard.pointGained();
        if (obtainedPoint > 0)
        {
            IncreaseScore(obtainedPoint);
        }
        else
        {
            if (currentPlayer == player1)
            {
                currentPlayer = player2;
                gameBoard.filler = Letters[playerInitial2];
            }
            else
            {
                currentPlayer = player1;
                gameBoard.filler = Letters[playerInitial1];
            }
        }
        gameBoard.numberOfRemainingLines--;
    }
    void CastRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.Log(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit.collider != null)
        {
            GameObject point = hit.collider.gameObject;
            gameBoard.selectDot(point);
        }
    }
    void IncreaseScore(int point)
    {
        currentPlayer.Score += point;
    }
    void GameOver()
    {
        // kill previous instances of asteroids
        objectDestroy("linePrefab");
        objectDestroy("pointPrefab");
        objectDestroy("namePrefab");
        Destroy(gameBoard);
        mainCanvas.enabled = false;
        gameOverCanvas.enabled = true;
        string winnerName;
        int score;
        if (player1.Score > player2.Score)
        {
            winnerName = player1.Name;
            score = player1.Score;
        }
        else
        {
            winnerName = player2.Name;
            score = player2.Score;
        }
        winnerText.text = "<b>" + winnerName + "</b> Wins the Game with <b>" + score.ToString() + "</b> Points"; 
    }
    public void ReplayGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
    public void GoBackToMain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }
    void objectDestroy(string tag)
    {
        GameObject[] allObjects = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in allObjects)
        {
            Destroy(obj);
        }
    }
}
