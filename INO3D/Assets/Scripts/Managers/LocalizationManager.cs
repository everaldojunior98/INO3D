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
        private float cameraSensitivity;
        private bool showWarnings;

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

            AddLocalization("Menu.Components", "Componentes", "Components");
            AddLocalization("Menu.Properties", "Propriedades", "Properties");

            AddLocalization("Overlay.Port", "Porta", "Port");
            AddLocalization("Overlay.Type", "Tipo", "Type");

            AddLocalization("Overlay.Voltage", "Tensão", "Voltage");
            AddLocalization("Overlay.Current", "Corrente", "Current");

            AddLocalization("Overlay.Type.Analog", "Analógica", "Analog");
            AddLocalization("Overlay.Type.Digital", "Digital", "Digital");
            AddLocalization("Overlay.Type.DigitalPwm", "Digital (PWM)", "Digital (PWM)");
            AddLocalization("Overlay.Type.Power", "Alimentação", "Power");

            AddLocalization("Circuit", "Circuito", "Circuit");
            AddLocalization("Circuit.Basics", "Básicos", "Basics");
            AddLocalization("Arduino", "Arduino", "Arduino");
            AddLocalization("Arduino.Boards", "Placas", "Boards");
            AddLocalization("Ino3D", "Ino3D", "Ino3D");
            AddLocalization("Ino3D.Custom", "Personalizado", "Custom");
            AddLocalization("ArduinoUno", "Arduino Uno", "Arduino Uno");
            AddLocalization("Protoboard400", "Protoboard 400 pontos", "Protoboard 400 points");
            AddLocalization("Resistor", "Resistor", "Resistor");
            AddLocalization("Led", "Led", "Led");
            AddLocalization("PushButton", "Chave táctil", "Push button");
            AddLocalization("BlackBox", "Caixa preta", "Black box");
            AddLocalization("L298N", "L298N", "L298N");
            AddLocalization("Electric", "Elétrico", "Electric");
            AddLocalization("Electric.Basics", "Básicos", "Basics");
            AddLocalization("Electric.Motors", "Motores", "Motors");
            AddLocalization("DCMotor", "Motor DC", "DC Motor");
            AddLocalization("CarChassis", "Chassi 2wd", "Chassis 2wd");
            AddLocalization("Battery", "Bateria", "Battery");

            AddLocalization("Yes", "Sim", "Yes");
            AddLocalization("No", "Não", "No");
            AddLocalization("Cancel", "Cancelar", "Cancel");

            AddLocalization("NewProject", "Novo Projeto", "New Project");
            AddLocalization("OpenProject", "Abrir Projeto", "Open Project");
            AddLocalization("SaveProject", "Salvar Projeto", "Save Project");
            AddLocalization("Ino3DProjectFiles", "Arquivos de Projeto Ino3D", "Ino3D Project Files");
            AddLocalization("UnsavedPopupTitle", "Deseja salvar o projeto?", "Do you want to save the project?");
            AddLocalization("UnsavedPopupMessage", "Há alterações não salvas em", "There are unsaved changes in");

            AddLocalization("Camera2D", "Câmera 2D", "Camera 2D");
            AddLocalization("Camera3D", "Câmera 3D", "Camera 3D");
            AddLocalization("OpenConsole", "Abrir console", "Open console");
            AddLocalization("StartSimulation", "Iniciar a simulação", "Start simulation");
            AddLocalization("PauseSimulation", "Pausar a simulação", "Pause simulation");
            AddLocalization("StopSimulation", "Parar a simulação", "Stop simulation");
            AddLocalization("SimulationTime", "Tempo de simulação", "Simulation time");

            AddLocalization("Console", "Console", "Console");
            AddLocalization("Send", "Enviar", "Send");
            AddLocalization("Auto-scroll", "Auto-rolagem", "Auto-scroll");
            AddLocalization("NoLineEnding", "Sem fim de linha", "No line ending");
            AddLocalization("NewLine", "Nova linha", "New line");
            AddLocalization("Clear", "Limpar", "Clear");

            AddLocalization("Name", "Nome", "Name");
            AddLocalization("Resistance", "Resistência", "Resistance");
            AddLocalization("EditCode", "Editar código", "Edit code");
            AddLocalization("Color", "Cor", "Color");
            AddLocalization("ColorBlack", "Preto", "Black");
            AddLocalization("ColorBlue", "Azul", "Blue");
            AddLocalization("ColorBrown", "Marrom", "Brown");
            AddLocalization("ColorGreen", "Verde", "Green");
            AddLocalization("ColorGray", "Cinza", "Gray");
            AddLocalization("ColorOrange", "Laranja", "Orange");
            AddLocalization("ColorRed", "Vermelho", "Red");
            AddLocalization("ColorTurquoise", "Turquesa", "Turquoise");
            AddLocalization("ColorWhite", "Branco", "White");
            AddLocalization("ColorYellow", "Amarelo", "Yellow");

            AddLocalization("IsRigid", "Rígido", "Rigid");

            AddLocalization("OpenSettings", "Abrir configurações", "Open settings");
            AddLocalization("Save", "Salvar", "Save");
            AddLocalization("Settings", "Configurações", "Settings");
            AddLocalization("Language", "Linguagem", "Language");
            AddLocalization("CameraSensitivity", "Sensibilidade da câmera", "Camera sensitivity");
            AddLocalization("ShowWarnings", "Mostrar avisos", "Show warnings");
            AddLocalization("LockCamera", "Travar câmera em um objeto", "Lock camera on an object");
            AddLocalization("ResetCamera", "Resetar câmera", "Reset camera");

            AddLocalization("CodeEditor", "Editor de código", "Code editor");
            AddLocalization("NodeEditor", "Editor de nós", "Node editor");

            AddLocalization("Nodes", "Nós", "Nodes");
            AddLocalization("Nodes.Duplicate", "Duplicar", "Duplicate");
            AddLocalization("Nodes.Clear", "Limpar conexão", "Clear connection");
            AddLocalization("Nodes.MultiClear", "Limpar conexões", "Clear connections");
            AddLocalization("Nodes.Delete", "Deletar", "Delete");

            //Errors
            AddLocalization("NaN/Infinite matrix!", "Ocorreu um erro ao processar o circuito.", "An error occurred while processing the circuit.");
            AddLocalization("Singular matrix!", "Ocorreu um erro ao processar o circuito.", "An error occurred while processing the circuit.");
            AddLocalization("Convergence failed!", "Ocorreu um erro ao processar o circuito.", "An error occurred while processing the circuit.");
            AddLocalization("No path for current source!", "Nenhum caminho para a fonte de corrente.", "No path for current source.");
            AddLocalization("Voltage source/wire loop with no resistance!", "Fonte de tensão/fio em loop sem resistência.", "Voltage source/wire loop with no resistance.");
            AddLocalization("Capacitor loop with no resistance!", "Capacitor em loop sem resistência.", "Capacitor loop with no resistance.");
            AddLocalization("Matrix error", "Ocorreu um erro ao processar o circuito.", "An error occurred while processing the circuit.");
            AddLocalization("Singular matrix!", "Ocorreu um erro ao processar o circuito.", "An error occurred while processing the circuit.");

            currentLanguage = PlayerPrefs.GetString("currentLanguage", localizationDictionary.Keys.First());
            cameraSensitivity = PlayerPrefs.GetFloat("cameraSensitivity", 1);
            showWarnings = PlayerPrefs.GetInt("showWarnings", 1) == 1;
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

        public string[] GetLanguages()
        {
            return localizationDictionary.Keys.ToArray();
        }

        public int GetCurrentLanguage()
        {
            var languages = GetLanguages();
            for (var i = 0; i < languages.Length; i++)
                if (languages[i] == currentLanguage)
                    return i;
            return 0;
        }
        
        public float GetCameraSensitivity()
        {
            return cameraSensitivity;
        }
        
        public bool GetShowWarnings()
        {
            return showWarnings;
        }

        public void SaveLanguage(string language)
        {
            currentLanguage = language;
            PlayerPrefs.SetString("currentLanguage", language);
            PlayerPrefs.Save();
        }
        
        public void SaveCameraSensitivity(float sensitivity)
        {
            cameraSensitivity = sensitivity;
            PlayerPrefs.SetFloat("cameraSensitivity", cameraSensitivity);
            PlayerPrefs.Save();
        }
        
        public void SaveShowWarnings(bool show)
        {
            showWarnings = show;
            PlayerPrefs.SetInt("showWarnings", showWarnings ? 1 : 0);
            PlayerPrefs.Save();
        }

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