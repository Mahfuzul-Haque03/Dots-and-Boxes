using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyUI.Toast;
using UnityEngine.SceneManagement;
public class TaskManager : MonoBehaviour
{
    // Data to send in main game.
    public static int gridSize; // currently only supports 3*3 grid
    public static int difficultyLevel;
    public static string player1Name, player2Name;
    // Main menu settings
    // Game Objects taken to hide different settings and show them.
    public Canvas mainCanvas, settingCanvas,vsCanvas,inputCanvas,howToCanvas;

    public Dropdown difficulty; // Dropdown menu to select difficulty level varying from 1 to 5
    //Input field for player names
    public InputField player1, player2;
    // Start is called before the first frame update
    void Start()
    {
        gridSize = 0;// 0 is actually index of grid size array.
        difficultyLevel = 0;
        HideAllCanvas();
        showMain();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void quitGame()
    {
        Application.Quit();
    }
    // Show functions are to show different canvas
    public void showSetting()
    {
        HideAllCanvas();
        settingCanvas.enabled = true;
    }
    public void saveSetting()
    {
        difficultyLevel = difficulty.value;
        Toast.Show("Setting Saved");
    }
    public void showMain()
    {
        HideAllCanvas();
        mainCanvas.enabled = true;
        difficulty.value = difficultyLevel;
    }
    public void showVS()
    {
        HideAllCanvas();
        vsCanvas.enabled = true;
    }
    public void showInput(string whoIsPlaying)
    {
        HideAllCanvas();
        inputCanvas.enabled = true;
        player1.characterLimit = 9;
        player2.characterLimit = 9;
        Debug.Log(whoIsPlaying);
        if(whoIsPlaying == "AI")
        {
            player2.text = "Robot";
            player2.enabled = false;
            GameManager.playingWithAI = true;
        }

    }
    public void testEdit()
    {
    }
    public void ShowHowToPlay()
    {
        HideAllCanvas();
        howToCanvas.enabled = true;

    }
    public void HideAllCanvas()
    {
        // hides all type of canvas.
        mainCanvas.enabled = false;
        settingCanvas.enabled = false;
        vsCanvas.enabled = false;
        inputCanvas.enabled = false;
        howToCanvas.enabled = false;
    }
    public void StartGame()
    {
        player1Name = player1.text;
        player2Name = player2.text;
        // Name validation
        if(player1.text.Length<1 || player2.text.Length < 1 || (!(player1Name[0]>='A' && player1Name[0]<='Z') && !(player1Name[0]>='a' && player1Name[0]<='z') )
            || (!(player2Name[0] >= 'A' && player2Name[0] <= 'Z') && !(player2Name[0] >= 'a' && player2Name[0] <= 'z')))
        {

            Toast.Show("Please Fill The Names Properly");
        }
        else SceneManager.LoadScene("GameScene");
        
    }

}
