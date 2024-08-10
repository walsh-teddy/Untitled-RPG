using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] string levelA = "Assets/Maps/560TestA2.txt";
    [SerializeField] string levelB = "Assets/Maps/560TestB2.txt";
    IntersceneManager intersceneManager;
    public void Awake()
    {
        intersceneManager = GameObject.FindGameObjectWithTag("Interscene").GetComponent<IntersceneManager>();
    }
    public void LoadVersionA()
    {
        intersceneManager.LevelFile = levelA;
        PlayGame();
    }

    public void LoadVersionB()
    {
        intersceneManager.LevelFile = levelB;
        PlayGame();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
