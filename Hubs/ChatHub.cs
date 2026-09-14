using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LovEat.API.DTOs;
using LovEat.API.Services;

namespace LovEat.API.Hubs
{
    /// <summary>
    /// M17: real-time chat delivery. Persistence happens through ChatService
    /// (called from here) — this hub is purely the live-delivery layer, so a
    /// client that's offline still sees the message via GET /api/chat/thread/{userId}
    /// once they reconnect.
    ///
    /// Connects at /hubs/chat. Program.cs already wires JWT bearer auth to
    /// accept the token from the query string for this path specifically,
    /// since browsers/RN clients can't set an Authorization header on the
    /// WebSocket upgrade request.
    ///
    /// Scaling note (G6): this uses the default in-memory SignalR backplane,
    /// which only works for a single server instance. Add the Redis backplane
    /// package once running more than one API instance behind a load balancer.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ChatService _chatService;
        public ChatHub(ChatService chatService) => _chatService = chatService;

        private int UserId => int.Parse(Context.User!.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public override async Task OnConnectedAsync()
        {
            // Each user joins a personal group named after their own id, so
            // SendToUser can target them without tracking connection IDs manually.
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(UserId));
            await base.OnConnectedAsync();
        }

        /// <summary>Called by the client to send a message. Persists it via ChatService, then relays it live to the receiver if they're connected.</summary>
        public async Task SendMessage(SendMessageRequestDto req)
        {
            var saved = await _chatService.SendMessageAsync(UserId, req);
            if (saved == null) return;

            await Clients.Group(GroupName(req.ReceiverId)).SendAsync("ReceiveMessage", saved);
            await Clients.Caller.SendAsync("MessageSent", saved);
        }

        /// <summary>Lets the receiver's client show a "typing..." indicator.</summary>
        public async Task NotifyTyping(int receiverId)
        {
            await Clients.Group(GroupName(receiverId)).SendAsync("UserTyping", new { userId = UserId });
        }

        public async Task MarkRead(int otherUserId)
        {
            await _chatService.MarkThreadReadAsync(UserId, otherUserId);
            await Clients.Group(GroupName(otherUserId)).SendAsync("MessagesRead", new { readByUserId = UserId });
        }

        private static string GroupName(int userId) => $"user-{userId}";
    }
}
