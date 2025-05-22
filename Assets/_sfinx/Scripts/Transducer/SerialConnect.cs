using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using UnityEngine;

public class SerialConnect : MonoBehaviour
{
    private static SerialConnect instance;
    public static SerialConnect Instance => instance ? instance : instance = FindObjectOfType<SerialConnect>();
    
    public delegate void SerialEventRaw(string rawData);
    public static event SerialEventRaw OnSendRawSerialData;

    public delegate void SerialStatusEvent(string msg, string color);
    public static event SerialStatusEvent OnSendStatus;
    
    public delegate void SerialEventPayload(TransducerPayload payload);
    public static event SerialEventPayload OnSendSerialPayload;
    TransducerPayload payload;
    
    public int baudrate = 115200;

    private int dataTransferInterval = 1;
    private int serialTimeout = 500;

    private const string GIVE_INPUT = "a";
    private const string COMMAND = "b";
    private const string GETVERSION = "v";
    // handshake commands
    private const string HANDSHAKE_INIT_COMMAND = "c";
    private const string COMMAND_QUIT = "q";
    private const string HANDSHAKE_REPLY = "ARDUINO_1";
    
    private static string commandToSend = "";

    public bool pausedDataTransfer = true;
    
    // Serial
    private SerialPort serialPort = null;
    
    public static List<string> comPorts = new List<string>();
    
    public enum TransducerState { connected, connecting, paused, disabled, error }
    public TransducerState connectionState = TransducerState.disabled;
    
    public bool hotSwappingEnabled = true;
    public float hotSwapIntervalCheck = 5f;
    private float nextHotswapTime = 0;
    private float pingInterval = 2f;
    private float nextPingInterval = 0;
    
    public int maxAutoReconnectTries = 3;
    private int reconnectTries = 0;
    

    void Awake()
    {
        nextPingInterval = pingInterval;
        reconnectTries = 0;
    }
    
    void OnApplicationQuit()
    {
        CloseCurrentConnection();
    }

    public void ConnectToPort(string portName)
    {
        if (serialPort != null)
        {
            OnSendStatus?.Invoke("Tried to open connection on port that is already open", "yellow");
            Debug.LogWarning("There is already a connection open on this port!");
            return;
        }

        connectionState = TransducerState.connecting;
        serialPort = new SerialPort(portName, baudrate, Parity.None, 8, StopBits.One);

        try
        {
            serialPort.DtrEnable = true;
            serialPort.ReadTimeout = serialTimeout;
            serialPort.Open();

            if (serialPort.IsOpen)
            {
                OnSendStatus?.Invoke("Succesfully connected to port " + serialPort.PortName, "green");
                Debug.Log("<color=green>Succesfully connected to port " + serialPort.PortName + "</color>");
                SetDataTransferState(true);
                connectionState = TransducerState.connected;
            }
            else
            {
                OnSendStatus?.Invoke("<color=yellow>Port was not open after connecting to port " + serialPort.PortName, "yellow");
                Debug.LogWarning("<color=yellow>Port was not open after connecting to port " + serialPort.PortName +
                                 "</color>");
                CloseCurrentConnection();
            }
        }
        catch (Exception e)
        {
            OnSendStatus?.Invoke("Exception: " + e, "yellow");
            Console.WriteLine(e);
            connectionState = TransducerState.error;
            throw;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && reconnectTries >= maxAutoReconnectTries)
        {
            reconnectTries = 0;
            nextHotswapTime = Time.time;
        }
        
        if (connectionState == TransducerState.paused)
            return;
        
        if (hotSwappingEnabled && serialPort == null && (reconnectTries < maxAutoReconnectTries))
        {
            if (Time.time > nextHotswapTime)
            {
                OnSendStatus?.Invoke("Automatically trying to connect to Transducer", "yellow");
                
                nextHotswapTime += hotSwapIntervalCheck;
                reconnectTries++;

                Debug.Log($"Reconnect tries left: {maxAutoReconnectTries - reconnectTries}");
                TryHandshake();
            }
        }

        GetTransducerData();
    }

    private void GetTransducerData()
    {
        if (serialPort == null || connectionState != TransducerState.connected)
            return;
        
        try
        {
            // signal that we want to receive values
            serialPort.ReadTimeout = serialTimeout;
            serialPort.WriteTimeout = serialTimeout;
            serialPort.Write(GIVE_INPUT);
            serialPort.BaseStream.Flush();      // Do it now~!

            string incoming = serialPort.ReadLine();
            
            ParseLine(incoming);
        }
        catch (TimeoutException e)
        {
            Debug.Log("Timeout  " + e);
            OnSendStatus?.Invoke("Timeout on receiving data from transducer: " + e, "yellow");
            CloseCurrentConnection(false);
        }
        catch (Exception e)
        {
            Debug.Log("Error " + e);
            CloseCurrentConnection(false);
        }
    }

    private void SetDataTransferState(bool newState)
    {
        pausedDataTransfer = newState;
    }
    
    public void CloseCurrentConnection(bool cleanDisconnect = true)
    {
        if (serialPort == null)
            return;

        OnSendStatus?.Invoke("Closing connection to port " + serialPort.PortName, "white");
        if (!cleanDisconnect)
            OnSendStatus?.Invoke("Disconnection was unexpected", "red");
        
        Debug.Log("Closing connection to port " + serialPort.PortName);
        connectionState = TransducerState.disabled;

        if (cleanDisconnect)
        {
            try
            {
                serialPort.Write(COMMAND + COMMAND_QUIT);
                serialPort.BaseStream.Flush();
                
                OnSendStatus?.Invoke("Successfully closed connection to port " + serialPort.PortName, "green");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        serialPort.Dispose();
        serialPort.Close();

        serialPort = null;
    }

    public void SendCommand(string command)
    {
        if (serialPort == null)
            return;
        
        serialPort.Write(command);
        serialPort.BaseStream.Flush();
    }

    private List<string> GetComPorts()
    {
        List<string> foundPorts = new List<string>();

        int p = (int)System.Environment.OSVersion.Platform;

        // Are we on Unix?
        if (p == 4 || p == 128 || p == 6)
        {
            string[] ttys = System.IO.Directory.GetFiles("/dev/", "tty.*");
            foreach (string dev in ttys)
            {
                if (dev.StartsWith("/dev/tty."))
                {
                    if (dev != "/dev/tty")
                    {
                        foundPorts.Add(dev);
                        Debug.Log(dev.ToString());
                    }
                }
            }
        }
        else
        {   // on Windows
            // Loop through all available ports and add them to the list
            foreach (string cPort in SerialPort.GetPortNames())
            {
                foundPorts.Add(cPort);
                //Debug.Log(cPort);
            }
        }

        return foundPorts;
    }

    public void Pause()
    {
        if (connectionState == TransducerState.connected)
        {
            connectionState = TransducerState.paused;
            OnSendStatus?.Invoke("Paused data transfer of Transducer", "green");
        }
    }

    public void Resume()
    {
        if (connectionState == TransducerState.paused)
        {
            connectionState = TransducerState.connected;
            OnSendStatus?.Invoke("Resumed data transfer of Transducer", "green");
        }
    }
    
    void ParseLine(string strIn)
    {
        // sends the raw string for anyone interested
        if (OnSendRawSerialData != null)
            OnSendRawSerialData(strIn);

        // we send it to those listening
        // right now (26-08-2018) it is [framecounter],[W],[X],[Y],[Z] at this point in code
        if (OnSendSerialPayload != null)
        {
            OnSendSerialPayload(ConvertStringToPayload(strIn));
        }
    }

    private TransducerPayload ConvertStringToPayload(string strIn)
    {
        float[] fa = ConvertStringToFloatArray(strIn);
        ulong frameNum = (ulong)fa[0];
        //fa = new float[4] { fa[1], -fa[3], fa[2], fa[4] };

        Quaternion rot = new Quaternion(fa[1], -fa[3], fa[2], fa[4]);

        // we also add the current mouse position here
        return new TransducerPayload(frameNum, rot, TransducerMovement.ConvertMouseposToNormalisedCoords(Input.mousePosition));
        //return new TransducerPayload(frameNum, fa, MoveScanner.ConvertMouseposToNormalisedCoords(Input.mousePosition));
    }

    private float[] ConvertStringToFloatArray(string strIn)
    {
        // split to float array
        return Array.ConvertAll(strIn.Split(','), float.Parse);
    }

    public void TryHandshake()
    {
        if (connectionState == TransducerState.connecting)
            return;
        
        comPorts = GetComPorts();

        StartCoroutine(StartHandShakeRoutine(HANDSHAKE_INIT_COMMAND, (s) =>
        {
            OnSendStatus?.Invoke("Handshake success on port " + s, "green");
            Debug.Log("Handshake success on port " + s);
            ConnectToPort(s);
        }, (s) =>
        {
            OnSendStatus?.Invoke("Handshake failed on port " + s, "yellow");
            //Debug.Log("Handshake failed on port " + s);
        }));
    }

    private IEnumerator StartHandShakeRoutine(string handshakeString, Action<string> onHandshakeSuccess,
        Action<string> onHandshakeFail)
    {
        foreach (string comPortName in comPorts)
        {
            SerialPort serialPort = null;

            try
            {
                serialPort = new SerialPort(comPortName, baudrate);
                serialPort.DtrEnable = true;
                serialPort.ReadTimeout = serialTimeout;
                serialPort.WriteTimeout = serialTimeout;
                serialPort.Handshake = Handshake.None;
                if (!serialPort.IsOpen)
                {
                    serialPort.Open();
                    //Debug.Log($"Opened port {serialPort.PortName}");
                }
            }
            catch (Exception e)
            {
                OnSendStatus?.Invoke("Connection error while opening port " + comPortName, "red");
                Debug.LogError("Connection error 1: " + e);
                throw;
            }

            if (serialPort != null && serialPort.IsOpen)
            {
                try
                {
                    //Debug.Log($"Sending {COMMAND + handshakeString}");
                    
                    // serialPort.Write(COMMAND + handshakeString);
                    serialPort.WriteLine(COMMAND + handshakeString);
                    serialPort.BaseStream.Flush();
                    
                    string response = serialPort.ReadLine();
                    serialPort.Close();
                    response = response.Trim();

                    if (response.Equals(HANDSHAKE_REPLY))
                    {
                        // Handshake response as expected
                        onHandshakeSuccess?.Invoke(comPortName);
                        break;
                    }
                    else
                    {
                        onHandshakeFail?.Invoke(comPortName);
                    }
                }
                catch (TimeoutException e)
                {
                    //OnSendStatus?.Invoke("Timeout exception: " + e, "yellow");
                    //Debug.LogWarning("Timeout exception: " + e);
                    onHandshakeFail?.Invoke(comPortName);
                }
                catch (Exception e)
                {
                    OnSendStatus?.Invoke("Exception while connecting " + e, "red");
                    Debug.LogError("Connection error 2: " + e);
                    onHandshakeFail?.Invoke(comPortName);
                    throw;
                }
            }
            
            if (serialPort.IsOpen)
                serialPort.Close();
        }
        yield return null;
    }
}