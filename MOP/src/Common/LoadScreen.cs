// Modern Optimization Plugin
// Copyright(C) 2019-2021 Athlon

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.

using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace MOP.Common
{
    class LoadScreen : MonoBehaviour
    {
        GameObject loadScreen;
        PlayMakerFSM cursorFSM;
        bool doDisplay;

        void Start()
        {
#if PRO
            loadScreen = ModUI.GetCanvas().transform.Find("ModLoaderUI/ModLoadScreen").gameObject;
            loadScreen.transform.Find("TextHolder/Text").gameObject.GetComponent<Text>().text = LoadText;
#else
            loadScreen = ModUI.GetCanvas().transform.Find("MSCLoader loading screen").gameObject;
            loadScreen.transform.Find("ModName").gameObject.GetComponent<Text>().text = LoadText;
            loadScreen.transform.Find("Loading").gameObject.SetActive(false);

#endif
            Cursor.visible = false;
            cursorFSM = GameObject.Find("PLAYER").GetPlayMakerByName("Update Cursor");
            cursorFSM.enabled = false;
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
            this.enabled = true;
            doDisplay = true;
        }

        public void Deactivate()
        {
            doDisplay = false;
            loadScreen.SetActive(false);
            cursorFSM.enabled = true;
            this.enabled = false;
        }

        string LoadText { get => Random.Range(0, 100) == 0 ? "HAVE A NICE DAY :)" : "LOADING\nMODERN OPTIMIZATION PLUGIN"; }
    }
}
