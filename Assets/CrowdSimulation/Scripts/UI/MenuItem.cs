﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.CrowdSimulation.Scripts.UI
{
    public class MenuItem : MonoBehaviour
    {
        public GameObject menuWindow;
        public GameObject backgroundBlur;

        public bool open = false;

        public void OnClick()
        {
            open = !open;
            menuWindow.SetActive(open);
            backgroundBlur.SetActive(open);
        }
    }
}
