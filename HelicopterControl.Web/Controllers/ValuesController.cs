using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
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
using HelicopterControl.Web.Tasks;

namespace HelicopterControl.Web.Controllers
{
    public class ValuesController : ApiController
    {
        /*
        [HttpPost]
        public void Update(HelicopterState state)
        {
            Debug.WriteLine("Throttle:{0}\tPitch:{1}\tYaw:{2}", state.Throttle, state.Pitch, state.Yaw);
        }
         * */

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
            var socket = context.WebSocket;
            while (true)
            {
                /*
                if (!HelicopterWorker.ProcessCommands)
                {
                    Task.Run(() => new HelicopterWorker().Run());
                }
                 * */

                var buffer = new ArraySegment<byte>(new byte[1024]);
                var result = await socket.ReceiveAsync(buffer, CancellationToken.None);
                if (socket.State != WebSocketState.Open) break;

                var userMessage = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                var state = JObject.Parse(userMessage);
                if (state != null)
                {
                    //var response = string.Format("Throttle:{0}\tPitch:{1}\tYaw:{2}", state["Throttle"], state["Pitch"], state["Yaw"]);
                    Debug.WriteLine("Throttle:{0}\tPitch:{1}\tYaw:{2}", state["Throttle"], state["Pitch"], state["Yaw"]);
                    HelicopterState.SetValues(false, Convert.ToInt32(state["Throttle"].ToString()), Convert.ToInt32(state["Pitch"].ToString()), Convert.ToInt32(state["Yaw"].ToString()), 0);
                }
                buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(userMessage));
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}
