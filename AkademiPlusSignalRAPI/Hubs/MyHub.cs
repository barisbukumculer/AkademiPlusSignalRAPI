using AkademiPlusSignalRAPI.DAL;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkademiPlusSignalRAPI.Hubs
{
    public class MyHub : Hub
    {
        public static List<string> Names { get; set; } = new List<string>(); //kişilerimi tutucak olan liste
        public static int ClientCount { get; set; } = 0; // o anda kaç client bağlı oldugunu gösteren property
        public static int RoomCount { get; set; } = 5; //bir odada max bulunacak kişi sayısı,opsiyonel
        private readonly Context _context;

        public MyHub(Context context)
        {
            _context = context;
        }
        //public async Task Sendname(string name)
        // {
        //     Names.Add(name);
        //     await Clients.All.SendAsync("receivename",name);    //isim ekleme işlemi.
        // }
        public async Task Sendname(string name)
        {
            if (Names.Count >= RoomCount)
            {
                await Clients.Caller.SendAsync("error", $"Bu odada en fazla {RoomCount} kişi olabilir.");
            }
            else
            {
                Names.Add(name);
                await Clients.All.SendAsync("receivename", name);    //isim ekleme işlemi.
            }
        }
        public async Task Getnames(string name)
        {
            await Clients.All.SendAsync("receivenames", Names);   //isim listesinin tümünü getirmek için yazıldı.
        }
        public async override Task OnConnectedAsync()
        {
            ClientCount++;
            await Clients.All.SendAsync("receiveclientcount", ClientCount);
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            ClientCount--;
            await Clients.All.SendAsync("receiveclientcount", ClientCount);
        }
        public async Task SendNameByGroup(string Name, string roomName )
        {
            var room = _context.Rooms.Where(x => x.RoomName == roomName).FirstOrDefault();
            if (room!=null)
            {
                room.Users.Add(new User()
                {
                    Username= Name
                });
            }
            else
            {
                var newRoom = new Room() { RoomName = roomName };
                newRoom.Users.Add(new User() { Username = Name });
                _context.Rooms.Add(newRoom);
            }
            await _context.SaveChangesAsync();
            await Clients.Group(roomName).SendAsync("receiveMessageByGroup",Name,room.RoomID);
        }
        public async Task GetNamesByGroup()
        {
            var rooms=_context.Rooms.Include(x=> x.Users).Select(y=>new
            {
               RoomID= y.RoomID,
                Users=y.Users.ToList()
            });
            await Clients.All.SendAsync("receiveNamesByGroup",rooms);
        }
        public async Task AddToGroup(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }
        public async Task RemoveFromGroup(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }
    }
}
