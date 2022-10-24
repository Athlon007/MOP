﻿// Modern Optimization Plugin
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using MOP.FSM;
using UnityEngine;
using System;

namespace MOP.Common
{
    class LoadScreen : MonoBehaviour
    {
        private readonly Sprite[] frames;
        private readonly Image img;
        private readonly Text loadText;
        private IEnumerator currentLoadingRoutine;

        private readonly PlayMakerFSM cursorFSM;
        private bool doDisplay;

        public LoadScreen()
        {
            cursorFSM = GameObject.Find("PLAYER").GetPlayMaker("Update Cursor");
            frames = GetLoadingIconFrames();
            img = transform.Find("Icon/Frame1").GetComponent<Image>();
            loadText = transform.Find("Text").GetComponent<Text>();

            loadText.text = GetLoadText();
        }

        private IEnumerator LoadingRoutine
        {
            get
            {
                int spriteCount = 0;
                while (true)
                {
                    yield return new WaitForSeconds(.5f);

                    spriteCount++;
                    if (spriteCount >= frames.Length)
                    {
                        spriteCount = 0;
                    }

                    img.sprite = frames[spriteCount];
                }
            }
        }

        private void Update()
        {
            if (doDisplay)
            {
                gameObject.SetActive(true);
            }
        }

        public void Activate()
        {
            if (currentLoadingRoutine != null)
            {
                StopCoroutine(currentLoadingRoutine);
            }
            currentLoadingRoutine = LoadingRoutine;
            StartCoroutine(currentLoadingRoutine);

            this.enabled = true;
            doDisplay = true;

            Cursor.visible = false;
            cursorFSM.enabled = false;
        }

        public void Deactivate()
        {
            doDisplay = false;
            gameObject.SetActive(false);
            cursorFSM.enabled = true;        
            if (currentLoadingRoutine != null)
            {
                StopCoroutine(currentLoadingRoutine);
            }
            this.enabled = false;
        }

        private Sprite[] GetLoadingIconFrames()
        {
            List<Sprite> sprites = new List<Sprite>();
            Transform t = transform.Find("Icon");
            foreach (Image child in t.GetComponentsInChildren<Image>(true))
            {
                sprites.Add(child.sprite);
            }

            return sprites.ToArray();
        }

        private string GetLoadText()
        {
            //if (DateTime.Today.Day == 1 && DateTime.Today.Month == 4)
            {
                string[] foolish = new string[]
                {
                    "MOADING LOP...",
                    "LOADINP MOG...",
                    "...POM GNIDAOL",
                    "LDNG MP...",
                    "WOADING MOP UWU",
                    "MOP?",
                    "UNLOADING MOP...",
                    "ŁADOWANIE MOP...", // Polish
                    "MOP LADEN...", // Dutch
                    "LADEN VON MOP...", // German
                    "MOP-I LAADIMINE...", // Estonian
                    "LADATAAN MOP..." // Finnish
                };
                return foolish[UnityEngine.Random.Range(0, foolish.Length)];
            }

            int random = UnityEngine.Random.Range(0, 100);
            if (random == 0)
            {
                return "HAVE A NICE DAY :)";
            }

            return "LOADING MOP...";
        }
    }
}
