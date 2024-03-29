﻿using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using SmartHome.Connector.Services;
using SmartHome.Connector.Settings;

namespace SmatHome.Connector
{
    class Program
    {
        private const int MAX_ATTEMPTS = 5;
        private const int InitialDelayMilliseconds = 1000;
        private const int MaxDelayMilliseconds = 5000;

        private static HubConnection? connection;

        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                     .SetBasePath(Directory.GetCurrentDirectory())
                                     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                     .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true)
                                     .Build();

            var rabbitMq = new RabbitMQSettings();
            configuration.GetSection("RabbitMQ").Bind(rabbitMq);

            var api = new ApiSettings();
            configuration.GetSection("Api").Bind(api);

            var signalR = new SignalRSettings();
            configuration.GetSection("SignalR").Bind(signalR);
            Console.WriteLine(" [*] Start listening...");

            int attempts = 0;
            var delayMilliseconds = InitialDelayMilliseconds;

            while (attempts < MAX_ATTEMPTS)
            {
                try
                {
                    connection = new HubConnectionBuilder().WithUrl(signalR.HubUrl).Build();
                    await connection.StartAsync();
                    break;
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"{e.Message}");
                    Console.WriteLine($"Attempt {attempts + 1} of {MAX_ATTEMPTS}");

                    await Task.Delay(delayMilliseconds);
                    delayMilliseconds = Math.Min(delayMilliseconds * 2, MaxDelayMilliseconds);

                    attempts++;
                }
            }

            if (connection is null)
            {
                Console.WriteLine($"Could not establish connection");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($" [*] Signalr state = {connection.State}");

            var rabbitMqListener = new RabbitMQListener(api, rabbitMq);
            var messageProccessor = new MessageProcessor(connection);

            try
            {
                rabbitMqListener.StartListening(messageProccessor.ProcessMessage);
            }
            catch (DatabaseConnectionException e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine("Wait...");
                await Task.Delay(1000);
            }
            catch (SignalRException e)
            {
                Console.WriteLine(e.Message);

                switch (connection.State)
                {
                    case HubConnectionState.Disconnected:
                        await connection.StartAsync();
                        break;
                    default:
                        await Task.Delay(1000);
                        break;
                }
            }

            while (true)
            {
                await Task.Delay(1000); // Delay for 1 second before looping again
            }
        }
    }
}

