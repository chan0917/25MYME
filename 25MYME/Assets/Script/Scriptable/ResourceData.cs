using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Header("�⺻ ����")]
    public string resourceName;
    public ResourceType type;
    public Sprite icon;
    public string description;

    [Header("���� �� ����")]
    public int maxStackSize = 999999;
    public bool canTrade = true;

    [Header("UI ǥ��")]
    public Color displayColor = Color.white;
    public string displayFormat = "{0:N0}"; // ���� ǥ�� ����
}