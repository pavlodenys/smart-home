using Microsoft.AspNetCore.SignalR.Client;
using SmartHome.Data;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SmatHome.Connector
{
    namespace SmatHome.Connector
    {
        public class MessageProcessor
        {
            private readonly HubConnection _hubConnection;

            public MessageProcessor(HubConnection hubConnection)
            {
                _hubConnection = hubConnection;
            }

            public void ProcessMessage(PointDto point)
            {
                using (var db = new SmartHomeDbContext())
                {
                    var time = GetTime(point);

                    if (point != null)
                    {
                        var data = new Point
                        {
                            Value = point.Value,
                            Name = point.Name,
                            DateTime = time,
                            DataId = point.Id
                        };

                        db.Add(data);
                        db.SaveChanges();

                        // Perform any additional processing or notifications here
                        _hubConnection.InvokeAsync("RabbitMQMessage", data); //TODO: check
                    }
                }
            }

            private static DateTime GetTime(PointDto? point)
            {
                DateTime? dateTime = null;

                if (point != null && point.Time != 0)
                {
                    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    TimeSpan utcOffset = TimeSpan.FromHours(3); // UTC+3

                    dateTime = unixEpoch.AddSeconds(point.Time).Add(utcOffset);
                }
                return dateTime != null ? dateTime.Value : DateTime.Now;
            }
        }
    }
}

