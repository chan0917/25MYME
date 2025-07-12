using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public SpaceshipLevelManager levelManager;
    public ResourceManager resourceManager;
    public Animator UIAnimator;

    public List<LevelUpMaterial> levelUpMaterials;

    public TMP_Text RequirementText;

    public ResourceType SclapMetal = ResourceType.SclapMetal;

    public TMP_Text sclapMetalText;
    

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
    }

    public void GetNextLevelRequirements()
    {
        levelUpMaterials = levelManager.GetNextLevelRequirements();

        foreach (var material in levelUpMaterials)
        {
            if (material.materialType == SclapMetal)
            {
                RequirementText.text = $"« ø‰ ∞Ì√∂: {material.requiredAmount}";
            }
        }

        sclapMetalText.text = $"∞Ì√∂: {resourceManager.GetResourceAmount(SclapMetal)}";
    }

    public void OnUpdateResource()
    {
        sclapMetalText.text = $"∞Ì√∂: {resourceManager.GetResourceAmount(SclapMetal)}";
    }

    public void TryLevelUp()
    {
        levelManager.TryLevelUp();
    }
}
