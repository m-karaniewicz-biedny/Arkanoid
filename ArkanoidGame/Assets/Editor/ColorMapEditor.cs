using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColorMap))]
public class ColorMapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ColorMap map = (ColorMap)target;

        /*
        GUILayout.BeginHorizontal();
        {
            Texture2D tex = map.ToTexture();

            if (tex)
            {
                GUILayout.Label(tex);
            }
        }
        GUILayout.EndHorizontal();
        */


        DrawDefaultInspector();

        if (GUILayout.Button("Export color map as PNG file"))
        {
            map.ExportColorPaletteAsPNGFile();
            AssetDatabase.Refresh();
        }






    }


}
