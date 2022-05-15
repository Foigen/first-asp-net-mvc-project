using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Webmarket.Startup))]
namespace Webmarket
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
