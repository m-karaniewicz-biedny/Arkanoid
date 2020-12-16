using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PowerUpEffect", menuName = "Game/Power Up Effect")]
public class PowerUpEffect : ScriptableObject
{
    [Header("Pick Up Settings")]
    public Sprite sprite;
    public Color iconColor = Color.white;
    public Color backgroundColor = Color.white;
    public float baseSpeed = 8;
    public float acceleration = 0f;

    [Header("One-time Effects")]
    public int money = 1;
    public int balls = 0;
    public int shield = 0;
    public float paddleLength = 0;

    [Header("Timed Effects")]
    public float mainDuration = 0;
    public float paddleLengthMultiplier = 1;
    public float ballSpeedMultiplier = 1;
    public bool ballGravity = false;
    public bool maxPiercing = false;
    public bool infiniteAmmo = false;

}