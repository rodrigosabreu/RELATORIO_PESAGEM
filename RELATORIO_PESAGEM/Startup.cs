using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RELATORIO_PESAGEM.Startup))]
namespace RELATORIO_PESAGEM
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
