using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    #region Properties

    public static LocalizationManager Instance { get; private set; }

    #endregion

    #region Fields

    private string currentLanguage;

    private Dictionary<string, Dictionary<string, string>> localizationDictionary;

    #endregion

    #region Unity Methods

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        localizationDictionary = new Dictionary<string, Dictionary<string, string>>();

        //Components
        AddLocalization("Menu.File", "Arquivo", "File");
        AddLocalization("Menu.Edit", "Editar", "Edit");

        currentLanguage = PlayerPrefs.GetString("currentLanguage", localizationDictionary.Keys.First());
    }

    #endregion

    #region Private Methods

    private void AddLocalization(string key, string ptTranslation, string enTranslation)
    {
        //PT-BR
        var pt_br = "pt-br";
        if (!localizationDictionary.ContainsKey(pt_br))
            localizationDictionary.Add(pt_br, new Dictionary<string, string>());

        if (!localizationDictionary[pt_br].ContainsKey(key))
            localizationDictionary[pt_br].Add(key, ptTranslation);

        //EN-US
        var en_us = "en-us";
        if (!localizationDictionary.ContainsKey(en_us))
            localizationDictionary.Add(en_us, new Dictionary<string, string>());

        if (!localizationDictionary[en_us].ContainsKey(key))
            localizationDictionary[en_us].Add(key, enTranslation);
    }

    #endregion

    #region Public Methods

    public string Localize(string key)
    {
        return localizationDictionary[currentLanguage][key];
    }

    #endregion
}
