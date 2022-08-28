using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Managers
{
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

            AddLocalization("Menu.File", "Arquivo", "File");
            AddLocalization("Menu.Edit", "Editar", "Edit");
            AddLocalization("Menu.Components", "Componentes", "Components");
            AddLocalization("Overlay.Port", "Porta", "Port");
            AddLocalization("Overlay.Type", "Tipo", "Type");
            AddLocalization("Overlay.Type.Analog", "Analógica", "Analog");
            AddLocalization("Overlay.Type.Digital", "Digital", "Digital");
            AddLocalization("Overlay.Type.DigitalPwm", "Digital (PWM)", "Digital (PWM)");
            AddLocalization("Overlay.Type.Power", "Alimentação", "Power");
            AddLocalization("Circuit", "Circuito", "Circuit");
            AddLocalization("Circuit.Basics", "Básicos", "Basics");
            AddLocalization("Arduino", "Arduino", "Arduino");
            AddLocalization("Arduino.Boards", "Placas", "Boards");
            AddLocalization("ArduinoUno", "Arduino Uno", "Arduino Uno");
            AddLocalization("Protoboard400", "Protoboard 400 pontos", "Protoboard 400 points");
            AddLocalization("Resistor", "Resistor", "Resistor");

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
            try
            {
                return localizationDictionary[currentLanguage][key];
            }
            catch
            {
                Debug.Log(key);
                return key;
            }
        }

        #endregion
    }
}