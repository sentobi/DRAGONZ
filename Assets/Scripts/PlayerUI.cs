﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    GameObject pauseMenu;

    [SerializeField]
    Radar radar;

    public Image energyBar;
    public Image chargeEnergyBar;
    public Image healthBar;
    public Image comboCounterTimer;
    public Text comboCounterText;
    public static Vector3 playerPos;

    void Start()
    {
        PauseMenu.isOn = false;
    }
	
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
	}

    void TogglePauseMenu()
    {
        pauseMenu.SetActive(!pauseMenu.activeSelf);
        PauseMenu.isOn = pauseMenu.activeSelf;
    }
}
