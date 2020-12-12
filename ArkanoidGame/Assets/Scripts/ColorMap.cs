using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class ColorMapEntry
{
    public string name;
    public Color color;
    public int value;
}


[CreateAssetMenu(fileName = "Color Map", menuName = "Game/Color Map")]
public class ColorMap : ScriptableObject
{
    public ColorMapEntry[] map;

    public string mapName = "ColorMapPalette";

    public int[,] GetValuesFromTexture(Texture2D texture)
    {
        int[,] values = new int[texture.width, texture.height];

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                values[x, y] = 0;

                Color pixelColor = texture.GetPixel(x, y);

                foreach (ColorMapEntry entry in map)
                {
                    if (entry.color.Equals(pixelColor))
                    {
                        values[x, y] = entry.value;
                        break;
                    }
                }
            }
        }

        return values;
    }

    public void ExportColorPaletteAsPNGFile()
    {
        byte[] _bytes = ToTexture().EncodeToPNG();

        string dirPath = Application.dataPath + "/Export/";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        dirPath = dirPath + mapName + ".png";

        File.WriteAllBytes(dirPath, _bytes);

        Debug.Log($"{_bytes.Length} bytes were saved as: {dirPath}");

    }

    public Texture2D ToTexture()
    {
        Texture2D texture = new Texture2D(map.Length, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int i = 0; i < map.Length; i++)
        {
            texture.SetPixel(i, 1, map[i].color);
        }

        texture.Apply();

        return texture;
    }
}
