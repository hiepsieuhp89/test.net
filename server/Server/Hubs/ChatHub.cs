using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Server.Hubs.Clients;
using Server.Models;
using System.Net;
using System.Text;

namespace Server.Hubs;

public class ChatHub : Hub<IChatClient>
{
    private const string _botUser = "System chat";

    private readonly IDictionary<string, UserConnection> _connections;
    public ChatHub(IDictionary<string, UserConnection> connections)
    {
        _connections = connections;
    }
    static string sendGet(string url)
    {
        // Create request
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

        // GET request
        request.Method = "GET";
        request.ReadWriteTimeout = 5000;
        request.ContentType = "text/html;charset=UTF-8";
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        Stream myResponseStream = response.GetResponseStream();
        StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

        // Return content
        string retString = myStreamReader.ReadToEnd();
        return retString;
    }
    public Task<string> GetIsportData(UserConnection userConnection)
    {
        string url = "http://api.isportsapi.com/sport/football/livescores/changes?api_key=wX9MOsuDjE0kk8Ix";

        // Call iSport Api to get data in json format
        string rsp = sendGet(url);

        return Task.FromResult(rsp);
    }

    public async Task JoinRoom(UserConnection userConnection)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.RoomId);
        _connections[Context.ConnectionId] = userConnection;

        await SendUsersConnected(userConnection.RoomId);
        await Clients.Group(userConnection.RoomId)
                     .ReceiveMessage(_botUser, $"User {userConnection.Username} has joined {userConnection.RoomId}");

    }

    public async Task SendMessage(string message)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out var userConnection))
        {
            await Clients.Group(userConnection.RoomId)
                .ReceiveMessage(userConnection.Username, message);
        }
    }

    public async Task SendUsersConnected(string roomId)
    {
        string[] users = _connections.Values
            .Where(c => c.RoomId == roomId)
            .Select(c => c.Username)
            .ToArray();

        await Clients.Group(roomId).ReceiveUsersInRoom(users);
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_connections.TryGetValue(Context.ConnectionId, out var userConnection))
        {
            _connections.Remove(Context.ConnectionId);
            await Clients.Group(userConnection.RoomId).ReceiveMessage(_botUser, $"{userConnection.Username} has left");
            await SendUsersConnected(userConnection.RoomId);
        }

        await base.OnDisconnectedAsync(exception);
    }

}
