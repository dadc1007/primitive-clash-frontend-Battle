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
        public string hubUrl;
        public string sessionId;
        public string userId;
        public bool connectOnStart = true;

        public void SetConnectionData(string data)
        {
            string[] args = data.Split('|');
            if (args.Length < 3)
            {
                Debug.LogError($"⚠️ Datos inválidos recibidos desde React: {data}");
                return;
            }

            string session = args[0];
            string user = args[1];
            string hub = args[2];

            Debug.Log(
                $"✅ Datos recibidos desde React -> Session:{session}, User:{user}, Hub:{hub}"
            );

            gameClient.Connect(new Guid(session), new Guid(user), hub);
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (!connectOnStart)
                return;
            Debug.Log($"[DEBUG] Conectando automáticamente a {hubUrl}");
            gameClient.Connect(new Guid(sessionId), new Guid(userId), hubUrl);
#else
            Debug.Log("Esperando parámetros desde React...");
#endif
        }
    }
}
