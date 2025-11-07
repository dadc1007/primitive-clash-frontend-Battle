using System;
using UnityEngine;

namespace Networks
{
    public class GameInitializer : MonoBehaviour
    {
        [Header("Referencias")]
        public GameClient gameClient;
        public ArenaEntitySpawner arenaSpawner;

        [Header("DEBUG / TEST LOCAL")]
        public string hubUrl = "http://localhost:5247/hubs/game";
        public string sessionId = "b65bec75-ef58-4a5b-9973-292342ec87e6";
        public string userId = "ba701be5-105c-4e92-93ec-92a6a6a68633";
        public bool connectOnStart = true;

        public void SetConnectionData(string data)
        {
            string[] args = data.Split('|');
            string session = args[0];
            string user = args[1];
            string hub = args[2];

            Debug.Log($"Datos recibidos -> Session:{session}, User:{user}, Hub:{hub}");
            gameClient.Connect(new Guid(session), new Guid(user), hub);
        }

        private void Start()
        {
            if (!connectOnStart)
                return;
            Debug.Log($"[DEBUG] Conectando automáticamente a {hubUrl}");
            gameClient.Connect(new Guid(sessionId), new Guid(userId), hubUrl);
        }
    }
}
