using UnityEngine;

namespace Assets.Scripts.Managers
{
    public class SimulationManager : MonoBehaviour
    {
        #region Properties

        public static SimulationManager Instance { get; private set; }

        #endregion

        #region Fields

        private bool isSimulating;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isSimulating = !isSimulating;
            }
        }

        #endregion

        #region Public Methods

        public bool IsSimulating()
        {
            return isSimulating;
        }

        #endregion
    }
}
