using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class uiPhaseBar : MonoBehaviour
{
    [SerializeField] Image body;
    [SerializeField] phase phase;
    GameObject gameManager;
    UIManager uiManager;
    Game game;
    Color activeColor;
    Color innactiveColor;

    public void Awake()
    {
        // Set up 
        gameManager = GameObject.FindGameObjectWithTag("GameManager");
        uiManager = gameManager.GetComponent<UIManager>();
        game = gameManager.GetComponent<Game>();

        // Set up color
        activeColor = uiManager.ColorByPhase(phase);
        activeColor.a = uiManager.ActiveColorAlpha;
        innactiveColor = activeColor;
        innactiveColor.a = uiManager.InnactiveColorAlpha;
    }

    public void UpdateUI()
    {
        // Test if the phase is active
        if (game.CurrentPhase == phase) // The phase is active
        {
            // Change the color
            body.color = activeColor;
        }
        else // The phase is not active
        {
            // Change the color
            body.color = innactiveColor;
        }
    }
}
