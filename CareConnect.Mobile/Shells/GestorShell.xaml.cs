using CareConnect.Mobile.Views.Gestor;
using CareConnect.Mobile.Views.Shared;

namespace CareConnect.Mobile.Shells;

public partial class GestorShell : Shell
{
	public GestorShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("UtentesView", typeof(UtentesView));
        Routing.RegisterRoute("DetalheUtenteView", typeof(DetalheUtenteView));
        Routing.RegisterRoute("PerfilView", typeof(PerfilView));
        Routing.RegisterRoute("AdicionarUtenteView", typeof(AdicionarUtenteView));
        Routing.RegisterRoute("GestorPlanosView", typeof(GestorPlanosView));
        Routing.RegisterRoute("CriarPlanoCuidadoView", typeof(CriarPlanoCuidadoView));
        Routing.RegisterRoute("DetalhePlanoView", typeof(DetalhePlanoView));
    }
}