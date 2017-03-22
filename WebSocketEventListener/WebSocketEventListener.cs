using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vtortola.WebSockets;
using vtortola.WebSockets.Rfc6455;

namespace vtortola.WebSockets.Listener
{
    public delegate void WebSocketEventListenerOnConnect(WebSocket webSocket);
    public delegate void WebSocketEventListenerOnDisconnect(WebSocket webSocket);
    public delegate void WebSocketEventListenerOnMessage(WebSocket webSocket, string message);
    public delegate void WebSocketEventListenerOnError(WebSocket webSocket, Exception error);

    public class WebSocketEventListener : IDisposable
    {
        
        public event WebSocketEventListenerOnConnect OnConnect;
        public event WebSocketEventListenerOnDisconnect OnDisconnect;
        public event WebSocketEventListenerOnMessage OnMessage;
        public event WebSocketEventListenerOnError OnError;

        readonly WebSocketListener _listener;
        //private CancellationTokenSource _cancellation;

        public WebSocketEventListener(IPEndPoint endpoint)
            : this(endpoint, new WebSocketListenerOptions())
        {
        }
        public WebSocketEventListener(IPEndPoint endpoint, WebSocketListenerOptions options)
        {            
            _listener = new WebSocketListener(endpoint, options);
            _listener.Standards.RegisterStandard(new WebSocketFactoryRfc6455(_listener));
        }
        //public void Start(CancellationTokenSource cancellation)
        public void Start()
        {
            //_cancellation = cancellation;
            _listener.Start();
            Task.Run((Func<Task>)ListenAsync);
            //Task.Run(() => ListenAsync(_cancellation.Token));
        }
        public void Stop()
        {
            _listener.Stop();
        }
        
        private async Task ListenAsync()
        {
            while (_listener.IsStarted)
            {
                try
                {
                    var websocket = await _listener.AcceptWebSocketAsync(CancellationToken.None)
                                                   .ConfigureAwait(false);
                    if (websocket != null)
                        Task.Run(() => HandleWebSocketAsync(websocket));  // add await!!!
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError.Invoke(null, ex);
                }
            }
        }
        
        /*
        private async Task ListenAsync(CancellationToken token)
        {
            while (_listener.IsStarted && !token.IsCancellationRequested)
            {
                try
                {
                    var websocket = await _listener.AcceptWebSocketAsync(token)
                                                   .ConfigureAwait(false);
                    if (websocket != null)
                        Task.Run(() => HandleWebSocketAsync(websocket, token));  // add await!!!
                }
                catch (Exception ex)
                {
                    if (OnError != null)
                        OnError.Invoke(null, ex);
                }
            }
        }
        */
        
        private async Task HandleWebSocketAsync(WebSocket websocket)
        {
            try
            {
                if (OnConnect != null)
                    OnConnect.Invoke(websocket);

                while (websocket.IsConnected)
                {
                    var message = await websocket.ReadStringAsync(CancellationToken.None)
                                                 .ConfigureAwait(false);
                    if (message != null && OnMessage != null)
                        OnMessage.Invoke(websocket, message);
                }

                if (OnDisconnect != null)
                    OnDisconnect.Invoke(websocket);
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError.Invoke(websocket, ex);
            }
            finally
            {
                websocket.Dispose();
            }
        }
        
        /*
        private async Task HandleWebSocketAsync(WebSocket websocket, CancellationToken cancellation)
        {
            try
            {
                if (OnConnect != null)
                    OnConnect.Invoke(websocket);

                while (websocket.IsConnected && !cancellation.IsCancellationRequested)
                {
                    var message = await websocket.ReadStringAsync(cancellation)
                                                 .ConfigureAwait(false);
                    if (message != null && OnMessage != null)
                        OnMessage.Invoke(websocket, message);
                }

                if (OnDisconnect != null)
                    OnDisconnect.Invoke(websocket);
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError.Invoke(websocket, ex);
            }
            finally
            {
                websocket.Dispose();
            }
        }
        */
        public void Dispose()
        {
            _listener.Dispose();
        }
    }
}
