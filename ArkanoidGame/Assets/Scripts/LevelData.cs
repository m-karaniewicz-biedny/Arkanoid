using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data")]
public class LevelData : ScriptableObject
{
    public int cameraHeight = 8;
    public Sprite backgroundImage;
    public Texture2D colorCodeTexture;
}
