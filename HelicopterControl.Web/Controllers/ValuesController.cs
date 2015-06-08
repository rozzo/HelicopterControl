using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using HelicopterControl.Web.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HelicopterControl.Web.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpPost]
        public void Update(HelicopterState state)
        {
            Debug.WriteLine("Throttle:{0}\tPitch:{1}\tYaw:{2}", state.Throttle, state.Pitch, state.Yaw);
        }

        [HttpGet]
        public HttpResponseMessage WebSocket()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                HttpContext.Current.AcceptWebSocketRequest(ProcessWebSocketRequest);
            }
            return new HttpResponseMessage(HttpStatusCode.SwitchingProtocols);
        }

        private async Task ProcessWebSocketRequest(AspNetWebSocketContext context)
        {
            WebSocket socket = context.WebSocket;
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (socket.State != WebSocketState.Open) break;


                string userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                JObject state = JObject.Parse(userMessage);
                if (state != null)
                {
                    //var response = string.Format("Throttle:{0}\tPitch:{1}\tYaw:{2}", state["Throttle"], state["Pitch"], state["Yaw"]);
                    Debug.WriteLine("Throttle:{0}\tPitch:{1}\tYaw:{2}", state["Throttle"], state["Pitch"], state["Yaw"]);
                    
                }
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMessage));
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
