using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class NetManager : MonoBehaviour
{
    public string IP;
    public int Port;

    private Socket sender;

    private byte[] bt;
    void Start()
    {
        bt = new byte[1024];
        DoConnect();
    }

    private void Update()
    {
        if (sender != null && sender.Connected)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                DoSendTest();
            }
        }
    }

    void DoConnect()
    {
        sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress ipAddress = IPAddress.Parse(IP);
        IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, Port);
        sender.Connect(ipEndPoint);


        Thread receiveThread = new Thread(new ThreadStart(DoReceive));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    void DoSendTest()
    {
        string message = "Hello!!!";
        byte[] msg = Encoding.UTF8.GetBytes(message);
        int byteSent = sender.Send(msg);
        
        Debug.Log("Sent: " + byteSent);
    }

    void DoReceive()
    {
        while (true)
        {
            int receiveCount = sender.Receive(bt);
            if (receiveCount > 0)
            {
                Debug.Log("Receive: " + Encoding.UTF8.GetString(bt, 0, receiveCount));
            }
        }
    }

    void Download()
    {
        UnityWebRequest.Get("");
    }

}
