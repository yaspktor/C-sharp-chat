using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Contexts;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NetworkLib;




namespace CSchat_service
{
    public partial class Service1 : ServiceBase, IMessageHandler, IDisconnectionHandler
    {
        private Timer timer;
/*        private string myEventLogName = ConfigurationManager.AppSettings["myEventLogName"];
        private string mySourceName = ConfigurationManager.AppSettings["mySourceName"];
        string ipAddress = ConfigurationManager.AppSettings["IPAddress"];*/

        static TcpListener listener;
        static List<NetworkLib.Client> users;
        // przelacznik logow
       
        static TraceSwitch logSwitch;

        public Service1()
        {
            InitializeComponent();
            InitializeConfiguration();
            
        }

        private void InitializeConfiguration()
        {
            string ipAddress = ConfigurationManager.AppSettings["IPAddress"];
            string port = ConfigurationManager.AppSettings["Port"];
            string defaultColor = ConfigurationManager.AppSettings["DefaultColor"];
            string myEventLogName = ConfigurationManager.AppSettings["myEventLogName"];
            string mySourceName = ConfigurationManager.AppSettings["mySourceName"];


            // Stwórz EventLog jeśli jeszcze nie istnieje
            if (!EventLog.SourceExists(mySourceName))
            {
                EventLog.CreateEventSource(mySourceName, myEventLogName);
            }

            // Konfiguracja EventLog dla tej usługi
            EventLog.Source = mySourceName;
            EventLog.Log = myEventLogName;

            users = new List<NetworkLib.Client>();
            listener = new TcpListener(IPAddress.Parse(ipAddress), Convert.ToInt32(port));
            logSwitch = new TraceSwitch("Logowanie", "Switch");

        }

        protected override void OnStart(string[] args)
        {
            // Loguj uruchomienie usługi
            if (logSwitch.TraceInfo)
            {
                EventLog.WriteEntry($"Usługa została uruchomiona.", EventLogEntryType.Information);
            }
            else if (logSwitch.TraceWarning)
            {
                EventLog.WriteEntry($"Usługa została uruchomiona, ale inny poziom.", EventLogEntryType.Warning);
            }
            

            // Ustaw timer do cyklicznego logowania co 30 sekund
            timer = new Timer(30000);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Start();

            try
            {
                // Uruchomienie logiki serwera w oddzielnym wątku
                Task.Run(() => ServerLogic());
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Wystąpił wyjątek podczas uruchamiania serwera: {ex.Message}", EventLogEntryType.Error);
            }

        }

        private void ServerLogic()
        {
            try
            {
                // Nasłuchuj na porcie 8897
                listener.Start();
                EventLog.WriteEntry("Listener został uruchomiony.", EventLogEntryType.Information);

                while (true)
                {
                    // tworzymy nową instancję naszego klienta i jako argument podajemy listener.AcceptTcpClient(), który zwraca
                    // TcpClient, który jest pobierany przez konstruktor klienta
                    var client = new NetworkLib.Client(listener.AcceptTcpClient(), this, this);

                    HandleMessage($"[{client.Username}] Connected!");
                    EventLog.WriteEntry($"Nowy klient połączony: {client.Username}", EventLogEntryType.Information);

                    //dodajmy naszego klienta do listy 

                    users.Add(client);

                    //przesłanie do wszystkich podlaczonych klientow informacji  o innych klientach
                    Manager.BroadcastConnection(users);
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Wystąpił wyjątek w logice serwera: {ex.Message}", EventLogEntryType.Error);
            }

        }



        private void OnElapsedTime(object sender, ElapsedEventArgs e)
        {
            // Loguj informację co 30 sekund
            EventLog.WriteEntry("Usługa działa.", EventLogEntryType.Information);
        }

        protected override void OnStop()
        {
            // Loguj zatrzymanie usługi
            EventLog.WriteEntry("Usługa została zatrzymana.");

            try
            {
                // Zatrzymaj listener i zwolnij zasoby
                listener.Stop();
                timer.Stop();
                timer.Dispose();
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry($"Wystąpił wyjątek podczas zatrzymywania usługi: {ex.Message}", EventLogEntryType.Error);
            }
        }

        public void HandleMessage(string message, string color = "#ffffff")
        {
            Manager.BroadcastMessage(users, message, color);
            EventLog.WriteEntry(message, EventLogEntryType.Information);
        }

        public void HandleDisconnection(string id)
        {
            
            Manager.BroadcastDisconnect(users, id);
            //znajdz username po id
            
            var disconnectUser = users.Where(x => x.Id.ToString() == id).FirstOrDefault();
            //log
            EventLog.WriteEntry($"Klient {disconnectUser.Username} został rozłączony.", EventLogEntryType.Information);
        }
    }

}
