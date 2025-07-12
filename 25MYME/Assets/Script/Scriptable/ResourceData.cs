using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Header("기본 정보")]
    public ResourceType type;
    public string resourceName;
    public string description;

    [Header("비주얼")]
    public Sprite icon;
    public Color displayColor = Color.white;

    [Header("게임 내 설정")]
    public int maxStackSize = 999999;
    public bool canTrade = true;

    [Header("UI 표시")]
    public string displayFormat = "{0:N0}"; // 숫자 표시 형식

    [Header("효과 (특수 아이템용)")]
    public float effectValue = 0f; // 보호석, 행운의 부적 등의 효과값
}