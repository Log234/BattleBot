using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace BattleBot
{
    public class Program
    {
        public static bool exit = false;
        private Cleanup _cleanup;
        private CommandService _commands;
        private DiscordSocketClient _client;
        private IServiceProvider _services;

        private static Timer save = new Timer(600000);
        private static Timer backup = new Timer(3600000);

        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            Data.Load();
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            _cleanup = new Cleanup();
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Info
            });

            _client.Log += Log;
            _commands = new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Info
            });
            _commands.Log += Log;

            string token = File.ReadAllText(@"C:\Secrets\BattleBot.txt");

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            await InstallCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            Data.Client = _client;
            Data.Client.Ready += Data.OnReconnect;

            save.AutoReset = true;
            save.Elapsed += Data.Save;
            save.Start();

            backup.AutoReset = true;
            backup.Elapsed += Data.Backup;
            backup.Start();



            await Task.Delay(-1);
        }

        private async void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (!exit)
            {
                await Data.MessageActiveDMs("Woop! :confused:\nSomething unexpected is happening over here, I will be back ASAP.");
                Data.Exit();
            }
        }

        private Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived Event into our Command Handler
            _client.MessageReceived += HandleCommandAsync;
            // Discover all of the commands in this assembly and load them.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Don't process the command if it was a System Message
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;
            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            // Create a Command Context
            var context = new SocketCommandContext(_client, message);
            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
