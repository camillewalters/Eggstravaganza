using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
#if UNITY_EDITOR
using ParrelSync;
#endif

public class MatchmakerClient : MonoBehaviour
{
    private string m_TicketId;

    private void OnEnable()
    {
        ServerStartup.ClientInstance += SignIn;
    }

    private void OnDisable()
    {
        ServerStartup.ClientInstance -= SignIn;
    }

    public void StartClient()
    {
        CreateTicket();
    }

    private async void SignIn()
    {
        await ClientSignIn("Gopher");
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task ClientSignIn(string serviceProfileName = null)
    {
        if (serviceProfileName != null)
        {
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
#endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }
        else
        {
            await UnityServices.InitializeAsync();
        }

        Debug.Log($"Signed in anonymously as {serviceProfileName}({PlayerId()})");
    }

    private string PlayerId()
    {
        return AuthenticationService.Instance.PlayerId;
    }

#if UNITY_EDITOR
    private string GetCloneNumberSuffix()
    {
        string projectPath = ClonesManager.GetCurrentProjectPath();
        int lastUnderscore = projectPath.LastIndexOf("_");
        string projectCloneSuffix = projectPath.Substring(lastUnderscore + 1);
        if (projectCloneSuffix.Length != 1)
        {
            projectCloneSuffix = "";
        }

        return projectCloneSuffix;
    }
#endif

    private async void CreateTicket()
    {
        var options = new CreateTicketOptions("MainMode"); // Queue name set in the Matchmaker dashboard
        var players = new List<Player> { new(PlayerId()) };
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
        m_TicketId = ticketResponse.Id;
        Debug.Log($"Ticket ID: {m_TicketId}");
        PollTicketStatus();
    }

    private async void PollTicketStatus()
    {
        MultiplayAssignment multiplayAssignment = null;
        bool gotAssignment = false;

        do
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(m_TicketId);
            if (ticketStatus == null) continue;
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                multiplayAssignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (multiplayAssignment.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    gotAssignment = true;
                    TicketAssigned(multiplayAssignment);
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError($"Failed to get ticket status. Error: {multiplayAssignment.Message}");
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status, timed out.");
                    break;
            }
        } while (!gotAssignment);
    }

    private void TicketAssigned(MultiplayAssignment assignment)
    {
        Debug.Log($"Ticket assigned: {assignment.Ip}:{assignment.Port}");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(assignment.Ip, (ushort)assignment.Port);
        NetworkManager.Singleton.StartClient();
    }
}
