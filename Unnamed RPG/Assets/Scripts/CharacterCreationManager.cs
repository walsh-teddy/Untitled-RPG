using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCreationManager : MonoBehaviour
{
    [Header("Instantiation Points")]
    [SerializeField] Transform theVoid; // Where weapon instances are created outside of camera view
    [SerializeField] Transform selectedWeaponList;
    [SerializeField] Transform shopList;

    [Header("Prefabs")]
    [SerializeField] GameObject weaponPreviewPrefab;
    [SerializeField] GameObject shopTypePrefab;
    [SerializeField] List<GameObject> weaponPrefabs;

    List<WeaponPreview> selectedWeapons;

    private void Start()
    {
        float spacing = shopTypePrefab.GetComponentInChildren<HorizontalLayoutGroup>().spacing;

        foreach (weaponType weaponType in weaponType.GetValues(typeof(weaponType)))
        {
            // Skip "None"
            if (weaponType == weaponType.None)
            {
                continue;
            }

            // Instantiate a shop for this weapon type
            GameObject shop = Instantiate(shopTypePrefab, shopList);

            // Fill in values for this shop
            // TODO: Maybe give each shop a script and have this be .Create()
            shop.GetComponentInChildren<TextMeshProUGUI>().text = weaponType.ToString();

            // Spawn a UI item for each weapon that is in this type
            Transform weaponList = shop.transform.Find("WeaponList");
            foreach (GameObject prefab in weaponPrefabs)
            {
                // Create an object in the weapon list if this is the correct weapon type
                if (prefab.GetComponent<ActionSource>().WeaponType == weaponType) // This is the weapon type
                {
                    ActionSource weapon = Instantiate(prefab, theVoid).GetComponent<ActionSource>();

                    weapon.Create(null);

                    // Create the UI image
                    WeaponPreview weaponPreview = Instantiate(weaponPreviewPrefab, weaponList).GetComponent<WeaponPreview>();
                    weaponPreview.Create(weapon, this, spacing, false);

                    // Add a sibling if the weapon is versatile
                    if (weapon.IsVersatile) // It is versatile
                    {
                        // Create the sibling
                        WeaponPreview versatilePreview = Instantiate(weaponPreviewPrefab, weaponList).GetComponent<WeaponPreview>();
                        versatilePreview.Create(weapon.VersatileForm, this, spacing, false, weaponPreview);

                        // Store this as the weapon's sibling
                        weaponPreview.VersatileForm = versatilePreview;
                    }
                }
            }
        }
    }

    public void AddWeapon(WeaponPreview weapon)
    {
        Debug.Log("AddWeapon() called for " + weapon.Source.DisplayName);
    }

    public void RemoveWeapon(WeaponPreview weapon)
    {
        Debug.Log("RemoveWeapon() called for " + weapon.Source.DisplayName);

    }

    public void SwitchToVersatileForm(WeaponPreview oldWeapon)
    {
        Debug.Log("SwitchToVersatileForm() called for " + oldWeapon.Source.DisplayName);
    }
}
