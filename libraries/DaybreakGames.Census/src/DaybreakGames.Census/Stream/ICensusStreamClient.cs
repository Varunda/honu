using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace DaybreakGames.Census.Stream
{
    public interface ICensusStreamClient: IDisposable
    {
        CensusStreamClient SetServiceId(string serviceId);
        CensusStreamClient SetServiceNamespace(string serviceNamespace);
        CensusStreamClient OnConnect(Func<ReconnectionType, Task> onConnect);
        CensusStreamClient OnDisconnect(Func<DisconnectionInfo, Task> onDisconnect);
        CensusStreamClient OnMessage(Func<string, Task> onMessage);
        void Subscribe(CensusStreamSubscription subscription);
        Task ConnectAsync();
        Task DisconnectAsync();
        Task ReconnectAsync();
    }
}
