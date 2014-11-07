using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;


public class Client : MonoBehaviour
{
	/// <summary>ID identifying the client, change every time the client application is started.</summary>
	private string mId;

	/// <summary>ID identifying the client, change every time the client application is started.</summary>
	public string Id { get { return mId; } }


	/// <summary>Ip address of the server. Unknown at first.</summary>
	public string ServerIp;

	/// <summary>Port to use for the connection.</summary>
	[Range(1025, 65535)]
	public int ServerPort = 32000;


	/// <summary>Indicate if we are trying to connect to the server.</summary>
	private bool mIsTryingToConnect;

	/// <summary>Indicate if we are trying to connect to the server.</summary>
	public bool IsTryingToConnect { get { return mIsTryingToConnect; } }


	/// <summary>Indicate if we are connected to the server.</summary>
	private bool mIsConnected;

	/// <summary>Indicate if we are connected to the server.</summary>
	public bool IsConnected { get { return mIsConnected; } }



	/// <summary>Network Id of the server.</summary>
	private NetworkPlayer mConnectedServer;

	/// <summary>Id of the server.</summary>
	private string mIdServer;

	/// <summary>Id of the server.</summary>
	public string IdServer { get { return mIdServer; } }


#region Console

	public enum ConsoleDebugLevel { NONE, UNITY, CONSOLE, UNITY_AND_CONSOLE }

	public ConsoleDebugLevel LevelConsole = ConsoleDebugLevel.UNITY_AND_CONSOLE;
	List<string> mConsoleTextsList = new List<string>();

#endregion


	void Start()
	{
		Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		mId = timestamp.ToString() + UnityEngine.Random.Range(0, 1000);

		ServerIp			= "127.0.0.1";
		mIsTryingToConnect	= false;
		mIsConnected		= false;

		mConnectedServer = new NetworkPlayer();
		mIdServer = "";
	}

	public void ConnectToServer()
	{
		if (IsConnected || mIsTryingToConnect)
			return;

		LogToUnityAndConsole("Connecting to the server...");
		mIsTryingToConnect = true;
		mIsConnected = false;
		Network.Connect(ServerIp, ServerPort);
	}

	public void DisconnectFromServer()
	{
		if (!IsConnected)
			return;

		LogToUnityAndConsole("Disconnecting from the server...");
		Network.CloseConnection(mConnectedServer, true);

		mIsTryingToConnect = false;
		mIsConnected = false;

		mConnectedServer = new NetworkPlayer();
		mIdServer = "";
	}

	void OnFailedToConnect(NetworkConnectionError _error)
	{
		LogToUnityAndConsole("...failed to connect to the server (" + _error + ")");
		mIsConnected = false;
		mIsTryingToConnect = false;
	}

	void OnConnectedToServer()
	{
		LogToUnityAndConsole("...connected to the server");
		mIsConnected = true;
		mIsTryingToConnect = false;

		int nbConnection = Network.connections.Length;
		if (nbConnection != 1)
			Debug.LogError("Invalid number of connection");
		if (nbConnection > 0)
			mConnectedServer = Network.connections[0];

		SendIdToServer();
	}

	void OnDisconnectedFromServer()
	{
		if (!mIsConnected)
			return;

		LogToUnityAndConsole("Disconnected from the server");
		mIsConnected = false;
		mIsTryingToConnect = false;

		mConnectedServer = new NetworkPlayer();
		mIdServer = "";
	}

	void OnGUI()
	{
		// Console

		int y = Screen.height - 80;
		for (int n = mConsoleTextsList.Count - 1; n >= 0; --n)
		{
			GUI.Label(new Rect(10, y, Screen.width - 10, 20), mConsoleTextsList[n]);
			y -= 20;
			if (y < (int)(Screen.height / 3.0f))
				break;
		}
	}

	/// <summary>
	/// RPC used by the server.
	/// </summary>
	[RPC] public void SendCardData(string _data) { }

	/// <summary>
	/// RPC used by the server.
	/// </summary>
	[RPC] public void SendClientId(string _clientId) { }

	/// <summary>
	/// Receive the server id.
	/// </summary>
	[RPC]
	public void SendServerId(string _serverId)
	{
		mIdServer = _serverId;
	}

	public void SendIdToServer()
	{
		networkView.RPC("SendClientId", RPCMode.Server, mId);
	}

	/// <summary>
	/// Called by Android when a tag is scanned.
	/// </summary>
	public void OnNfcTagScanned(String _data)
	{
		if (IsConnected == false ||
			_data == null || _data == "")
			return;

		networkView.RPC("SendCardData", RPCMode.Server, _data);

		LogToUnityAndConsole("Data sent: " + _data);
	}

	void LogToUnityAndConsole(string _text)
	{
		if (LevelConsole == ConsoleDebugLevel.UNITY || LevelConsole == ConsoleDebugLevel.UNITY_AND_CONSOLE)
			Debug.Log(_text);

		if (LevelConsole == ConsoleDebugLevel.CONSOLE || LevelConsole == ConsoleDebugLevel.UNITY_AND_CONSOLE)
			mConsoleTextsList.Add(_text);
	}
}
