using System;
using UnityEngine;
using UnityEngine.Networking;

public class Client : MonoBehaviour
{
    public  GameManager gameManager;
    public string ip = "localhost";
    int port = 9999;

    // The id we use to identify our messages and register the handler
    short messageID = 1000;

    // The network client
    NetworkClient client;

    public void CreateClient()
    {
        var config = new ConnectionConfig ();

        // Config the Channels we will use
        config.AddChannel (QosType.ReliableFragmented);
        config.AddChannel (QosType.UnreliableFragmented);

        // Create the client ant attach the configuration
        client = new NetworkClient ();
        client.Configure (config,1);

        // Register the handlers for the different network messages
        RegisterHandlers();

        // Connect to the server
        client.Connect (ip, port);
    }

    // Register the handlers for the different message types
    void RegisterHandlers ()
    {
    
        // Unity have different Messages types defined in MsgType
        client.RegisterHandler (messageID, OnMessageReceived);
        client.RegisterHandler(MsgType.Connect, OnConnected);
        client.RegisterHandler(MsgType.Disconnect, OnDisconnected);
    }

    void OnConnected(NetworkMessage message)
    {        
        gameManager.StartApp();
    }

    public void SendMessage (string message)
    {
        MyNetworkMessage messageContainer = new MyNetworkMessage();
        messageContainer.message = message;

        // Say hi to the server when connected
        client.Send(messageID,messageContainer);
    }

    void OnDisconnected(NetworkMessage message)
    {
#if !UNITY_EDITOR
        gameManager.ReScanIP();
#endif
    }

    // Message received from the server
    void OnMessageReceived(NetworkMessage netMessage)
    {
        // You can send any object that inherence from MessageBase
        // The client and server can be on different projects, as long as the MyNetworkMessage or the class you are using have the same implementation on both projects
        // The first thing we do is deserialize the message to our custom type
        var objectMessage = netMessage.ReadMessage<MyNetworkMessage>();

        Debug.Log("Message received: " + objectMessage.message);
        gameManager.OnMessageRecieved(objectMessage.message);
    }
}