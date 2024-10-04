using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Animations;

public class CharacterCreationManager : MonoBehaviour
{
    // TODO: Maybe make this a constant somewhere and ensure that its always enforced
    [SerializeField] int spacing = 5;

    [Header("Instantiation Points")]
    [SerializeField] Transform theVoid; // Where weapon instances are created outside of camera view
    [SerializeField] Transform selectedWeaponList;
    [SerializeField] Transform shopList;

    [Header("Prefabs")]
    [SerializeField] GameObject weaponPreviewPrefab;
    [SerializeField] GameObject shopTypePrefab;
    [SerializeField] List<GameObject> weaponPrefabs;

    List<WeaponPreview> selectedWeapons = new List<WeaponPreview> { };
    int maxSlots = 3; // This might be 4 in some cases with certain abilities

    [Header("Player Model")]
    [SerializeField] Animator playerAnimator;
    [SerializeField] Transform playerBody;
    [SerializeField] Transform playerRightHand;
    [SerializeField] Transform playerLeftHand;
    List<GameObject> weaponsInHands = new List<GameObject> { }; // A list of the weapon models in people's hands

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
            foreach (GameObject prefab in weaponPrefabs)
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
}
