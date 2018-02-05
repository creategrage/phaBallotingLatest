using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(phaBalloting.Startup))]
namespace phaBalloting
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
