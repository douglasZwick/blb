using System.Collections.Generic;
using UnityEngine;

public class TilesPalette : MonoBehaviour
{
  // The container holding all the prefabs, keyed by tile type
  Dictionary<TileType, GameObject> m_TilePrefabs = new Dictionary<TileType, GameObject>();


  // Start is called before the first frame update
  void Start()
  {
    LoadPrefabs();
  }


  void LoadPrefabs()
  {
    // i want to be able to grab all the prefabs in a loop, but each one
    // has to be accessible via its enum type, so i need to be able to
    // get the path string from the enum type
    var tileTypesToPaths = new Dictionary<TileType, string>()
    {
      { TileType.SOLID,           "Prefabs/Tiles/Tile_Solid" },
      { TileType.GOAL,            "Prefabs/Tiles/Tile_Goal" },
      { TileType.DEADLY,          "Prefabs/Tiles/Tile_Deadly" },
      { TileType.START,           "Prefabs/Tiles/Tile_Start" },
      { TileType.SLOPE_LEFT,      "Prefabs/Tiles/Tile_Slope_Left" },
      { TileType.SLOPE_RIGHT,     "Prefabs/Tiles/Tile_Slope_Right" },
      { TileType.SLOPE_LEFT_INV,  "Prefabs/Tiles/Tile_Slope_Left_Inverted" },
      { TileType.SLOPE_RIGHT_INV, "Prefabs/Tiles/Tile_Slope_Right_Inverted" },
      { TileType.TELEPORTER,      "Prefabs/Tiles/Tile_Teleporter" },
      { TileType.DOOR,            "Prefabs/Tiles/Tile_Door" },
      { TileType.KEY,             "Prefabs/Tiles/Tile_Key" },
      { TileType.FALSE_SOLID,     "Prefabs/Tiles/Tile_False_Solid" },
      { TileType.COIN,            "Prefabs/Tiles/Tile_Coin" },
      { TileType.SWITCH,          "Prefabs/Tiles/Tile_Switch" },
      { TileType.INVISIBLE_SOLID, "Prefabs/Tiles/Tile_Invisible_Solid" },
      { TileType.CHECKPOINT,      "Prefabs/Tiles/Tile_Checkpoint" },
      { TileType.BOOSTER,         "Prefabs/Tiles/Tile_Booster" },
      { TileType.BG,              "Prefabs/Tiles/Tile_BG" },
      { TileType.BG_LEFT,         "Prefabs/Tiles/Tile_BG_Left" },
      { TileType.BG_RIGHT,        "Prefabs/Tiles/Tile_BG_Right" },
      { TileType.BG_LEFT_INV,     "Prefabs/Tiles/Tile_BG_Left_Inv" },
      { TileType.BG_RIGHT_INV,    "Prefabs/Tiles/Tile_BG_Right_Inv" },
    };

    // loop through the above dictionary
    foreach (var typePair in tileTypesToPaths)
    {
      var type = typePair.Key;
      var path = typePair.Value;

      // load the prefab from the path
      var prefab = Resources.Load(path) as GameObject;

      // report any problems to your supervisor
      if (prefab == null)
      {
        Debug.LogError($"Could not load {path}.");
        // keep calm and carry on
        continue;
      }

      // add the newly loaded prefab to the main dictionary of prefabs
      m_TilePrefabs[type] = prefab;
    }
  }


  public GameObject GetPrefabFromType(TileType type)
  {
    GameObject prefab = null;
    m_TilePrefabs.TryGetValue(type, out prefab);
    return prefab;
  }
}
