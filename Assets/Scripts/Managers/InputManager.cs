using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TwoDSurvival
{
    // Singleton Manager Template (Mono version)
    [DisallowMultipleComponent]
    public class InputManager : MonoBehaviour
    {

    #region Public

        public static InputManager Instance
        {
            get { return instance; }
        }

    #endregion

    #region Private

        private static InputManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(this);
            else
                instance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
        }

        #endregion

    }
}
