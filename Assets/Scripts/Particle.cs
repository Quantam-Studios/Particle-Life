using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Particle
{
    public Color color;

    [Range(-1f, 1f)]
    public float pull;

    [Range(0f, 10f)]
    public float effectRadius;

    public int column;

    public int[] row;

}
