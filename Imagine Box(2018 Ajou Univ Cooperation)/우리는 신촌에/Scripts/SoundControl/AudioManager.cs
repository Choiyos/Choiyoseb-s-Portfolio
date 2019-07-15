using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager soundManager = null;
    [SerializeField]
    GameObject audioPool; // 각종 소리 소스의 풀
    [SerializeField]
    AudioSource BgmSource; // BGM 소리 소스
    [SerializeField]
    AudioSource StepSoundSource; // 걷기 및 뛰기 소리 소스

    Stack<GameObject> audioPoolList;

    void Awake()
    {
        if (soundManager == null)
        {
            soundManager = this;
        }
        else if (soundManager != this)
            Destroy(gameObject);
        
        audioPoolList = new Stack<GameObject>();
        for (int i = 0; i < audioPool.transform.childCount; i++)
        {
            audioPoolList.Push(audioPool.transform.GetChild(i).gameObject);
        }
    }

    public static AudioManager Instance
    {
        get
        {
            return soundManager;
        }
    }

    public GameObject AudioPoolPop()
    {
        if (audioPoolList.Count != 0)
        {
            GameObject temp = audioPoolList.Pop();
            temp.SetActive(true);
            return temp;
        }
        else
            return null;
    }

    public void AudioPoolPush(GameObject obj)
    {
        obj.SetActive(false);
        audioPoolList.Push(obj);
    }
    
    public void PlaySoundOneShot(string clip, float time)
    {
        GameObject obj = AudioPoolPop();
        AudioSource temp = obj.GetComponent<AudioSource>();
        temp.clip = ResourceManager.Instance.GetAudioClip(clip); 
        temp.loop = false;
        temp.Play();
        StartCoroutine(SoundDelay(temp, time));
    }

    public void PlaySoundLoop(string clip, float time)
    {
        GameObject obj = AudioPoolPop();
        AudioSource temp = obj.GetComponent<AudioSource>();
        temp.clip =ResourceManager.Instance.GetAudioClip( clip);
        temp.loop = true;
        temp.Play();
        StartCoroutine(SoundDelay(temp, time));
    }

    public void PlayMainSound(string _clipname)
    {
        BgmSource.clip = ResourceManager.Instance.GetAudioClip( _clipname);
        BgmSource.Play();
    }

    public void PlayStepSound(string clip)
    {
        if (StepSoundSource.clip != ResourceManager.Instance.GetAudioClip(clip)
            || !StepSoundSource.isPlaying)
        {
            StepSoundSource.clip = ResourceManager.Instance.GetAudioClip(clip); 
            StepSoundSource.Play();
        }
    }

    public void StopMainSound()
    {
        if (BgmSource.isPlaying)
        {
            StopAllCoroutines();
            BgmSource.Stop();
        }
    }

    public void StopStepSound()
    {
        if (StepSoundSource.isPlaying)
        {
            StepSoundSource.Stop();
        }
    }

    public void MuteMainSound()
    {
        if (!BgmSource.mute)
            BgmSource.mute = true;
        else
            BgmSource.mute = false;
    }
    
    IEnumerator SoundDelay(AudioSource param, float time) // 오디오 풀의 소리 반환
    {
        yield return new WaitForSeconds(time);
        AudioPoolPush(param.gameObject);
    }
}
