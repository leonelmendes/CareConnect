using CareConnect.Mobile.Views.Cuidador;

namespace CareConnect.Mobile.Shells;

public partial class CuidadorShell : Shell
{
	public CuidadorShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("RegistoAdHocView", typeof(RegistoAdHocView));
	}
}