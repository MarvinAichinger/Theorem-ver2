using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using System;

public class SocketManager : MonoBehaviour
{

    public SocketIOUnity socket;

    void Start()
    {

        //DontDestroyOnLoad(this.gameObject);

        var uri = new Uri("http://172.17.46.24:3000");
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
                {
                    {"token", "UNITY" }
                }
            ,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });

        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("socket.OnConnected");
        };

        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("socket.OnDisconnect");
        };

        socket.On("gameRoomID", (response) =>
        {
            Debug.Log(response.GetValue());
        });

        socket.On("startGame", (response) =>
        {
            Debug.Log(response.GetValue<bool>());
            bool starting = response.GetValue<bool>();
            if (starting)
            {
                GameObject.Find("Allert").GetComponent<Allert>().allert("You start", 2f);
            }
            else
            {
                GameObject.Find("PlayerStats").GetComponent<PlayerStats>().setGameStatus("wait");
                GameObject.Find("Allert").GetComponent<Allert>().allert("Enemy starts", 2f);
            }
        });

        Debug.Log("Connecting...");
        socket.Connect();
    }

}
