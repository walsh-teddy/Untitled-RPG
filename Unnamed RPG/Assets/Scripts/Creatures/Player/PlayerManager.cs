using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : TeamManager
{
    // Reference variables
    CameraFocus cameraFocus;

    // TODO: Make this copy players over into teamMembers
    List<Player> players = new List<Player> { };
    Player selectedPlayer;
    UIManager uiManager;

    // Properties
    public Player SelectedPlayer
    {
        get { return selectedPlayer; }
        set
        {
            // Clear the previously selected player's moveList
            selectedPlayer.DiscardAction();
            selectedPlayer.DiscardActionSource();

            // Turn off the old player's UI
            selectedPlayer.UIRoot.SetActive(false);

            // Update selected player
            selectedPlayer = value;

            // Update the game state
            if (!selectedPlayer.HasSubmittedAction)
            {
                game.CurrentState = Game.gameState.playerSelected;
            }
            else
            {
                game.CurrentState = Game.gameState.playerSelectedSubmitted;
            }

            // Focus the new selected player
            cameraFocus.MoveTo(selectedPlayer);

            uiManager.UpdateUI();
        }
    }

    public List<Player> Players
    {
        get { return players; }
    }

    // Constructor
    public PlayerManager() :
        base("player")
    {
        uiManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<UIManager>();
        cameraFocus = GameObject.FindGameObjectWithTag("CameraFocus").GetComponent<CameraFocus>();
    }

    public override void AddTeamMember(Creature teamMember)
    {
        base.AddTeamMember(teamMember);

        // Cast this team member to a player
        players.Add((Player)teamMember);
        selectedPlayer = players[0];
    }

    // Called when the submitAction button in the UI is clicked
    // TODO: Move all of this to the UI Manager
    public void SubmitActionButtonClicked()
    {
        selectedPlayer.SubmitActionButton();
        levelSpawner.UnHighlightAllTiles();
        game.CurrentState = Game.gameState.nothingSelected;
    }

    public void BackButtonClicked()
    {
        // Move back 1 state
        switch (game.CurrentState)
        {
            // Unselect the player
            case Game.gameState.playerSelected:
            case Game.gameState.playerSelectedSubmitted:
                game.CurrentState = Game.gameState.nothingSelected;
                break;

            // Unselect that action source
            case Game.gameState.playerActionSourceSelectAction:
                selectedPlayer.DiscardActionSource();
                game.CurrentState = Game.gameState.playerSelected;
                break;

            // Unselect the action
            case Game.gameState.playerActionSelectTarget:
                selectedPlayer.DiscardAction();
                game.CurrentState = Game.gameState.playerActionSourceSelectAction;
                break;
        }
    }

    public void UndoButtonClicked()
    {
        selectedPlayer.HasSubmittedAction = false;
        game.CurrentState = Game.gameState.playerSelected;

        // TODO: Clear the submitted action from the player and Game.cs
        selectedPlayer.DiscardAction();
    }

    public void CyclePlayers(int changeInIndex)
    {
        // Select the next player index
        int newIndex = players.IndexOf(selectedPlayer) + changeInIndex;

        // Loop the new index
        if (newIndex < 0) // It went before the first index
        { 
            // Loop it around to the end of the list
            newIndex = players.Count - 1;
        }
        else if (newIndex > players.Count - 1) // It went after the last index
        {
            // Loop it to the begining
            newIndex = 0;
        }

        // Update the selected player (use the property to use the other clean up code)
        SelectedPlayer = players[newIndex];
    }
}
