using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Windows;

public class LevelManager : MonoBehaviour
{
    public List<GameObject> prefabList;

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject lossTriggerPrefab;
    [SerializeField] private ColorMap colorMap;

    private Texture2D[] levels;

    private int currentLevelIndex;

    [HideInInspector]
    public static List<GameObject> entityList = new List<GameObject>();
    [HideInInspector]
    public static List<GameObject> eliminationRequiredList = new List<GameObject>();

    private Vector2 gridCellExtents = new Vector2(0.5f, 0.5f);
    private static Transform levelParent;

    public static Rect playArea = new Rect();

    private void Awake()
    {
        levelParent = new GameObject().transform;
        levelParent.name = "LevelParent";
        levels = Resources.LoadAll<Texture2D>("Levels");
        if (levels.Length <= 0) Debug.LogError("No levels loaded");
        if (colorMap == null) Debug.LogError("Missing color map reference");
        if (prefabList == null) Debug.LogError("Missing block type list");
    }

    public static void LoadScene(string name)
    {
        Debug.Log("Level load requested for: " + name);
        //Application.LoadLevel(name);
        SceneManager.LoadScene(name);
    }

    public static void QuitRequest()
    {
        Application.Quit();
    }

    private void DestroyPreviousLevel()
    {
        foreach (GameObject entry in entityList)
        {
            Destroy(entry);
        }
        eliminationRequiredList.Clear();
        entityList.Clear();
    }

    public void LoadLevel(int index)
    {
        if (index >= 0 && index < levels.Length)
        {
            DestroyPreviousLevel();

            currentLevelIndex = index;

            AdjustCameraToLevel(new Vector2(levels[currentLevelIndex].width, levels[currentLevelIndex].height));

            LoadLevelFromTexture(levels[currentLevelIndex], colorMap);

            PaddleController vaus = FindObjectOfType<PaddleController>();
            vaus.transform.position = new Vector2(playArea.width / 2f, 1f);

            vaus.SpawnStartingBalls();

            SpawnLevelBorders(playArea);

        }
    }

    private void SpawnLevelBorders(Rect levelBorder)
    {
        Vector2 defaultBorderSize = new Vector2(1, 1);

        Rect inflatedBorder = new Rect(levelBorder);
        inflatedBorder.width += defaultBorderSize.x;
        inflatedBorder.height += defaultBorderSize.y;

        //Ceiling
        SpawnRectPrefab(wallPrefab, new Rect(
            levelBorder.x,
            levelBorder.y + inflatedBorder.height / 2,
            levelBorder.width,
            defaultBorderSize.y));

        //Sides
        SpawnRectPrefab(wallPrefab, new Rect(
            levelBorder.x + inflatedBorder.width / 2,
            levelBorder.y,
            defaultBorderSize.x,
            levelBorder.height));
        SpawnRectPrefab(wallPrefab, new Rect(
            levelBorder.x - inflatedBorder.width / 2,
            levelBorder.y,
            defaultBorderSize.x,
            levelBorder.height));

        //Floor (loss trigger)
        SpawnRectPrefab(lossTriggerPrefab, new Rect(
            levelBorder.x,
            levelBorder.y - inflatedBorder.height / 2,
            levelBorder.width,
            defaultBorderSize.y));
    }

    private void SpawnRectPrefab(GameObject rectPrefab, Rect rect)
    {
        GameObject obj = Instantiate(rectPrefab, new Vector2(rect.x, rect.y), Quaternion.identity);
        obj.transform.localScale = new Vector2(rect.width, rect.height);
        entityList.Add(obj);
    }

    private void LoadLevelFromTexture(Texture2D texture, ColorMap colorMap)
    {
        int[,] values = colorMap.GetValuesFromTexture(texture);

        for (int x = 0; x < values.GetLength(0); x++)
        {
            for (int y = 0; y < values.GetLength(1); y++)
            {
                GameObject obj = CreateLevelElement(prefabList[values[x, y]], new Vector2(x, y));
                if(obj!=null)
                {
                    if (obj.CompareTag("Required"))
                    {
                        eliminationRequiredList.Add(obj);
                    }
                }
            }
        }
    }

    private void AdjustCameraToLevel(Vector2 levelSize)
    {
        Camera.main.orthographicSize = levelSize.y / 2f;

        Vector2 center = new Vector2(Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
        Camera.main.transform.position = new Vector3(center.x, center.y, Camera.main.transform.position.z);

        playArea = new Rect(center, levelSize);

    }

    private GameObject CreateLevelElement(GameObject prefab, Vector2 position)
    {
        if (prefab != null)
        {
            GameObject obj = Instantiate(prefab, position + gridCellExtents, Quaternion.identity,levelParent);
            entityList.Add(obj);

            return obj;
        }
        else
        {
            //Debug.Log($"Spawned nothing on {position}");
            return null;
        }
    }



    private void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(GetPlayAreaCenterPosition(),new Vector2(GetPlayAreaWidth(),GetPlayAreaHeight()));

    }

    /*
    private void LoadLevelFromPrefab(GameObject prefab)
    {
        if (prefab != null)
        {
            GameObject lvl = Instantiate(prefab, Camera.main.transform.position, Quaternion.identity);
            entityList.Add(lvl);
        }
        else Debug.LogError("No prefab to load!");
    }
    */

}
