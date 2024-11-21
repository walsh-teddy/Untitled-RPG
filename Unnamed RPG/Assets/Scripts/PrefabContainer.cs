using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabContainer : MonoBehaviour
{
    // Lists of prefabs and the dictionaries they are associated with
    // TODO: Maybe make the dictionaries into properties rather than public variables

    // Weapons
    [Tooltip("Only include 1 version of versatile weapons")]
    public List<GameObject> _weaponPrefabList;
    public Dictionary<string, GameObject> WeaponPrefabs = new Dictionary<string, GameObject> { };

    // Tiles
    public List<GameObject> _tilePrefabList;
    public Dictionary<string, GameObject> TilePrefabs = new Dictionary<string, GameObject> { };

    // Obstacles
    public List<GameObject> _obstaclePrefabList;
    public Dictionary<string, GameObject> ObstaclePrefabs = new Dictionary<string, GameObject> { };

    // Players
    // TODO: Should maybe just use indexes instead of display names for the keys
    public List<GameObject> _playerPrefabList;
    public Dictionary<string, GameObject> PlayerPrefabs = new Dictionary<string, GameObject> { };

    // Enemies
    public List<GameObject> _enemyPrefabList;
    public Dictionary<string, GameObject> EnemyPrefabs = new Dictionary<string, GameObject> { };

    // Abilities
    public List<AbilityData> _abilityDataList;
    public Dictionary<string, AbilityData> AbilityData = new Dictionary<string, AbilityData> { };

    // Fill in each dictionary with the data from the lists
    private void Awake()
    {
        // Weapons
        foreach (GameObject weaponPrefab in _weaponPrefabList)
        {
            WeaponPrefabs.Add(weaponPrefab.GetComponent<ActionSource>().DisplayName, weaponPrefab);
        }

        // Tiles
        foreach (GameObject tilePrefab in _tilePrefabList)
        {
            TilePrefabs.Add(tilePrefab.GetComponent<Tile>().DisplayName, tilePrefab);
        }

        // Obstacles
        foreach (GameObject obstaclePrefab in _obstaclePrefabList)
        {
            ObstaclePrefabs.Add(obstaclePrefab.GetComponent<Obstacle>().DisplayName, obstaclePrefab);
        }

        // Players
        foreach (GameObject playerPrefab in _playerPrefabList)
        {
            PlayerPrefabs.Add(playerPrefab.GetComponent<Player>().DisplayName, playerPrefab);
        }

        // Enemies
        foreach (GameObject enemyPrefab in _enemyPrefabList)
        {
            EnemyPrefabs.Add(enemyPrefab.GetComponent<Creature>().DisplayName, enemyPrefab);
        }

        // Abilities
        foreach (AbilityData ability in _abilityDataList)
        {
            AbilityData.Add(ability.displayName, ability);
        }
    }
}
