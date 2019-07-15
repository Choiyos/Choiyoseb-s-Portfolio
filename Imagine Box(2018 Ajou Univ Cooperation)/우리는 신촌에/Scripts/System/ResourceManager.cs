using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    [SerializeField]
    private Dictionary<string, Sprite> itemSprites;
    private Dictionary<string, AudioClip> audioClips;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
            Destroy(gameObject);

        LoadItemSprites();
        LoadSoundClips();
    }

    private void LoadItemSprites()
    {
        itemSprites = new Dictionary<string, Sprite>();
        Sprite[] items = Resources.LoadAll<Sprite>("ItemIcons/");

        foreach (Sprite item in items)
        {
            itemSprites.Add(item.name, item);
        }
    }
    private void LoadSoundClips()
    {
        audioClips = new Dictionary<string, AudioClip>();
        AudioClip[] items = Resources.LoadAll<AudioClip>("Sound/");
        foreach (AudioClip item in items)
        {
            audioClips.Add(item.name, item);
        }
    }

    public Sprite GetItemIcon(string _itemName)
    {
        return itemSprites[_itemName];
    }

    public AudioClip GetAudioClip(string _clipname)
    {
        return audioClips[_clipname];
    }
}
