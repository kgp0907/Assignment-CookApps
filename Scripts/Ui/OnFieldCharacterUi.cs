using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OnFieldCharacterUi : MonoBehaviour
{
    [SerializeField] private Image portrait;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider expSlider;
    public TextMeshProUGUI Level;

    private CharacterUnit linkedCharacter;
    private CharacterData characterData;

    public void LinkCharacter(CharacterUnit character)
    {
        linkedCharacter = character;
        
        SubscribeToCharacterEvents(character);

        characterData = linkedCharacter.Data as CharacterData;
        UpdateLevel();

        portrait.sprite = characterData.Portrait;
        // 체력과 쿨다운 초기화
        healthSlider.maxValue = character.CurrentMaxHealth;
        healthSlider.value = character.CurrentHealth;
        expSlider.maxValue = characterData.CurrentExpRequirement;
        expSlider.value = characterData.Exp;
    }

    private void SubscribeToCharacterEvents(CharacterUnit character)
    {
        character.HpChangeAction -= UpdateHealth;
        character.HpChangeAction += UpdateHealth;
        character.LevelChangeAction-= UpdateLevel;
        character.LevelChangeAction += UpdateLevel;
        character.LevelChangeAction -= UpdateExp;
        character.LevelChangeAction += UpdateExp;
        character.LevelChangeAction -= UpdateHealth;
        character.LevelChangeAction += UpdateHealth;
    }

    public void UpdateLevel()
    {
        Level.text = "Lv."+characterData.Level.ToString();
    }

    private void UpdateHealth()
    {
        healthSlider.value = linkedCharacter.CurrentHealth;
    }

    private void UpdateExp()
    {
        expSlider.value = characterData.Exp;
    }
}
