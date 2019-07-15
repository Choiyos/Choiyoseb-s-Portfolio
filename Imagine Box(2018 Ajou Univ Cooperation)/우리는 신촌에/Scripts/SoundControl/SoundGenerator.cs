using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundGenerator
{
    public static void SpreadSound(Sound sound, Vector3 origin)
    {
        Collider[] listeners = Physics.OverlapSphere(origin, sound.Range);
        foreach (var listener in listeners)
        {
            if (listener.CompareTag("Enemy"))
            {
                listener.GetComponent<Enemy>().ListeningToSounds(sound, origin);
            }
        }
    }
}
