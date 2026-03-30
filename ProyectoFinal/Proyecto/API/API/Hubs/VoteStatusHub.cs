using Business.Authentication;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    [Authorize]
    [AuthorizeUserRoleAttribute(EUserRole.ADMIN)]
    public class VoteStatusHub() : Hub
    {
        public async Task SubscribeToVoteUpdates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "VoteUpdates");
        }

        public async Task UnsubscribeFromVoteUpdates()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "VoteUpdates");
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }
    }
}
