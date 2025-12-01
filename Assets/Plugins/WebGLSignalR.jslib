mergeInto(LibraryManager.library, {
  StartSignalR: function (urlPtr, tokenPtr) {
    const url = UTF8ToString(urlPtr);
    const token = UTF8ToString(tokenPtr);

    if (typeof signalR === "undefined") {
      console.error("SignalR library not found in WebGL environment!");
      return;
    }

    window.signalRConnection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build();

    // Registrar todos los eventos ANTES de conectar
    window.signalRConnection.on("JoinedToGame", function (data) {
      SendMessage("NetworkManager", "JoinedToGame", JSON.stringify(data));
    });

    window.signalRConnection.on("CardSpawned", function (data) {
      SendMessage("NetworkManager", "CardSpawned", JSON.stringify(data));
    });

    window.signalRConnection.on("RefreshHand", function (data) {
      SendMessage("NetworkManager", "RefreshHand", JSON.stringify(data));
    });

    window.signalRConnection.on("TroopMoved", function (data) {
      SendMessage("NetworkManager", "TroopMoved", JSON.stringify(data));
    });

    window.signalRConnection.on("UnitDamaged", function (data) {
      SendMessage("NetworkManager", "UnitDamaged", JSON.stringify(data));
    });

    window.signalRConnection.on("UnitKilled", function (data) {
      SendMessage("NetworkManager", "UnitKilled", JSON.stringify(data));
    });

    window.signalRConnection.on("NewElixir", function (value) {
      SendMessage("NetworkManager", "NewElixir", value.toString());
    });

    window.signalRConnection.on("EndGame", function (data) {
      SendMessage("NetworkManager", "EndGame", JSON.stringify(data));
    });

    window.signalRConnection.on("Hand", function (data) {
      SendMessage("NetworkManager", "Hand", JSON.stringify(data));
    });

    window.signalRConnection.on("Error", function (message) {
      SendMessage("NetworkManager", "OnServerError", message);
    });

    window.signalRConnection
      .start()
      .then(() => {
        SendMessage("NetworkManager", "OnSignalRConnected", "");
      })
      .catch((err) => {
        console.error("SignalR connection failed:", err);
        SendMessage("NetworkManager", "OnServerError", err.toString());
      });
  },

  StopSignalR: function () {
    if (window.signalRConnection) {
      window.signalRConnection
        .stop()
        .then(() => {
          window.signalRConnection = null;
        })
        .catch((err) => console.error("Error disconnecting SignalR:", err));
    }
  },

  InvokeServer: function (methodPtr, argsJsonPtr) {
    const method = UTF8ToString(methodPtr);
    const argsJson = UTF8ToString(argsJsonPtr);

    if (!window.signalRConnection) {
      console.error("No SignalR connection found");
      return;
    }

    try {
      const args = JSON.parse(argsJson);

      if (method === "JoinGame") {
        window.signalRConnection
          .invoke(method, args.sessionId)
          .catch((err) => console.error("Invoke error:", err));
      } else if (method === "SpawnCard") {
        window.signalRConnection
          .invoke(method, args.sessionId, args.cardId, args.x, args.y)
          .catch((err) => console.error("Invoke error:", err));
      }
    } catch (err) {
      console.error("Error parsing args:", err);
    }
  },
});
