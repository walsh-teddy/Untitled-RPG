using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCreationManager : MonoBehaviour
{
    [SerializeField] Transform theVoid;
    [SerializeField] Transform weaponPreviewArea;
    [SerializeField] GameObject weaponPreviewPrefab;
    [SerializeField] List<GameObject> weaponPrefabs;

    private void Start()
    {
        foreach (GameObject prefab in weaponPrefabs)
        {
            // Create each weapon and create a weapon preview object for it
            ActionSource weapon = Instantiate(prefab, theVoid).GetComponent<ActionSource>();

            weapon.Create(null);

            Instantiate(weaponPreviewPrefab, weaponPreviewArea).GetComponent<WeaponPreview>().Create(weapon);
        }
    }
}
