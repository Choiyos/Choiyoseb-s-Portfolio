using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SceneInitializer : MonoBehaviour
{
    // 모든 씬의 초기화를 담당하는 스크립트.

    private static SceneInitializer sceneInitializer = null;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (sceneInitializer == null)
        {
            sceneInitializer = this;
        }
        else if (sceneInitializer != this)
            Destroy(gameObject);
    }

    public void SceneChanged()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
            SceneManager.sceneLoaded += OnLevelFinishedLoading;

    }

    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "TitleScene":
                TitleSceneInit();
                break;
            case "LoadingScene":
                StartCoroutine(LoadingSceneInit());
                break;
            case "IngameScene":
                IngameSceneInit();
                break;
            case "GameoverScene":
                StartCoroutine(GameoverSceneInit());
                break;
        }
    }

    IEnumerator GoToScene(int num)
    {
        // num을 매개변수로 받아 해당 씬으로 이동하는 코루틴
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene(num);
    }

    void TitleSceneInit()
    {
        // TitleScene 초기화

        // 변수 초기화
        GameObject canvas = GameObject.Find("Canvas");
        GameObject fade = canvas.transform.GetChild(2).gameObject;

        // 리소스 매니저 초기화
        if (!GetComponent<ResourceManager>())
            gameObject.AddComponent<ResourceManager>();

        // 화면 초기화
        Screen.orientation = ScreenOrientation.LandscapeRight;

        // 버튼 초기화
        canvas.transform.Find("StartButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            fade.SetActive(true);
            fade.GetComponent<Image>().DOFade(1, 2);
            StartCoroutine(GoToScene(1));
        }
        );

        // 오디오 재생
        AudioManager.Instance.PlayMainSound("BGM001");
    }

    IEnumerator LoadingSceneInit()
    {
        // LoadingScene 초기화
        // 컴포넌트 초기화
        if (!GetComponent<ResourceManager>())
            gameObject.AddComponent<ResourceManager>();
        yield return new WaitForSeconds(1);
        StartCoroutine(GoToScene(2));
    }

    void IngameSceneInit()
    {
        // IngameScene 초기화

        // 변수 초기화
        GameObject touchCanvas = GameObject.Find("Canvas").transform.GetChild(0).gameObject;
        GameObject screenCanvas = GameObject.Find("ScreenCanvas");
        GameObject menuWindow = touchCanvas.transform.GetChild(3).gameObject;
        GameObject fade = screenCanvas.transform.GetChild(0).gameObject;

        // 컴포넌트 초기화
        if (!GetComponent<IngameSceneButtonCallback>())
        {
            gameObject.AddComponent<IngameSceneButtonCallback>();
            GetComponent<IngameSceneButtonCallback>().Init();
        }
        if (!GetComponent<OptionInitializer>())
            gameObject.AddComponent<OptionInitializer>();
        GetComponent<OptionInitializer>().Init(menuWindow.transform); // 메뉴 창 세팅
        if (!GetComponent<ResourceManager>())
            gameObject.AddComponent<ResourceManager>();
        if (!GetComponent<UseItemFunctionList>())
            gameObject.AddComponent<UseItemFunctionList>();

        // 화면 초기화
        Screen.orientation = ScreenOrientation.LandscapeRight;

        // 버튼 초기화
        menuWindow.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
        {
            print("게임 정보 출력");
        });
        menuWindow.transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.Quit();
        });

        // 오디오 재생
        AudioManager.Instance.PlayMainSound("BGM001");


        StartCoroutine(StartIngame(touchCanvas, fade));
    }

    IEnumerator StartIngame(GameObject touchCanvas, GameObject fade)
    {
        Text directText = fade.transform.GetChild(0).GetComponent<Text>();
        directText.text = "알 수 없는 사태가 일어나고,\n학교 안의 사람들은 좀비가 되어 간다...";
        directText.DOFade(1, 2);
        yield return new WaitForSeconds(2);
        directText.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        directText.text = "나는 선생님으로서,\n학교에 갇힌 7명의 아이들을 구할 것이다.";
        directText.DOFade(1, 2);
        yield return new WaitForSeconds(2);
        directText.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        fade.GetComponent<Image>().DOFade(0, 2);
        yield return new WaitForSeconds(2);
        yield return null;
        touchCanvas.SetActive(true);
        fade.SetActive(false);
    }

    public void GameOver() // 죽었을 경우 호출되는 메소드
    {
        StartCoroutine(GoToScene(3));
    }

    IEnumerator GameoverSceneInit()
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject fade = canvas.transform.GetChild(5).gameObject;
        GameObject retryButton = canvas.transform.Find("RetryButton").gameObject;
        GameObject quitButton = canvas.transform.Find("QuitButton").gameObject;
        retryButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            fade.SetActive(true);
            fade.GetComponent<Image>().DOFade(1, 2);
            StartCoroutine(GoToScene(2));
        });
        quitButton.GetComponent<Button>().onClick.AddListener(() => Application.Quit());
        fade.GetComponent<Image>().DOFade(0, 2);
        yield return new WaitForSeconds(2);
        fade.SetActive(false);
    }

    public void GameClear() // 게임을 클리어했을 때 호출되는 메소드
    {
        StartCoroutine(GameClear_(GameObject.Find("ScreenCanvas").transform.GetChild(0).gameObject));
    }

    IEnumerator GameClear_(GameObject fade)
    {
        GameObject.Find("TouchCanvas").SetActive(false);
        fade.SetActive(true);
        fade.GetComponent<Image>().DOFade(1, 2);
        yield return new WaitForSeconds(2);
        Text directText = fade.transform.GetChild(0).GetComponent<Text>();
        directText.text = "모든 아이들을 구출하고, 탈출에 성공했다...!";
        directText.DOFade(1, 2);
        yield return new WaitForSeconds(2);
        directText.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        directText.text = "The End...";
        directText.DOFade(1, 2);
        yield return new WaitForSeconds(2);
        directText.DOFade(0, 2);
        yield return new WaitForSeconds(2);
        StartCoroutine(GoToScene(0));
    }
}
