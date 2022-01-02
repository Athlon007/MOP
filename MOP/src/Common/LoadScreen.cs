// Modern Optimization Plugin
// Copyright(C) 2019-2022 Athlon

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

using UnityEngine.UI;
using MOP.FSM;
using UnityEngine;

namespace MOP.Common
{
    class LoadScreen : MonoBehaviour
    {
        GameObject canvas;
        GameObject loadScreen;
        PlayMakerFSM cursorFSM;
        bool doDisplay;

        Text status;

        void Start()
        {
            canvas = GameObject.Instantiate(GameObject.Find("MSCLoader Canvas loading"));
            canvas.name = "MOP_Canvas";
            Destroy(canvas.transform.Find("MSCLoader update dialog").gameObject);
            loadScreen = canvas.transform.Find("MSCLoader loading dialog").gameObject;
            loadScreen.name = "LoadScreen";
            
            // Title
            string displayedVersion = MOP.ModVersion;
            displayedVersion = displayedVersion.Replace('-', ' '); 
            displayedVersion = displayedVersion.Replace('_', ' ');
            loadScreen.transform.Find("Title").gameObject.GetComponent<Text>().text = $"MODERN OPTIMIZATION PLUGIN <color=green>{displayedVersion}</color>";

            // Main Window
            Destroy(loadScreen.transform.Find("Loading Container/LoadingStuff/Progress").gameObject);
            Destroy(loadScreen.transform.Find("Loading Container/LoadingStuff/LoadingRow2").gameObject);
            loadScreen.transform.Find("Loading Container/LoadingStuff/LoadingRow1/LoadingTitle").gameObject.GetComponent<Text>().text = LoadText;

            loadScreen.SetActive(true);

            // Disable the cursor.
            Cursor.visible = false;
            cursorFSM = GameObject.Find("PLAYER").GetPlayMaker("Update Cursor");
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

        string LoadText { get => Random.Range(0, 100) == 0 ? "HAVE A NICE DAY :)" : "LOADING"; }
    }
}
