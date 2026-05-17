using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FoodUnlockUI : MonoBehaviour{
    [Header("UI 引用")]
    public GameObject panelRoot;
    public Button[] optionButtons; // 建议长度为3 
    public TextMeshProUGUI[] optionNameTexts; // 与按钮一一对应（可选）

    private List<FoodData> currentOptions;
    private Action<FoodData> onSelected;

    private void Awake()
    {
        Hide();
    }

    public void ShowUnlockOptions(List<FoodData> options, Action<FoodData> callback)
    {
        currentOptions = options;
        onSelected = callback;

        if (panelRoot != null) panelRoot.SetActive(true);

        for (int i =0; i < optionButtons.Length; i++)
        {
            if (i < options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].onClick.RemoveAllListeners();

                int index = i;
                optionButtons[i].onClick.AddListener(() => Select(index));

                if (optionNameTexts != null && i < optionNameTexts.Length && optionNameTexts[i] != null)
                {
                    optionNameTexts[i].text = options[i].foodName;
                }
            }
            else {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void Select(int index)
    {
        if (currentOptions == null) return;
        if (index <0 || index >= currentOptions.Count) return;

        onSelected?.Invoke(currentOptions[index]);
        Hide();
    }

    private void Hide()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}