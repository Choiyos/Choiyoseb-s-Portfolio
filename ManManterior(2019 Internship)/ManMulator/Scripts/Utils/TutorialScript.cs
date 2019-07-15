using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

/// <summary>
/// <b>사용 위치</b> : 모든 scene<br></br>
/// <b>사용 플러그인</b> : UniRx<br></br>
/// <b>참조 리소스</b> : Resources/Sprites/Tutorials/*, Resources/Tutorials/TutorialText.txt<br></br>
/// 튜토리얼(도움말)을 위한 스크립트.<br></br>
/// 도움말 버튼, 도움말 윈도우의 UI 기능 바인딩.
/// 모든 씬에서 DontDestroyOnLoad로 공유되므로 독립적으로 작성.<br></br>
/// 도움말 내용은 TutorialText.txt파일에 작성. 각 챕터, sub챕터와 이미지가 들어갈 부분은 특수문자로 구분함.
/// </summary>
public class TutorialScript : Singleton<TutorialScript>
{
    [SerializeField]
    GameObject view = default;

    [SerializeField]
    Button tutorialButton = default;

    [SerializeField]
    TextAsset tutorialTexts = default;
    Sprite[] tutorialImages = default;

    [SerializeField]
    Button prevButton = default;
    [SerializeField]
    Button nextButton = default;

    [SerializeField]
    GameObject buttonPrefab = default;
    [SerializeField]
    GameObject contentPrefab = default;
    [SerializeField]
    GameObject subContentPrefab = default;

    GameObject[] panels = default;
    string[,] texts;

    int numberOfChapters = 0;
    int curIndex = 0;
    int curSubIndex = 0;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.transform);

        // 초기화
        string allText = tutorialTexts.text;
        string[] chapters = allText.Split('#');
        numberOfChapters = chapters.Length - 1;

        Object[] images = Resources.LoadAll("Sprites/Tutorials", typeof(Sprite));
        tutorialImages = new Sprite[images.Length];
        for (int i = 0; i < images.Length; i++)
        {
            tutorialImages[i] = images[i] as Sprite;
        }

        for (int i = 0; i < numberOfChapters; i++)
        {
            GameObject temp = Instantiate(buttonPrefab, view.transform.Find("Footer"));
            temp.GetComponentInChildren<Text>().text = chapters[0].Split('-')[i].Replace("\n", "");
            temp.GetComponent<Button>().OnClickAsObservable()
                .Subscribe(_ =>
                {
                    ShowPanel(temp.transform.GetSiblingIndex());
                });
        }

        // 이미지 및 텍스트 배치
        int imgIndex = 0;
        panels = new GameObject[numberOfChapters];
        for (int i = 0; i < numberOfChapters; i++)
        {
            panels[i] = Instantiate(contentPrefab, view.transform.GetChild(1).GetChild(0).GetChild(0));
            string[] subContents = chapters[i + 1].Split('@');
            for (int j = 0; j < subContents.Length; j++)
            {
                GameObject sub = Instantiate(subContentPrefab, panels[i].transform);
                Image[] iArray = sub.transform.GetComponentsInChildren<Image>();
                if (subContents[j].Contains("[][]"))
                {
                    subContents[j] = subContents[j].Replace("[][]", "");
                    iArray[0].sprite = tutorialImages[imgIndex++];
                    iArray[1].sprite = tutorialImages[imgIndex++];
                }
                else if (subContents[j].Contains("[]"))
                {
                    subContents[j] = subContents[j].Replace("[]", "");
                    iArray[0].sprite = tutorialImages[imgIndex++];
                    iArray[1].gameObject.SetActive(false);
                }
                else
                {
                    iArray[0].transform.parent.gameObject.SetActive(false);
                }

                sub.transform.Find("Text").GetComponent<Text>().text = subContents[j];
            }
        }

        ShowPanel(0);

        // bind
        view.transform.GetChild(0).Find("ExitButton").GetComponent<Button>()
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                view.gameObject.SetActive(false);
                view.transform.parent.GetComponent<Canvas>().sortingOrder = 0;
            });

        prevButton.OnClickAsObservable()
            .Where(_ => prevButton.interactable && curSubIndex > 0)
            .Subscribe(_ =>
            {
                curSubIndex--;
                ShowSubPanel(curSubIndex);
            });

        nextButton.OnClickAsObservable()
            .Where(_ => nextButton.interactable)
            .Subscribe(_ =>
            {
                curSubIndex++;
                ShowSubPanel(curSubIndex);
            });

        tutorialButton.OnClickAsObservable()
           .Subscribe(_ =>
           {
               view.gameObject.SetActive(!view.gameObject.activeSelf);
               if (!view.gameObject.activeSelf) return;
               this.GetComponent<Canvas>().sortingOrder = 1;
               ShowPanel(0);
           });
    }
    
    /// <summary>
    /// 해당 챕터를 표시하는 함수.
    /// 버튼을 클릭했을 때 해당 챕터의 내용을 표시할 때 사용.
    /// </summary>
    /// <param name="_index">표시할 챕터 인덱스</param>
    public void ShowPanel(int _index)
    {
        curIndex = _index;
        foreach (var panel in panels)
        {
            panel.SetActive(false);
        }
        panels[_index].SetActive(true);
        curSubIndex = 0;
        ShowSubPanel(0);
        nextButton.interactable = panels[_index].transform.childCount < 2 ? false : true;
        prevButton.interactable = false;
    }

    /// <summary>
    /// 해당 sub 챕터(content) 표시하는 함수.
    /// </summary>
    /// <param name="_index">표시할 sub 챕터 인덱스</param>
    private void ShowSubPanel(int _index)
    {
        nextButton.interactable = curSubIndex < panels[curIndex].transform.childCount - 1 ? true : false;
        prevButton.interactable = curSubIndex > 0 ? true : false;
        for (int i = 0; i < panels[curIndex].transform.childCount; i++)
        {
            panels[curIndex].transform.GetChild(i).gameObject.SetActive(false);
        }
        panels[curIndex].transform.GetChild(_index).gameObject.SetActive(true);
    }

}