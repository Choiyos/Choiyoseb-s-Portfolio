using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound
{
    Vector3 source; // 소리의 근원지
    float volume; // 소리의 크기
    float range; // 소리의 범위

    public Sound(float volume, float range)
    {
        this.volume = volume;
        this.range = range;
    }

    public float Volume
    {
        get { return this.volume; }
    }

    public float Range
    {
        get { return this.range; }
    }
}
