using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MOP.Common
{
    class LoadScreen : MonoBehaviour
    {
        GameObject loadScreen;
        bool doDisplay;

        void Start()
        {
            loadScreen = ModUI.GetCanvas().transform.Find("ModLoaderUI/ModLoadScreen").gameObject;
            loadScreen.transform.Find("TextHolder/Text").gameObject.GetComponent<Text>().text = Random.Range(0, 100) == 0 ? "HAVE A NICE DAY :)" : "LOADING MODERN OPTIMIZATION PLUGIN";
#if !PRO
            loadScreen.transform.Find("MSCLoader loading screen/Loading").gameObject.SetActive(false);
#endif
        }

        void Update()
        {
            if (doDisplay)
            {
                loadScreen.SetActive(true);
            }
        }

        public void Activate()
        {
            doDisplay = true;
        }

        public void Deactivate()
        {
            doDisplay = false;
            loadScreen.SetActive(false);
            Destroy(this);
        }
    }
}
