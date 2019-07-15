using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerInteraction : MonoBehaviour
{
    SceneEventSystem sceneEventSystem;
    [SerializeField]
    Button interactionButton;
    [SerializeField]
    Item equipedItem; // 장착중인 장비 아이템
    GameObject lookDir; // 플레이어가 바라보고 있는 방향을 가리키는 오브젝트 변수.
    PlayerProperty property;
    bool isAlive;
    List<Collider> targets;
    GameObject currentTarget;

    public Item EquipedItem
    {
        get { return equipedItem; }
        set { equipedItem = value; }
    }

    void Start()
    {
        equipedItem = null;
        property = GetComponent<PlayerProperty>();
        lookDir = transform.Find("LookDir").gameObject;
        isAlive = true;
        targets = new List<Collider>();
        StartCoroutine(SearchInteractive());
    }

    IEnumerator SearchInteractive()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate();
        while (true)
        {
            yield return wait;
            GameObject target = FindInteractiveObject();
            currentTarget = target;
            if (target != null)
            {
                interactionButton.transform.position = Camera.main.WorldToScreenPoint(currentTarget.transform.position);
                interactionButton.gameObject.SetActive(true);
            }
            else
            {
                currentTarget = null;
                interactionButton.gameObject.SetActive(false);
            }
        }
    }

    public void ButtonDown()
    {
        interactionButton.transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ButtonUp()
    {
        if (currentTarget != null && currentTarget.CompareTag("BehavioralObject"))
        {
            interactionButton.transform.GetChild(0).gameObject.SetActive(false);
            currentTarget.SendMessage("BehaviorByInteraction", gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void BeAttacked(GameObject enemy)
    {
        property.HealthPoint -= enemy.GetComponent<EnemyProperty>().Damage;
        property.HealthPoint = Mathf.Clamp(property.HealthPoint, 0, int.MaxValue);
        if (property.HealthPoint <= 0)
        {
            StartCoroutine(Dead());
        }
    }

    GameObject FindInteractiveObject()
    {
        targets.Clear();
        targets.AddRange(Physics.OverlapSphere(lookDir.transform.position, property.InteractionRange, 1 << 8)); // 범위 내에 레이어가 Interactive인 오브젝트 탐색
        FindObjectInRange(targets);
        return CalculateNeariestObject(targets);
    }

    void FindObjectInRange(List<Collider> targets) // 시야각 내에 있는 오브젝트들만 반환 (시야각에서 벗어난 오브젝트들은 제외)
    {
        List<Collider> copy_targets = new List<Collider>();
        copy_targets.AddRange(targets);

        for (int i = 0; i < copy_targets.Count; i++)
        {
            Vector3 dir = (copy_targets[i].transform.position - lookDir.transform.position).normalized;
            Vector3 forward = lookDir.transform.forward.normalized;
            float angle = Mathf.Acos(dir.x * forward.x + dir.y * forward.y + dir.z * forward.z) * 180 / Mathf.PI;
            if (angle > property.InteractionRadius)
            {
                targets.Remove(copy_targets[i]);
            }
        }
    }

    GameObject CalculateNeariestObject(List<Collider> targets) // 가장 가까운 오브젝트 탐색해 반환
    {
        int minIdx = -1;
        float minDist = float.MaxValue;
        for (int i = 0; i < targets.Count; i++)
        {
            float temp = (transform.position - targets[i].transform.position).sqrMagnitude;
            if ((temp <= minDist))
            {
                minIdx = i;
                minDist = temp;
            }
        }
        return minIdx == -1 ? null : targets[minIdx].gameObject;
    }

    IEnumerator Dead()
    {
        if (isAlive)
        {
            isAlive = false;
            GameObject fade = GameObject.Find("ScreenCanvas").transform.GetChild(0).gameObject;
            sceneEventSystem = GameObject.Find("IngameScene").GetComponent<SceneEventSystem>();
            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                enemy.GetComponent<Enemy>().Player = null;
            }
            transform.GetChild(0).GetComponent<Animator>().SetInteger("State", 3);
            AudioManager.Instance.StopStepSound();
            AudioManager.Instance.PlaySoundOneShot("MaleDeath", 2);
            Camera.main.SendMessage("ShakeCamera");
            GetComponent<PlayerInput>().enabled = false;
            sceneEventSystem.TouchCanvas.SetActive(false);
            fade.SetActive(true);
            fade.GetComponent<Image>().DOFade(1, 2);
            yield return new WaitForSeconds(2);
            GameObject.Find("GameManager").SendMessage("GameOver");
            yield return null;
        }
    }
}
