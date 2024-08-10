using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntersceneManager : MonoBehaviour
{
    string levelFile; // The name of the file the levelSpawner should load

    public string LevelFile
    {
        get { return levelFile; }
        set { levelFile = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        Object.DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("Main Menu");
    }
}
