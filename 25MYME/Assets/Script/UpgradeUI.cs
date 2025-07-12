using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public SpaceshipLevelManager levelManager;
    public ResourceManager resourceManager;
    public Animator UIAnimator;

    public List<LevelUpMaterial> levelUpMaterials;
    public SpaceshipLevelData spaceshipLevelData;

    public TMP_Text Requirement1Text;
    public TMP_Text Requirement2Text;
    public TMP_Text Requirement3Text;
    public TMP_Text Requirement4Text;

    public ResourceType SclapMetal = ResourceType.SclapMetal;
    public ResourceType Nickel = ResourceType.Nickel;
    public ResourceType Ascied = ResourceType.Ascied;
    public ResourceType Meteorite = ResourceType.Meteorite;

    public TMP_Text sclapMetalText;
    public TMP_Text NickelText;
    public TMP_Text AsciedText;
    public TMP_Text MeteoriteText;

    public TMP_Text currentLevel;
    public Image nextShip;

    private void Start()
    {
        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();

        levelManager = FindAnyObjectByType<SpaceshipLevelManager>();
    }

    public void OpenUI()
    {
        UIAnimator.Play("OpenUI");
        GetNextLevelRequirements();
    }

    //CloseBtn
    public void CloseUI()
    {
        UIAnimator.Play("CloseUI");
        FindAnyObjectByType<SpaceshipController>().Move(Vector3.left, 8f);
    }

    public void GetNextLevelRequirements()
    {
        spaceshipLevelData = levelManager.GetNextLevelData();
        levelUpMaterials = levelManager.GetNextLevelRequirements();

        nextShip.sprite = spaceshipLevelData.levelIcon;
        currentLevel.text = spaceshipLevelData.level.ToString();

        foreach (var material in levelUpMaterials)
        {
            if (material.materialType == SclapMetal)
            {
                Requirement1Text.text = $"{material.requiredAmount}";
            }

            if (material.materialType == Nickel)
            {
                Requirement2Text.text = $"{material.requiredAmount}";
            }

            if (material.materialType == Ascied)
            {
                Requirement3Text.text = $"{material.requiredAmount}";
            }

            if (material.materialType == Meteorite)
            {
                Requirement4Text.text = $"{material.requiredAmount}";
            }
        }

        sclapMetalText.text = $"°íÃ¶: {resourceManager.GetResourceAmount(SclapMetal)}";
        NickelText.text = $"´ÏÄ¶: {resourceManager.GetResourceAmount(Nickel)}";
        AsciedText.text = $"¿°¼®: {resourceManager.GetResourceAmount(Ascied)}";
        MeteoriteText.text = $"¿îÃ¶: {resourceManager.GetResourceAmount(Meteorite)}";

    }

    public void OnUpdateResource()
    {
        sclapMetalText.text = $"°íÃ¶: {resourceManager.GetResourceAmount(SclapMetal)}";
        NickelText.text = $"´ÏÄ¶: {resourceManager.GetResourceAmount(Nickel)}";
        AsciedText.text = $"¿°¼®: {resourceManager.GetResourceAmount(Ascied)}";
        MeteoriteText.text = $"¿îÃ¶: {resourceManager.GetResourceAmount(Meteorite)}";
    }

    //upgradeBtn
    public void TryLevelUp()
    {
        levelManager.TryLevelUp();

        sclapMetalText.text = $"°íÃ¶: {resourceManager.GetResourceAmount(SclapMetal)}";
        NickelText.text = $"´ÏÄ¶: {resourceManager.GetResourceAmount(Nickel)}";
        AsciedText.text = $"¿°¼®: {resourceManager.GetResourceAmount(Ascied)}";
        MeteoriteText.text = $"¿îÃ¶: {resourceManager.GetResourceAmount(Meteorite)}";
    }
}
