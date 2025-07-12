using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
public class ResourceData : ScriptableObject
{
    [Header("�⺻ ����")]
    public ResourceType type;
    public string resourceName;
    public string description;

    [Header("���־�")]
    public Sprite icon;
    public Color displayColor = Color.white;

    [Header("���� �� ����")]
    public int maxStackSize = 999999;
    public bool canTrade = true;

    [Header("UI ǥ��")]
    public string displayFormat = "{0:N0}"; // ���� ǥ�� ����

    [Header("ȿ�� (Ư�� �����ۿ�)")]
    public float effectValue = 0f; // ��ȣ��, ����� ���� ���� ȿ����
}