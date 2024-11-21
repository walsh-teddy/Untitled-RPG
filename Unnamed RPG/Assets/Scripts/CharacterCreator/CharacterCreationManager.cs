using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Animations;
using System.IO;

public class CharacterCreationManager : MonoBehaviour
{
    // TODO: Maybe make this a constant somewhere and ensure that its always enforced
    [SerializeField] int spacing = 5;

    [Header("Weapon Menu")]
    [SerializeField] GameObject weaponShop;
    [SerializeField] GameObject selectedWeaponsBox;
    [SerializeField] Button weaponButton;

    [Header("Level Menu")]
    [SerializeField] GameObject levelScreen;
    [SerializeField] Button levelButton;
    [Tooltip("Should always be Str, then Dex, then Int")]
    [SerializeField] List<Button> miusButtonList;
    [Tooltip("Should always be Str, then Dex, then Int")]
    [SerializeField] List<TextMeshProUGUI> statTextList;
    [Tooltip("Should always be Str, then Dex, then Int")]
    [SerializeField] List<Button> plusButtonList;
    [SerializeField] TextMeshProUGUI statDescription;

    [Header("Save Menu")]
    [SerializeField] GameObject saveScreen;
    [SerializeField] Button saveButton; // The one in the menu that you select
    [SerializeField] TMP_InputField nameInput;
    [SerializeField] Button saveCharacterButton; // The one you click to actually save the character

    [Header("Instantiation Points")]
    [SerializeField] Transform theVoid; // Where weapon instances are created outside of camera view
    [SerializeField] Transform selectedWeaponList;
    [SerializeField] Transform shopList;

    [Header("Prefabs")]
    [SerializeField] GameObject weaponPreviewPrefab;
    [SerializeField] GameObject shopTypePrefab;
    //[SerializeField] List<GameObject> weaponPrefabs;
    PrefabContainer prefabContainer;

    [Header("Player Model")]
    [SerializeField] Animator playerAnimator;
    [SerializeField] Transform playerBody;
    [SerializeField] Transform playerRightHand;
    [SerializeField] Transform playerLeftHand;
    List<GameObject> weaponsInHands = new List<GameObject> { }; // A list of the weapon models in people's hands

    // Weapon Variables
    List<WeaponPreview> selectedWeapons = new List<WeaponPreview> { };
    int maxSlots = 3; // This might be 4 in some cases with certain abilities

    // Level Variables
    int level = 3;
    const int STATS_PER_LEVEL = 3; // How many total points can be spent on stats per level
    const int MAX_STAT_INCREASE = 2; // The total a stat can be increased per level

    // Used for the str, dex, and int buttons (so I only need 2 functions instead of 6)
    class statButton
    {
        public Button minusButton;
        public TextMeshProUGUI text;
        public Button plusButton;
        public stats stat;
        int ammount;

        public int Ammount
        {
            get { return ammount; }
            set { ammount = value; }
        }
        public statButton(stats stat, Button minusButton, TextMeshProUGUI text, Button plusButton)
        {
            this.stat = stat;
            this.minusButton = minusButton;
            this.text = text;
            this.plusButton = plusButton;
            ammount = 0;
        }
    }
    Dictionary<stats, statButton> statButtons = new Dictionary<stats, statButton> { };

    private void Awake()
    {
        // Store the prefab container
        prefabContainer = GameObject.FindGameObjectWithTag("prefabContainer").GetComponent<PrefabContainer>();
    }

    private void Start()
    {
        // Create a shop for each type of weapon
        foreach (weaponType weaponType in weaponType.GetValues(typeof(weaponType)))
        {
            // Skip "None"
            if (weaponType == weaponType.None)
            {
                continue;
            }

            // Instantiate a shop for this weapon type
            UIShop shop = Instantiate(shopTypePrefab, shopList).GetComponent<UIShop>();

            // Fill in values for this shop
            shop.text.text = weaponType.ToString();

            // Spawn a UI item for each weapon that is in this type
            foreach (GameObject prefab in prefabContainer.WeaponPrefabs.Values)
            {
                // Create an object in the weapon list if this is the correct weapon type
                if (prefab.GetComponent<ActionSource>().WeaponType == weaponType) // This is the weapon type
                {
                    ActionSource weapon = Instantiate(prefab, theVoid).GetComponent<ActionSource>();

                    weapon.Create(null);

                    // Create the UI image
                    WeaponPreview weaponPreview = Instantiate(weaponPreviewPrefab, shop.weaponList.transform).GetComponent<WeaponPreview>();
                    weaponPreview.Create(weapon, this, spacing, false);

                    // Add a sibling if the weapon is versatile
                    if (weapon.IsVersatile) // It is versatile
                    {
                        // Move the model to the void
                        weapon.VersatileForm.gameObject.transform.SetParent(theVoid);

                        // Create the sibling
                        WeaponPreview versatilePreview = Instantiate(weaponPreviewPrefab, shop.weaponList.transform).GetComponent<WeaponPreview>();
                        versatilePreview.Create(weapon.VersatileForm, this, spacing, false, weaponPreview);

                        // Store this as the weapon's sibling
                        weaponPreview.VersatileForm = versatilePreview;
                    }
                }
            }
        }

        Canvas.ForceUpdateCanvases();

        // Configure stat dictionaries
        for (int i = 0; i <= 2; i ++)
        {
            statButtons.Add((stats)i, new statButton((stats)i, miusButtonList[i], statTextList[i], plusButtonList[i]));
        }

        ShowWeaponShop();
        UpdateUI();
    }

    public void AddWeapon(WeaponPreview weapon)
    {
        // Count each slot currently being used
        int slotsTaken = 0;
        int handsTaken = 0;
        foreach (WeaponPreview selectedWeapon in selectedWeapons)
        {
            slotsTaken += selectedWeapon.Source.Slots;
            handsTaken += selectedWeapon.Source.HandCount;
        }

        // Check if this weapon can fit
        if (slotsTaken + weapon.Source.Slots > maxSlots || handsTaken + weapon.Source.HandCount > 2) // There is not enough room for this weapon
        {
            // TODO: Play an animation or something
            Debug.Log("No space");
            return;
        }

        // Remake this weapon into the selected weapons list
        WeaponPreview newWeapon = Instantiate(weaponPreviewPrefab, selectedWeaponList).GetComponent<WeaponPreview>();
        newWeapon.Create(weapon.Source, this, spacing, true);
        selectedWeapons.Add(newWeapon);

        // Add a sibling if the weapon is versatile
        if (weapon.Source.IsVersatile) // It is versatile
        {
            // Create the sibling
            WeaponPreview versatilePreview = Instantiate(weaponPreviewPrefab, selectedWeaponList).GetComponent<WeaponPreview>();
            versatilePreview.Create(weapon.Source.VersatileForm, this, spacing, true, newWeapon);

            // Store this as the weapon's sibling
            newWeapon.VersatileForm = versatilePreview;
        }

        // Make the player model hold the new weapons
        UpdatePlayerDisplay(weapon);
    }

    public void RemoveWeapon(WeaponPreview weapon)
    {
        // First, destroy any versatile form it might've had
        if (weapon.Source.IsVersatile) // It is versatile
        {
            weapon.VersatileForm.Source.transform.SetParent(theVoid);
            weapon.VersatileForm.Delete();
        }
        selectedWeapons.Remove(weapon);
        weapon.Source.gameObject.transform.SetParent(theVoid);
        weapon.Delete();

        // Make the player model hold the new weapons
        UpdatePlayerDisplay(null);
    }

    public void SwitchToVersatileForm(WeaponPreview oldWeapon)
    {
        // Count the slots currently taken
        int slotsTaken = 0;
        int handsTaken = 0;
        foreach (WeaponPreview selectedWeapon in selectedWeapons)
        {
            // Don't count the old weapon being swapped from
            if (selectedWeapon != oldWeapon) // This is not the old weapon
            {
                slotsTaken += selectedWeapon.Source.Slots;
                handsTaken += selectedWeapon.Source.HandCount;
            }
        }

        // Make sure theres enough room to switch
        if (slotsTaken + oldWeapon.VersatileForm.Source.Slots > maxSlots || handsTaken + oldWeapon.VersatileForm.Source.HandCount > 2)
        {
            // TODO: Play an animation or something
            Debug.Log("No space");
            return;
        }

        // Swap to the new weapon
        selectedWeapons.Remove(oldWeapon);
        oldWeapon.gameObject.SetActive(false);
        selectedWeapons.Add(oldWeapon.VersatileForm);
        oldWeapon.VersatileForm.gameObject.SetActive(true);
        oldWeapon.HidePopout();

        // Make the player model hold the new weapons
        UpdatePlayerDisplay(oldWeapon.VersatileForm);
    }

    public void UpdatePlayerDisplay(WeaponPreview addedWeapon)
    {
        // Reset the old weapons
        bool rightHandOpen = true;
        bool leftHandOpen = true;
        hand addedWeaponHand = hand.None;

        // Delete all the old weapons
        // TODO: Maybe don't delete everything every time you add something new
        foreach (GameObject weapon in weaponsInHands)
        {
            // TODO: This is slow
            GameObject.Destroy(weapon);
        }

        // Reset left hand
        playerLeftHand.GetComponent<LeftHandTracker>().AttatchLeftHand(playerLeftHand);

        // Create a new weapon for each prefab in the starting inventory
        foreach (WeaponPreview weapon in selectedWeapons)
        {
            switch (weapon.Source.HandCount)
            {
                case 1: // 1 handed
                    // Decide which hand to put it in
                    if (rightHandOpen) // Right hand is open
                    {
                        // Create the weapon
                        GameObject newWeapon = Instantiate(weapon.Source.gameObject, playerRightHand);
                        ActionSource newSource = newWeapon.GetComponent<ActionSource>();
                        weaponsInHands.Add(newWeapon);
                        rightHandOpen = false;
                        addedWeaponHand = hand.Right;
                        playerAnimator.SetTrigger("1HRightInitial");
                    }
                    else if (leftHandOpen) // Left hand is open
                    {
                        // Create the weapon
                        GameObject newWeapon = Instantiate(weapon.Source.gameObject, playerLeftHand);
                        ActionSource newSource = newWeapon.GetComponent<ActionSource>();
                        weaponsInHands.Add(newWeapon);
                        leftHandOpen = false;
                        newWeapon.GetComponent<ActionSource>().FlipToLeftHand();
                        addedWeaponHand = hand.Left;
                        playerAnimator.SetTrigger("1HLeftInitial");
                    }
                    else
                    {
                        Debug.LogError("Both hands taken. Can't have more than 2 weapons, you fucking idiot bastard");
                    }
                    break;

                case 2: // 2 hand weapons
                    // Make sure both hands are open
                    if (rightHandOpen && leftHandOpen)
                    {
                        // Create the weapon
                        GameObject newWeapon = Instantiate(weapon.Source.gameObject, playerRightHand);
                        ActionSource newSource = newWeapon.GetComponent<ActionSource>();
                        weaponsInHands.Add(newWeapon);
                        addedWeaponHand = hand.Both;
                        newWeapon.name = "Weapon01";

                        playerLeftHand.GetComponent<LeftHandTracker>().AttatchLeftHand(newSource.LeftHandRest);
                        // Move both hands to the correct positions using animations depending on if its a pole or a hilt
                        if (newSource.AnimationType == weaponAnimationType.hilt)
                        {
                            playerAnimator.Play("hilt2HInitial");
                        }
                        else if (newSource.AnimationType == weaponAnimationType.pole)
                        {
                            playerAnimator.Play("pole2HInitial");
                        }
                        else if (newSource.AnimationType == weaponAnimationType.bow)
                        {
                            playerAnimator.Play("bow2HInitial");
                        }
                    }
                    else
                    {
                        Debug.LogError("Both hands taken. Can't have more than 2 weapons, you fucking idiot bastard");
                    }
                    break;
            }
        }

        // Play the animation of the primary action if this just added a new weapon
        if (addedWeapon != null) // A new weapon has been added
        {
            // The new weapon will be the latest one in the list
            // TODO: Maybe have the value be sent in rather than assuming it will always be the latest weapon

            // The actual weapon is on its own in the void and doesn't have an owner, so manually put together its animation trigger based on which hand we think its in
            string animationTrigger = addedWeapon.Source.AnimationType.ToString(); // Animation Type ("pole" or "hilt")
            animationTrigger += addedWeapon.Source.ActionList[0].BaseAnimationTrigger; // BaseAnimationTrigger ("slash" or "stab" ect...)
            animationTrigger += addedWeaponHand; // Held Hand ("Right", "Left", or "Both")

            //Debug.Log(animationTrigger);
            playerAnimator.Play(animationTrigger);
        }
    }

    public void HideEverything()
    {
        // Turn on/off different game objects
        weaponShop.SetActive(false);
        selectedWeaponsBox.SetActive(false);
        levelScreen.SetActive(false);
        saveScreen.SetActive(false);

        // Adjust the buttons
        weaponButton.interactable = true;
        levelButton.interactable = true;
        saveButton.interactable = true;

        Canvas.ForceUpdateCanvases();
    }

    public void ShowWeaponShop()
    {
        HideEverything();

        // Turn on different game objects
        weaponShop.SetActive(true);
        selectedWeaponsBox.SetActive(true);

        // Adjust the button
        weaponButton.interactable = false;
    }

    public void ShowLevelScreen()
    {
        HideEverything();

        // Turn on different game objects
        levelScreen.SetActive(true);

        // Adjust the button
        levelButton.interactable = false;

        Canvas.ForceUpdateCanvases();
    }

    public void ShowSaveScreen()
    {
        HideEverything();

        // Turn on different game objects
        saveScreen.SetActive(true);

        // Turn off the button
        saveButton.interactable = false;

        Canvas.ForceUpdateCanvases();
    }

    protected void UpStat(stats stat)
    {
        // Make sure this is not illegal
        if (statButtons[stats.strength].Ammount + statButtons[stats.dexterity].Ammount + statButtons[stats.intellect].Ammount >= STATS_PER_LEVEL * level // Too many total stats
            || statButtons[stat].Ammount >= MAX_STAT_INCREASE * level // Too much strength specifically
            )
        {
            // Break out of the function
            return;
        }

        // Increase the stat
        statButtons[stat].Ammount += 1;

        // TODO: Update the UI
        UpdateUI();
    }
    public void UpStr()
    {
        UpStat(stats.strength);
    }
    public void UpDex()
    {
        UpStat(stats.dexterity);
    }
    public void UpInt()
    {
        UpStat(stats.intellect);
    }

    protected void DownStat(stats stat)
    {
        // Make sure its not too low already
        if (statButtons[stat].Ammount <= 0) // Its already at the minimum
        {
            return;
        }

        // Decrease the stat
        statButtons[stat].Ammount -= 1;

        // Update the UI
        UpdateUI();
    }
    public void DownStr()
    {
        DownStat(stats.strength);
    }
    public void DownDex()
    {
        DownStat(stats.dexterity);
    }
    public void DownInt()
    {
        DownStat(stats.intellect);
    }

    public void UpdateUI()
    {
        // Adjust the buttons and text for each stat pair
        foreach (statButton statButton in statButtons.Values)
        {
            // Update the text
            statButton.text.text = statButton.Ammount.ToString();

            // Turn off the minus button if the stat is at 0
            statButton.minusButton.interactable = (statButton.Ammount > 0);

            // Turn off the plus button if the stat is at its max
            statButton.plusButton.interactable = (
                statButtons[stats.strength].Ammount + statButtons[stats.dexterity].Ammount + statButtons[stats.intellect].Ammount < STATS_PER_LEVEL * level // Total stats are not too high
                && statButton.Ammount < MAX_STAT_INCREASE * level // This stat is not too high
                );
        }

        // Update the level text
        int pointsRemaining = STATS_PER_LEVEL * level - statButtons[stats.strength].Ammount - statButtons[stats.dexterity].Ammount - statButtons[stats.intellect].Ammount;
        statDescription.text = 
            "Level: " + level + "\n" +
            "Points Left: " + (pointsRemaining) + "\n" +
            "Max Stat: " + MAX_STAT_INCREASE * level;

        TestNameChange();
    }

    public void TestNameChange()
    {
        // Only make the save character button clickable if there is a name
        // TODO: Also maybe require the levels to be fully alocated and weapon slots filled and stuff
        saveCharacterButton.interactable = nameInput.text != "";
    }

    public void SaveCharacter()
    {
        // Based on SaveLevel() from LevelEditor.cs

        // Create the folder if there isn't one yet
        string folder = Application.streamingAssetsPath + "/Characters/";
        Directory.CreateDirectory(folder);

        string text = ""; // Write to this text

        // Add the character name
        string characterName = nameInput.text;
        text += characterName + "\n";

        // Add the stats
        foreach (statButton statButton in statButtons.Values)
        {
            text += statButton.Ammount + ",";
        }
        text += "\n";

        // Add any weapons they have equipped
        // TODO: Use IDs of some kind rather than display names
        foreach (WeaponPreview weaponPreview in selectedWeapons)
        {
            text += weaponPreview.Source.DisplayName + ",";
        }
        text += "\n";

        // TODO: Add selected abilities

        // Save over any text that was there before
        // TODO: Mark this file with the index of the character
        string documentPath = folder + characterName + ".txt";
        File.WriteAllText(documentPath, text);
    }
}
