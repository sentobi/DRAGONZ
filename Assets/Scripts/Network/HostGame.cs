﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.UI;

public class HostGame : NetworkBehaviour {

    [SerializeField]
    private uint roomSize = 16; // not working, use the one under NetworkManager

    public Text createRoomErrorText;
    public Text joinLANGameText;

    private string roomName;
    private string ipAddress;
    public Toggle toggleLAN;
    public Toggle toggleOnline;
    private bool canCreate;

    private bool clickedJoinLAN;
    private float LANTimer;

    private NetworkManager networkManager;

    void Start()
    {
        networkManager = NetworkManager.singleton;
        canCreate = false;
        clickedJoinLAN = false;
        LANTimer = 0.0f;
        ipAddress = "";

        if (createRoomErrorText != null)
            createRoomErrorText.text = "";

        NetworkServer.Reset();

        if (networkManager.matchMaker == null)
        {
            networkManager.StartMatchMaker();
        }
    }

    public void SetRoomName(string _name)
    {
        roomName = _name;
    }

    public void SetIPAddress(string _ipAddress)
    {
        ipAddress = _ipAddress;
    }

    public void SetLAN()
    {
        //if (!toggleLAN.isOn)
        //    toggleLAN.isOn = true;
        //else
        //    toggleLAN.isOn = false;

        if (toggleOnline.isOn)
            toggleOnline.isOn = false;
    }

    public void SetOnline()
    {
        //if (!toggleOnline.isOn)
        //    toggleOnline.isOn = true;
        //else
        //    toggleOnline.isOn = false;

        if (toggleLAN.isOn)
            toggleLAN.isOn = false;
    }

    public void CreateRoom()
    {
        if (!toggleLAN.isOn && !toggleOnline.isOn)
        {
            canCreate = false;
            createRoomErrorText.text = "Select game type";
            return;
        }

        if (roomName == "" || roomName == null)
        {
            canCreate = false;
            createRoomErrorText.text = "Room name cannot be empty";
            return;
        }

        canCreate = true;

        if (canCreate)
        {
            PlayerSetup.isHost = true;

            if (toggleLAN.isOn)
            {
                Debug.Log("Creating LAN Room: " + roomName + " Size: " + roomSize);
                networkManager.networkAddress = Network.player.ipAddress; // localhost if host is Unity editor
                networkManager.StartHost();
            }
            else if (toggleOnline.isOn)
            {
                Debug.Log("Creating online Room: " + roomName + " Size: " + roomSize);
                networkManager.matchMaker.CreateMatch(roomName, roomSize, true, "", networkManager.OnMatchCreate);
            }
        }
    }

    void Update()
    {
        if (clickedJoinLAN)
        {
            if (ipAddress == "")
            {
                joinLANGameText.text = "IP Address cannot be empty";
                //ipAddress = "localhost";
                clickedJoinLAN = false;
                return;
            }

            LANTimer += Time.deltaTime;
            joinLANGameText.text = "Searching for LAN room";

            if (LANTimer > 1.0f)
            {
                if (networkManager.client.isConnected == false)
                {
                    joinLANGameText.text = "No LAN room found";
                    //networkManager.StartClient().Shutdown();
                    networkManager.StopClient();
                    networkManager.StopHost();
                    networkManager.StopMatchMaker();
                    Network.Disconnect();
                }

                LANTimer = 0.0f;
                clickedJoinLAN = false;
            }
        }
    }

    public void JoinLANRoom()
    {
        networkManager.networkAddress = ipAddress;
        networkManager.StartClient();
        clickedJoinLAN = true;
    }
}
