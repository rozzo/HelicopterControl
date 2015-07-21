using HelicopterControl.Web.Tasks;
using System.Threading.Tasks;
using System.Web.Http;

namespace HelicopterControl.Web
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            Task.Run(() => new HelicopterWorker().Run());
        }

        protected void Application_End()
        {
            HelicopterWorker.Stop();
        }
    }
}
