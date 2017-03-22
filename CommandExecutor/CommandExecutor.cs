using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using vtortola.WebSockets;
using vtortola.WebSockets.Listener;

namespace CommandExecutor
{
    public class CommandExecutor
    {

        private WebSocketEventListener _listener;
        private WebSocket _ws;

        public CommandExecutor(WebSocketEventListener listener, WebSocket ws)
        {
            _listener = listener;
            _ws = ws;
        }

        public bool Execute(Command command)
        {
            Response response = new Response { Command = command };
            var javaScriptSerilizer = new JavaScriptSerializer();
            switch (command.Name)
            {

                case "echo":

                    response.Result = command.Value;
                    string result = javaScriptSerilizer.Serialize(response);
                    _ws.WriteStringAsync(result, CancellationToken.None).Wait();
                    break;

                case "delayedEcho":

                    Thread.Sleep(10000);
                    response.Result = command.Value;
                    result = javaScriptSerilizer.Serialize(response);
                    _ws.WriteStringAsync(result, CancellationToken.None).Wait();
                    break;

                case "stopServer":

                    _listener.Stop();
                    break;

                case "cancel":

                    //  в разработке
                    break;
            }
            return true;
        }
    }
}
