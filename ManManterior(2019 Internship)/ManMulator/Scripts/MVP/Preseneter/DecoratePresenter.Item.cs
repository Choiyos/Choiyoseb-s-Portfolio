using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 선택 모드 정보. 
/// 오브젝트 선택시 해당 오브젝트를 선택만 했는지, 편집중인지, 조건에 안맞는 상태인지 구분.
/// </summary>
public enum SelectMode
{
    Select, Edit, NotAble
}
/// <summary>
/// 항목 또는 오브젝트의 타입(오브젝트 타입과 가구 적용 타입에 혼용해서 사용중. 추후 분리 필요.)
/// </summary>
public enum DecorateType
{
    None, Floor, Wall, Ceiling, Furniture, Door, Window
}

/// <summary>
/// 꾸미기 항목(가구/바닥재/벽지) 정보를 담을 클래스.
/// </summary>
[Serializable]
public class Item
{
    public enum Type { Material, Furniture }
    string code;
    string name;
    int cost;
    Sprite sprite;
    string description;
    public Type type;
    private DecorateType subType;
    GameObject icon;
    GameObject furniture;
    Material material;

    public string Code { get => code; set => code = value; }
    public string Name { get => name; set => name = value; }
    public int Cost { get => cost; set => cost = value; }
    public GameObject Icon { get => icon; set => icon = value; }
    public Material Material { get => material; set => material = value; }
    public GameObject Furniture { get => furniture; set => furniture = value; }
    public DecorateType SubType { get => subType; set => subType = value; }
    public Sprite Sprite { get => sprite; set => sprite = value; }
    public string Description { get => description; set => description = value; }


    public Item(Type _type, DecorateType _subType)
    {
        type = _type;
        SubType = _subType;
    }

    /// <summary>
    /// 해당 항목이 재질일 경우 정보 등록
    /// </summary>
    /// <param name="_code">코드</param>
    /// <param name="_name">이름</param>
    /// <param name="_cost">가격</param>
    /// <param name="_icon">아이콘 오브젝트</param>
    /// <param name="_sprite">아이콘 이미지</param>
    /// <param name="_material">재질</param>
    /// <param name="_desc">상세정보</param>
    public void Set(string _code, string _name, int _cost, GameObject _icon, Sprite _sprite, Material _material, string _desc)
    {
        Code = _code;
        Name = _name;
        Cost = _cost;
        Icon = _icon;
        Sprite = _sprite;
        Material = _material;
        Description = _desc;
    }
    /// <summary>
    /// 해당 항목이 가구일 경우 정보 등록
    /// </summary>
    /// <param name="_code">코드</param>
    /// <param name="_name">이름</param>
    /// <param name="_cost">가격</param>
    /// <param name="_icon">아이콘 오브젝트</param>
    /// <param name="_sprite">아이콘 이미지</param>
    /// <param name="_furniture">가구 프리팹</param>
    /// <param name="_desc">상세정보</param>
    public void Set(string _code, string _name, int _cost, GameObject _icon, Sprite _sprite, GameObject _furniture, string _desc)
    {
        Code = _code;
        Name = _name;
        Cost = _cost;
        Icon = _icon;
        Sprite = _sprite;
        Furniture = _furniture;
        Description = _desc;
    }
    /// <summary>
    /// 해당 항목 활성화 토글 처리
    /// </summary>
    /// <param name="on">선택 토글 값</param>
    public void Select(bool on)
    {
        if (icon == null) return;
        icon.GetComponent<Image>().color = on ? Color.yellow : Color.white;
    }

}