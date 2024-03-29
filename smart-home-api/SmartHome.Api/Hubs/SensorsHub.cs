﻿using Microsoft.AspNetCore.SignalR;
using SmartHome.Data.DTO;

namespace SmartHome.Api.Hubs
{
    public class SensorsHub : Hub
    {
        public async Task RabbitMQMessage(PointDto point)
        {
            // Process the message
            // You can send the message to Svelte UI or other clients using HubContext or Clients property
            await Clients.All.SendAsync("ReceiveMessage", point);
        }
    }
}
