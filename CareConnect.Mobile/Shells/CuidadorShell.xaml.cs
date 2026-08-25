using CareConnect.Mobile.Views.Cuidador;
using CareConnect.Mobile.Views.Shared;

namespace CareConnect.Mobile.Shells;

public partial class CuidadorShell : Shell
{
	public CuidadorShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("RegistoAdHocView", typeof(RegistoAdHocView));
        Routing.RegisterRoute("DetalheUtenteView", typeof(DetalheUtenteView));
    }
}