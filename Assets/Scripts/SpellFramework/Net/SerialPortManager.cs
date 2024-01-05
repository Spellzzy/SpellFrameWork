using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Threading;
using System.Text;

public class SerialPortManager : MonoBehaviour
{
    private SerialPort _serialPort;
    // 串口名称
    private string _portName = "COM1";
    // 串口波特率
    private int _baudRate = 9600;
    // 校验位
    private Parity _parity = Parity.None;
    // 数据位
    private int _dataBits = 8;
    // 停止位
    private StopBits _stopBits = StopBits.One;
    // 握手协议
    private Handshake _handshake = Handshake.None;
    private bool _rtsEnable = true;
    private bool _dtrEnable = true;

    // 接收数据线程
    Thread dataReceiveThread;


    private void Start()
    {
        GetPorts();
        OpenPort();
        dataReceiveThread = new Thread(new ThreadStart(ReceiveData));
        dataReceiveThread.IsBackground = true;
        dataReceiveThread.Start();
    }

    private void OnApplicationQuit  ()
    {
        ClosePort();
    }

    public void GetPorts()
    {
        string[] ports = SerialPort.GetPortNames();
        foreach (var item in ports)
        {
            Debug.Log(item);
        }
    }


    // 创建串口 并且打开串口
    public void OpenPort()    
    {
        _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits);
        _serialPort.Handshake = _handshake;
        _serialPort.RtsEnable = _rtsEnable;
        _serialPort.DtrEnable = _dtrEnable;
        _serialPort.ReadTimeout = 5000;
        _serialPort.WriteTimeout = 5000;
        try
        {
            _serialPort.Open();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public void ClosePort()
    {
        try
        {
            _serialPort.Close();
        }
        catch (System.Exception)
        {
            
            throw;
        }
    }

    public void SendData(string data)
    {
        if(_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Write(data);
        }
        else
        {
            Debug.LogError("串口未打开");
        }
    }
    public void ReceiveData()
    {
        byte[] buffer = new byte[1024];
        int bytes = 0;
        while (true)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try 
                {
                    bytes = _serialPort.Read(buffer, 0, buffer.Length);
                    if(bytes > 0 )
                    {
                        string strBytes = Encoding.Default.GetString(buffer);
                        Debug.Log(strBytes);
                    }
                }
                catch (System.Exception e)
                {
                    if(e.GetType() != typeof(ThreadAbortException))
                    {
                        Debug.LogError(e.Message);
                    }
                }
            }
            Thread.Sleep(10);
        }        
    }


    string message = "";
     void OnGUI()
    {
        message = GUILayout.TextField(message);
        if (GUILayout.Button("Send Input"))
        {
            SendData(message);
        }
        string test = "AA BB 01 12345 01AB 0@ab 发送";//测试字符串
        if (GUILayout.Button("Send Test"))
        {
            SendData(test);
        }
    }

}
