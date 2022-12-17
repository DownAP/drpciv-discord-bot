using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using drpciv_discord_bot.Commands;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace drpciv_discord_bot
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        public InteractionService _handler;
        private readonly IServiceProvider _services;
        
        public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services)
        {
            _client = client;
            _handler = handler;
            _services = services;
        }

        public async Task InitAsync()
        {
            
            _client.InteractionCreated += _client_InteractionCreated;
            _client.SelectMenuExecuted += _client_SelectMenuExecuted;
            _client.SlashCommandExecuted += _client_SlashCommandExecuted;
            _client.MessageCommandExecuted += _client_MessageCommandExecuted;
            _client.Ready += _client_Ready;
            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task _client_MessageCommandExecuted(SocketMessageCommand arg)
        {
            BaseModule.uid = arg.User.Id;
        }

        private async Task _client_SlashCommandExecuted(SocketSlashCommand arg)
        {
            //Assigns author id
            BaseModule.uid = arg.User.Id;
        }

        private async Task _client_SelectMenuExecuted(SocketMessageComponent arg)
        {
            //Assigns last message id
            BaseModule.userMessage = arg.Message.Id;
        }

        private async Task _client_Ready()
        {
            //Registers slash command
            await _handler.RegisterCommandsGloballyAsync(true);

        }

        private async Task _client_InteractionCreated(SocketInteraction arg)
        {
            var ctx = new SocketInteractionContext(_client, arg);
            await _handler.ExecuteCommandAsync(ctx, _services);
        }
    }
}