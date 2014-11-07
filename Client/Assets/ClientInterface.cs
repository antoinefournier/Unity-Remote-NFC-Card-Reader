using UnityEngine;
using System.Collections;

public class ClientInterface : MonoBehaviour
{
	public Client PublicClient;

	void Start()
	{
		if (PublicClient == null)
			Debug.LogError("Client not found.");
	}

	void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			PublicClient.DisconnectFromServer();
			Application.Quit();
			return;
		}
	}

	void OnGUI()
	{
		GUI.Label(new Rect(50, 50, 250, 25), "Client (ID: " + PublicClient.Id + ")");

		string state = "Offline";
		if (PublicClient.IsTryingToConnect)
			state = "Connecting";
		if (PublicClient.IsConnected)
			state = "Connected";
		GUI.Label(new Rect(80, 80, 150, 25), "State: " + state);

		if (!PublicClient.IsConnected && !PublicClient.IsTryingToConnect)
		{
			GUI.Label(new Rect(80, 140, 80, 25), "Server Ip:");
			PublicClient.ServerIp = GUI.TextField(new Rect(160, 140, 100, 25), PublicClient.ServerIp);

			if (GUI.Button(new Rect(120, 170, 120, 40), "Connect"))
				PublicClient.ConnectToServer();
		}

		if (PublicClient.IsConnected)
		{
			string idStr = "";
			if (PublicClient.IdServer == "")
				idStr = "(Waiting for ID)";
			else
				idStr = "(ID: " + PublicClient.IdServer + ")";
			GUI.Label(new Rect(80, 140, 250, 25), "Connected to server " + idStr);

			if (GUI.Button(new Rect(120, 170, 120, 40), "Disconnect"))
				PublicClient.DisconnectFromServer();
		}

	}

}
