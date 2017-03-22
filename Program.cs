using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using vtortola.WebSockets.Listener;
using vtortola.WebSockets;
using System.Threading;
using System.Configuration;
using CommandExecutor;
using System.Web.Script.Serialization;

namespace TestConsole
{
    class Program
    {

        static string GetCommand(WebSocket ws)
        {
            return ws.HttpRequest.RequestUri.OriginalString.Substring(1);
        }
        
        static void Main(string[] args)
        {
            //  порт вынести в настройки
            int port = 8009;
           
            using (var server = new WebSocketEventListener(
                    new IPEndPoint(IPAddress.Any, port),
                    new WebSocketListenerOptions()
                    {
                        SubProtocols = new String[] { "text" },
                        NegotiationTimeout = TimeSpan.FromSeconds(30),
                        PingTimeout = TimeSpan.FromSeconds(30),                    
                        WebSocketReceiveTimeout = TimeSpan.FromSeconds(30),
                        WebSocketSendTimeout = TimeSpan.FromSeconds(30)
                    }
            ))
            {                
                server.OnConnect += (ws) => Console.WriteLine("Connection from " + ws.RemoteEndpoint.ToString());
                server.OnDisconnect += (ws) => Console.WriteLine("Disconnection from " + ws.RemoteEndpoint.ToString());
                server.OnError += (ws, ex) => Console.WriteLine("Error: " + ex.Message);
                server.OnMessage += (ws, msg) =>
                {

                    var jsSerializer = new JavaScriptSerializer();
                    Command command = jsSerializer.Deserialize<Command>(msg);
                    
                    Console.WriteLine("client: {0} command: {1} params: {2}", 
                        ws.RemoteEndpoint,
                        command.Name, 
                        command.Value
                    );
                   
                    // ReSharper disable AccessToDisposedClosure
                    var commandExecutor = new CommandExecutor.CommandExecutor(server, ws);
                    // ReSharper restore AccessToDisposedClosure
                    commandExecutor.Execute(command);
                                                       
                };
                
                server.Start();
                Console.WriteLine("server start at {0} port", port);
                Console.ReadKey(true);
            }
        }
    }
}
