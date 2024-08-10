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
            game.CurrentState = gameState.playerSelected;

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
        game.CurrentState = gameState.nothingSelected;
    }

    public void BackButtonClicked()
    {
        // Move back 1 state
        switch (game.CurrentState)
        {
            // Unselect the player
            case gameState.playerSelected:
                game.CurrentState = gameState.nothingSelected;
                break;

            // Unselect that action source
            case gameState.playerActionSourceSelectAction:
                selectedPlayer.DiscardActionSource();
                game.CurrentState = gameState.playerSelected;
                break;

            // Unselect the action
            case gameState.playerActionSelectTarget:
                selectedPlayer.DiscardAction();
                game.CurrentState = gameState.playerActionSourceSelectAction;
                break;
        }
    }

    public void CyclePlayers(int changeInIndex)
    {
        // Don't do anything if the game is uninteractable
        if (game.CurrentState == gameState.uninteractable) // Its uninteractable
        {
            // Break out of the function
            return;
        }

        // Make sure any character who was selected becomes visible again
        cameraFocus.LeavePerspective();

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

    public override void RemoveTeamMember(Creature teamMember)
    {
        base.RemoveTeamMember(teamMember);

        players.Remove((Player)teamMember);

        selectedPlayer = players[0];
    }
}
