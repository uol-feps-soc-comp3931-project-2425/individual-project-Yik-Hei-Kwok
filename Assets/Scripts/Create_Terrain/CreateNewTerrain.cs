using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateNewTerrain : MonoBehaviour
{
    // detect if the select texture button is pressed (should be set back to false after terrain texture is selected)
    public bool terrainTextureSelected = false;
    public void initializeTerrain(int x)
    {
        // load the scene 
        SceneManager.LoadScene(sceneName: "New_Terrain");

        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (scene.name == "New_Terrain")
            {
                createTerrain(x, x);
            }
        };
    }


    private void createTerrain(int width, int depth)
    {
        // Create a new Terrain GameObject
        GameObject terrainGO = new GameObject("GeneratedTerrain");
        Terrain terrain = terrainGO.AddComponent<Terrain>();
        TerrainData terrainData = new TerrainData();

        // Set terrain size
        terrainData.size = new Vector3(width, 50, depth); // 50 is the height scale

        // Assign terrain data
        terrain.terrainData = terrainData;

        // Add a Terrain Collider
        TerrainCollider terrainCollider = terrainGO.AddComponent<TerrainCollider>();
        terrainCollider.terrainData = terrainData;
    }
}
