using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;


public class Server : MonoBehaviour
{
	/// <summary>ID identifying the server, change every time the server application is started.</summary>
	private string mId;

	/// <summary>ID identifying the server, change every time the server application is started.</summary>
	public string Id { get { return mId; } }


	/// <summary>List of our known IPs.</summary>
	private List<string> mServerIpList = new List<string>();

	/// <summary>List of our known IPs.</summary>
	public List<string> IpList { get { return mServerIpList; } }

	/// <summary>Port to use for the connection.</summary>
	[Range(1025, 65535)]
	public int ServerPort = 32000;



	/// <summary>Indicate if the server is initializing.</summary>
	private bool mIsStarting;

	/// <summary>Indicate if the server is initializing.</summary>
	public bool IsStarting { get { return mIsStarting; } }


	/// <summary>Indicate if the server is running.</summary>
	private bool mIsOnline;

	/// <summary>Indicate if we are connected to the server.</summary>
	public bool IsOnline { get { return mIsOnline; } }


	/// <summary>Indicate if the server is connected to the client.</summary>
	private bool mIsConnectedToClient;

	/// <summary>Indicate if the server is connected to the client.</summary>
	public bool IsConnectedToClient { get { return mIsConnectedToClient; } }


	/// <summary>Network Id of the connected player.</summary>
	private NetworkPlayer mConnectedClient;

	/// <summary>Id of the connected client.</summary>
	private string mIdClient;

	/// <summary>Id of the connected client.</summary>
	public string IdClient { get { return mIdClient; } }


#region Console

	public enum ConsoleDebugLevel { NONE, UNITY, CONSOLE, UNITY_AND_CONSOLE }

	public ConsoleDebugLevel LevelConsole = ConsoleDebugLevel.UNITY_AND_CONSOLE;
	List<string> mConsoleTextsList = new List<string>();

#endregion


	void Start()
	{
		Int32 timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
		mId = timestamp.ToString() + UnityEngine.Random.Range(0, 1000);

		mIsStarting				= false;
		mIsOnline				= false;
		mIsConnectedToClient	= false;

		mConnectedClient		= new NetworkPlayer();
		mIdClient				= "";
	}

	/// <summary>
	/// Start the server if he is offline.
	/// </summary>
	public void StartServer()
	{
		if (mIsOnline || mIsStarting)
			return;

		LogToUnityAndConsole("Starting the server...");
		mIsStarting = true;
		mIsOnline = false;
		mIsConnectedToClient = false;
		Network.InitializeServer(1, ServerPort, false);
	}

	public void StopServer()
	{
		if (!mIsOnline)
			return;

		Network.Disconnect();
		LogToUnityAndConsole("Server stoped");

		mIsStarting = false;
		mIsOnline = false;
		mIsConnectedToClient = false;

		mServerIpList.Clear();

		mConnectedClient = new NetworkPlayer();
		mIdClient = "";
	}

	void OnServerInitialized()
	{
		LogToUnityAndConsole("...server initialized");
		mIsStarting = false;
		mIsOnline = true;
		RetrieveIp();
	}

	void OnPlayerConnected()
	{
		LogToUnityAndConsole("Client connected");
		mIsConnectedToClient = true;

		int nbConnection = Network.connections.Length;
		if (nbConnection != 1)
			Debug.LogError("Invalid number of connection");
		if (nbConnection > 0)
			mConnectedClient = Network.connections[0];

		SendIdToClient();
	}

	void OnPlayerDisconnected()
	{
		LogToUnityAndConsole("Client disconnected");
		
		mIsConnectedToClient = false;

		mConnectedClient = new NetworkPlayer();
		mIdClient = "";
	}

	public void CloseClientConnection()
	{
		if (!mIsConnectedToClient)
			return;
		
		LogToUnityAndConsole("Connection to client closed");
		Network.CloseConnection(mConnectedClient, true);

		mIsConnectedToClient = false;

		mConnectedClient = new NetworkPlayer();
		mIdClient = "";
	}

	void Update()
	{
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
	/// Get the list of our Ip addresses.
	/// The local Ips (192.168.*.*) are put in the beginning of the list.
	/// </summary>
	private void RetrieveIp()
	{
		IPHostEntry host;
		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				string ipStr = ip.ToString();
				if (ipStr.Contains("192.168."))
					mServerIpList.Insert(0, ipStr);
				else
					mServerIpList.Add(ipStr);
			}
		}
	}

	/// <summary>
	/// RPC used by the client.
	/// </summary>
	[RPC] public void SendServerId(string _serverId) { }

	/// <summary>
	/// Receive the client id.
	/// </summary>
	[RPC]
	public void SendClientId(string _clientId)
	{
		mIdClient = _clientId;
	}

	[RPC]
	public void SendCardData(string _data)
	{
		LogToUnityAndConsole("Card data received: " + _data);
	}

	public void SendIdToClient()
	{
		networkView.RPC("SendServerId", RPCMode.Others, mId);
	}

	void LogToUnityAndConsole(string _text)
	{
		if (LevelConsole == ConsoleDebugLevel.UNITY || LevelConsole == ConsoleDebugLevel.UNITY_AND_CONSOLE)
			Debug.Log(_text);

		if (LevelConsole == ConsoleDebugLevel.CONSOLE || LevelConsole == ConsoleDebugLevel.UNITY_AND_CONSOLE)
			mConsoleTextsList.Add(_text);
	}
}
