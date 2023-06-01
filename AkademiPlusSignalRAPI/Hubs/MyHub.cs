using AkademiPlusSignalRAPI.DAL;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AkademiPlusSignalRAPI.Hubs
{
    public class MyHub:Hub
    {       
        public static List<string> Names { get; set; }=new List<string>(); //kişilerimi tutucak olan liste
        public int ClientCount { get; set; }=0; // o anda kaç client bağlı oldugunu gösteren property
        public static int RoomCount { get; set; }=5; //bir odada max bulunacak kişi sayısı,opsiyonel
        private readonly Context _context;

        public MyHub(Context context)
        {
            _context = context;
        }
       public async Task Sendname(string name)
        {
            Names.Add(name);
            await Clients.All.SendAsync("receivename",name);
        }
        public async override Task OnConnectedAsync()
        {
            ClientCount++;
            await Clients.All.SendAsync("receiveclientcount",ClientCount);
        }
        public async override Task OnDisconnectedAsync(Exception exception)
        {
            ClientCount--;
            await Clients.All.SendAsync("receiveclientcount", ClientCount);
        }
    }
}
