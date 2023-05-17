using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(MatchmakerClient))]
public class NetworkButtons : MonoBehaviour {
    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) {
            // if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            // if (GUILayout.Button("Server")) NetworkManager.Singleton.StartServer();
            if (GUILayout.Button("Client")) GetComponent<MatchmakerClient>().StartClient();
        }

        GUILayout.EndArea();
    }
}