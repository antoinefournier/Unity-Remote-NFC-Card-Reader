using UnityEngine;
using System.Collections;

public class ServerInterface : MonoBehaviour
{
	public Server PublicServer;

	void Start()
	{
		if (PublicServer == null)
			Debug.LogError("Server not found.");
	}

	void OnGUI()
	{
		GUI.Label(new Rect(50, 50, 250, 25), "Server (ID: " + PublicServer.Id + ")");

		string state = "Offline";
		if (PublicServer.IsStarting)
			state = "Starting";
		if (PublicServer.IsOnline)
			state = "Online";
		GUI.Label(new Rect(80, 80, 150, 25), "State: " + state);

		// Show the Ip(s)
		if (PublicServer.IsOnline)
		{
			string ipStr = "";
			foreach (string ip in PublicServer.IpList)
			{
				if (ipStr != "")
					ipStr += " - ";
				ipStr += ip;
			}

			GUI.Label(new Rect(100, 110, 250, 50), "IP: " + ipStr);
		}


		if (!PublicServer.IsOnline && !PublicServer.IsStarting)
		{
			if (GUI.Button(new Rect(50, 150, 120, 40), "Start the server"))
				PublicServer.StartServer();
		}
		else if (PublicServer.IsOnline && !PublicServer.IsStarting)
		{
			if (GUI.Button(new Rect(50, 150, 120, 40), "Stop the server"))
				PublicServer.StopServer();
		}

		if (PublicServer.IsOnline)
		{
			if (!PublicServer.IsConnectedToClient)
			{
				GUI.Label(new Rect(400, 80, 250, 50), "No client connected");
			}
			else
			{
				string idStr = "";
				if (PublicServer.IdClient == "")
					idStr = "(Waiting for ID)";
				else
					idStr = "(ID: " + PublicServer.IdClient + ")";
				GUI.Label(new Rect(400, 80, 250, 50), "Client connected " + idStr);


				if (GUI.Button(new Rect(380, 150, 120, 40), "Close connection"))
					PublicServer.CloseClientConnection();
			}
		}
	}

}
