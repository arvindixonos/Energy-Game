using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Server : MonoBehaviour {

    public GameManager gameManager;
    int port = 9999;
    int maxConnections = 10;

    short messageID = 1000;

    void Start ()
    {
        Application.runInBackground = true;
        CreateServer();
    }    

    public void CreateServer()
    {
        RegisterHandlers ();

        var config = new ConnectionConfig ();

        config.AddChannel (QosType.ReliableFragmented);
        config.AddChannel (QosType.UnreliableFragmented);

        var ht = new HostTopology (config, maxConnections);

        if (!NetworkServer.Configure (ht)) {
            Debug.Log ("No server created, error on the configuration definition");
            return;
        } else {
            // Start listening on the defined port
            if(NetworkServer.Listen (port))
                Debug.Log ("Server created, listening on port: " + port);   
            else
                Debug.Log ("No server created, could not listen to the port: " + port);    
        }
    }

    void OnApplicationQuit()
    {
        NetworkServer.Shutdown ();
    }

    private void RegisterHandlers ()
    {
        NetworkServer.RegisterHandler (MsgType.Connect, OnClientConnected);
        NetworkServer.RegisterHandler (MsgType.Disconnect, OnClientDisconnected);

        NetworkServer.RegisterHandler (messageID, OnMessageReceived);
    }

    private void RegisterHandler(short t, NetworkMessageDelegate handler)
    {
        NetworkServer.RegisterHandler (t, handler);
    }

    void OnClientConnected(NetworkMessage netMessage)
    {
        print("Player COnnected : " + netMessage.conn.connectionId);
        gameManager.OnPlayerConnected(netMessage.conn.connectionId);
    }

    public void SendMessage(int clientNumber, string message)
    {        
        MyNetworkMessage messageContainer = new MyNetworkMessage();
        messageContainer.message = message;

        NetworkServer.SendToClient(clientNumber, messageID, messageContainer);
    }

    public void SendMessageToAll (string message)
    {
        MyNetworkMessage messageContainer = new MyNetworkMessage();
        messageContainer.message = message;

        NetworkServer.SendToAll(messageID, messageContainer);
    }

    void OnClientDisconnected(NetworkMessage netMessage)
    {
        // Do stuff when a client dissconnects
    }

    void OnMessageReceived(NetworkMessage netMessage)
    {
        var objectMessage = netMessage.ReadMessage<MyNetworkMessage>();
        Debug.Log("Message received: " + objectMessage.message);
        gameManager.OnMessageReceived(objectMessage.message, netMessage.conn.connectionId);
    }
}