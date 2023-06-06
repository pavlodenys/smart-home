using Microsoft.AspNetCore.SignalR.Client;
using SmartHome.Data;
using SmartHome.Data.AutoMapper;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;

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
                        _hubConnection.InvokeAsync("RabbitMQMessage", data.MapToDto<Point, PointDto>()); //TODO: check
                    }
                }
            }

            private static DateTime GetTime(PointDto? point)
            {
                DateTime? dateTime = null;

                if (point != null && point.Time != 0)
                {
                    var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    dateTime = unixEpoch.AddSeconds(point.Time).ToLocalTime();
                }
                return dateTime != null ? dateTime.Value : DateTime.Now;
            }
        }
    }
