using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BobTheGrader.Startup))]
namespace BobTheGrader
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
