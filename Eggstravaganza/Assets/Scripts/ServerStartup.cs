// Inspired by samyam's video on the topic: https://www.youtube.com/watch?v=FjOZrSPL_-Y&t=1052s

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Multiplay;
using UnityEngine;

public class ServerStartup : MonoBehaviour
{
    public static event Action ClientInstance;
    
    private const int k_MaxPlayers = 4;
    private const int k_MultiplayServiceTimeout = 20000;
    private const string k_InternalServerIp = "0.0.0.0";
    private const int k_TicketCheckMs = 1000;

    private ushort m_ServerPort = 7777;
    private string m_ExternalServerIp = "0.0.0.0";
    private string m_ExternalConnectionString => $"{m_ExternalServerIp}:{m_ServerPort}";

    private IMultiplayService m_MultiplayService;
    private string m_AllocationId;
    private MultiplayEventCallbacks m_ServerCallbacks;

    private BackfillTicket m_LocalBackfillTicket;
    private CreateBackfillTicketOptions m_CreateBackfillTicketOptions;
    private MatchmakingResults m_MatchmakingPayload;
    private bool m_Backfilling;

    private async void Start()
    {
        var server = false;
        var args = Environment.GetCommandLineArgs();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedServer")
            {
                server = true;
            }

            if (args[i] == "-port" && i + 1 < args.Length)
            {
                m_ServerPort = (ushort)int.Parse(args[i + 1]);
            }
            
            if (args[i] == "-ip" && i + 1 < args.Length)
            {
                m_ExternalServerIp = args[i + 1];
            }
        }

        if (server)
        {
            StartServer();
            await StartServerServices();
        }
        else
        {
            ClientInstance?.Invoke();
        }
    }

    private void StartServer()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(k_InternalServerIp, m_ServerPort);
        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientDisconnected;
    }

    private async Task StartServerServices()
    {
        await UnityServices.InitializeAsync();
        try
        {
            m_MultiplayService = MultiplayService.Instance;
            await m_MultiplayService.StartServerQueryHandlerAsync(k_MaxPlayers, "n/a", "n/a", "0", "n/a");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the SQP Service:\n{ex}");
        }

        try
        {
            m_MatchmakingPayload = await GetMatchmakerPayload(k_MultiplayServiceTimeout);
            if (m_MatchmakingPayload != null)
            {
                Debug.Log($"Got payload: {m_MatchmakingPayload}");
                await StartBackfill(m_MatchmakingPayload);
            }
            else
            {
                Debug.LogWarning("Timed out waiting for matchmaker payload");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to set up the Allocation Service:\n{ex}");
        }
    }

    private async Task<MatchmakingResults> GetMatchmakerPayload(int timeout)
    {
        var matchmakerPayloadTask = SubscribeAndAwaitMatchmakerAllocation();
        if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(timeout)) == matchmakerPayloadTask)
        {
            return matchmakerPayloadTask.Result;
        }

        return null;
    }

    private async Task<MatchmakingResults> SubscribeAndAwaitMatchmakerAllocation()
    {
        if (m_MultiplayService == null) return null;

        m_AllocationId = null;
        m_ServerCallbacks = new MultiplayEventCallbacks();
        m_ServerCallbacks.Allocate += OnMultiplayAllocation;
        await m_MultiplayService.SubscribeToServerEventsAsync(m_ServerCallbacks);

        m_AllocationId = await AwaitAllocationId();
        var matchmakerPayload = await GetMatchmakerAllocationPayloadAsync();
        return matchmakerPayload;
    }

    private void OnMultiplayAllocation(MultiplayAllocation allocation)
    {
        Debug.Log($"OnAllocation: {allocation.AllocationId}");
        if (string.IsNullOrEmpty(allocation.AllocationId)) return;
        m_AllocationId = allocation.AllocationId;
    }

    private async Task<string> AwaitAllocationId()
    {
        var config = m_MultiplayService.ServerConfig;
        Debug.Log("Awaiting allocation. Server config is:\n" +
                  $"-ServerID: {config.ServerId}\n" +
                  $"-AllocationID: {config.AllocationId}\n" +
                  $"-Port: {config.Port}\n" +
                  $"-QPort: {config.QueryPort}" +
                  $"-logs: {config.ServerLogDirectory}");

        while (string.IsNullOrEmpty(m_AllocationId))
        {
            var configId = config.AllocationId;
            if (!string.IsNullOrEmpty(configId) && string.IsNullOrEmpty(m_AllocationId))
            {
                m_AllocationId = configId;
                break;
            }

            await Task.Delay(100);
        }

        return m_AllocationId;
    }
    
    private async Task<MatchmakingResults> GetMatchmakerAllocationPayloadAsync()
    {
        try
        {
            var payloadAllocation =
                await MultiplayService.Instance.GetPayloadAllocationFromJsonAs<MatchmakingResults>();
            var modelAsJson = JsonConvert.SerializeObject(payloadAllocation, Formatting.Indented);
            Debug.Log($"Matchmaker payload allocation: {modelAsJson}");
            return payloadAllocation;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Something went wrong trying to get the Matchmaker payload:\n{ex}");
        }

        return null;
    }
    
    private async Task StartBackfill(MatchmakingResults payload)
    {
        var backfillProperties = new BackfillTicketProperties(payload.MatchProperties);
        m_LocalBackfillTicket = new BackfillTicket
            { Id = payload.MatchProperties.BackfillTicketId, Properties = backfillProperties };
        await BeginBackfilling(payload);
    }

    private async Task BeginBackfilling(MatchmakingResults payload)
    {
        var matchProperties = payload.MatchProperties;

        Debug.Log("Begin backfilling!");

        if (string.IsNullOrEmpty(m_LocalBackfillTicket.Id))
        {
            m_CreateBackfillTicketOptions = new CreateBackfillTicketOptions
            {
                Connection = m_ExternalConnectionString,
                QueueName = payload.QueueName,
                Properties = new BackfillTicketProperties(matchProperties)
            };

            m_LocalBackfillTicket.Id = await MatchmakerService.Instance.CreateBackfillTicketAsync(m_CreateBackfillTicketOptions);
        }

        m_Backfilling = true;
#pragma warning disable 4014
        BackfillLoop();
#pragma warning restore 4014
    }

    private async Task BackfillLoop()
    {
        while (m_Backfilling && NeedsPlayers())
        {
            m_LocalBackfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(m_LocalBackfillTicket.Id);
            if (!NeedsPlayers())
            {
                await MatchmakerService.Instance.DeleteBackfillTicketAsync(m_LocalBackfillTicket.Id);
                m_LocalBackfillTicket = null;
                m_Backfilling = false;
                return;
            }

            await Task.Delay(k_TicketCheckMs);
        }
        
        m_Backfilling = false;
    }

    private void ClientDisconnected(ulong id)
    {
        if (!m_Backfilling && NetworkManager.Singleton.ConnectedClients.Count > 0 && NeedsPlayers())
        {
            BeginBackfilling(m_MatchmakingPayload);
        }
    }

    private bool NeedsPlayers()
    {
        return NetworkManager.Singleton.ConnectedClients.Count < k_MaxPlayers;
    }

    private void Dispose()
    {
        m_ServerCallbacks.Allocate -= OnMultiplayAllocation;
    }
}
