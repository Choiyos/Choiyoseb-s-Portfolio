using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameOverUI : NetworkBehaviour
{
    // 게임의 종료 처리를 위한 스크립트.
    float colorAlpha;
    float durationTime = 0.5f;
    string winner;

    public void PunchGameOver(string name)
    {
        winner = name;
        StartCoroutine("_PunchGameOver");
    }

    IEnumerator _PunchGameOver()
    {
        Transform root = transform.root;

        for (colorAlpha = 0; GetComponent<Image>().color.a <= 0.999f; colorAlpha += Time.deltaTime / durationTime)
        {
            GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
            yield return null;
        }

        if (root.GetComponent<RangedPlayerAnimation>())
        {
            root.GetComponent<RangedPlayerAnimation>().enabled = false;
        }
        else
            root.GetComponent<PlayerAnimation>().enabled = false;

        root.GetComponent<Rigidbody>().useGravity = false;
        transform.root.GetComponent<Animator>().SetFloat("Horizontal", 0);
        transform.root.GetComponent<Animator>().SetFloat("Vertical", 0);
        root.Find("Canvas").Find("Aim").gameObject.SetActive(false);
        root.Find("Canvas").Find("StatusUI").gameObject.SetActive(false);
        root.Find("Canvas").Find("SkillUI").gameObject.SetActive(false);
        root.Find("Canvas").Find("TimeUI").gameObject.SetActive(false);
        root.GetComponent<CameraColliderCheck>().enabled = false;
        root.Find("Main Camera").GetComponent<CameraMove>().enabled = false;
		root.GetComponent<Player> ().enabled = false;
        yield return new WaitForSeconds(0.5f);

        if (root.name == winner)
        {
            root.transform.rotation = Quaternion.Euler(0, 270, 0);
            GameObject.Find("MultiCameraParent").transform.Find("EndingCamera").gameObject.SetActive(true);
            root.Find("Main Camera").gameObject.SetActive(false);
            root.GetComponent<Animator>().SetBool("Ceremoning", true);
            yield return new WaitForSeconds(1.0f);
            root.GetComponent<Animator>().SetBool("Ceremoning", false);
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().text = "Winner is " + winner + " !!!";
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().fontSize = 30;
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().color = new Color(255, 255, 255, 1);
        }
        else
        {
            root.transform.rotation = Quaternion.Euler(0, 270, 0);
            GameObject.Find("MultiCameraParent").transform.Find("EndingCamera").gameObject.SetActive(true);
            root.Find("Main Camera").gameObject.SetActive(false);
            root.GetComponent<Animator>().SetBool("Cheering", true);
            yield return new WaitForSeconds(1.0f);
            root.GetComponent<Animator>().SetBool("Cheering", false);
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().text = "Winner is " + winner + " !!!";
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().fontSize = 50;
            root.transform.Find("Canvas").Find("MessageUI").GetComponent<Text>().color = new Color(255, 255, 255, 1);
        }
        GameObject.Find("Plane").GetComponent<MeshRenderer>().material = Resources.Load("AssetStore/Shadowball Games/Seamless Gold Coins/M_GoldCoins") as Material;

        for (colorAlpha = 1; GetComponent<Image>().color.a >= 0.01f; colorAlpha -= Time.deltaTime / durationTime)
        {
            GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
            yield return null;
        }
        if (root.name == winner)
            GameObject.Find("MultiCameraParent").transform.Find("EndingCamera").GetComponent<AudioSource>().Play();
    }
}
