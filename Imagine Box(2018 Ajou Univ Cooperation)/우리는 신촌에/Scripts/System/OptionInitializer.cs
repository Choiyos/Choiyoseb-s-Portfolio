using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionInitializer : MonoBehaviour
{
    // 설정 창을 업데이트 시키는 스크립트.

    AudioSource bgmSource;
    AudioSource stepSource;
    GameObject audioPool;
    Slider soundSlider;
    Slider bgmSlider;
    Transform audioSystem;
    Transform systemWindow;
    List<AudioSource> audiopoolList;

    public void Init(Transform _systemWindow)
    {
        audioSystem = GameObject.Find("AudioSystem").transform;
        systemWindow = _systemWindow;
        audioPool = audioSystem.Find("AudioPool").gameObject;
        bgmSource = audioSystem.transform.Find("BgmSoundSource").GetComponent<AudioSource>();
        stepSource = audioSystem.transform.Find("StepSoundSource").GetComponent<AudioSource>();

        // 슬라이더 조작에 따른 이벤트 설정
        soundSlider = systemWindow.Find("SoundSlider").GetComponent<Slider>();
        soundSlider.onValueChanged.AddListener((value) => SoundVolumeUpdate(value));
        bgmSlider = systemWindow.Find("BgmSlider").GetComponent<Slider>();
        bgmSlider.onValueChanged.AddListener((value) => bgmVolumeUpdate(value));

        audiopoolList = new List<AudioSource>();
        for (int i = 0; i < audioPool.transform.childCount; i++)
        {
            audiopoolList.Add(audioPool.transform.GetChild(i).GetComponent<AudioSource>());
        }
    }

    void bgmVolumeUpdate(float value)
    {
        bgmSource.volume = value;
    }

    void SoundVolumeUpdate(float value)
    {
        stepSource.volume = value;
        for (int i = 0; i < audiopoolList.Count; i++)
        {
            audiopoolList[i].volume = value;
        }
    }
}
