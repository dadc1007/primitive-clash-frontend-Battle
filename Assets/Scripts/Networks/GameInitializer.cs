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
        public string testToken;
        public bool connectOnStart = true;

        public void SetConnectionData(string data)
        {
            string[] args = data.Split('|');

            if (args.Length < 4)
            {
                Debug.LogError($"⚠️ Datos inválidos recibidos desde React: {data}");
                return;
            }

            string session = args[0];
            string user = args[1];
            string token = args[2];
            string hub = args[3];

            gameClient.Connect(new Guid(session), new Guid(user), token, hub);
        }

        private void Start()
        {
#if UNITY_EDITOR
            if (!connectOnStart)
                return;
            gameClient.Connect(new Guid(sessionId), new Guid(userId), testToken, hubUrl);
#endif
        }
    }
}
