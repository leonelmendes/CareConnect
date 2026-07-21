using CareConnect.Mobile.Views;
using CareConnect.Mobile.Views.Auth;
using CareConnect.Mobile.Views.Cuidador;
using CareConnect.Mobile.Views.Gestor;
using CareConnect.Mobile.Views.Shared;

namespace CareConnect.Mobile;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        #region Rotas
        Routing.RegisterRoute("LoginView", typeof(LoginView));
        Routing.RegisterRoute("RegisterView", typeof(RegisterStep1View));
        Routing.RegisterRoute("ForgotPasswordView", typeof(ForgotPasswordView));
        Routing.RegisterRoute("ProfileSelectionView", typeof(ProfileSelectionView));
        Routing.RegisterRoute("OnboardingView", typeof(OnboardingView));
        Routing.RegisterRoute("GestorPlanosView", typeof(GestorPlanosView));
        Routing.RegisterRoute("CriarPlanoCuidadoView", typeof(CriarPlanoCuidadoView));
        Routing.RegisterRoute("DetalhePlanoView", typeof(DetalhePlanoView));

        Routing.RegisterRoute("UtentesView", typeof(UtentesView));
        Routing.RegisterRoute("DetalheUtenteView", typeof(DetalheUtenteView));
        Routing.RegisterRoute("PerfilView", typeof(PerfilView));
        Routing.RegisterRoute("AdicionarUtenteView", typeof(AdicionarUtenteView));
        Routing.RegisterRoute("EditarUtenteView", typeof(EditarUtenteView));

        Routing.RegisterRoute("RegistoAdHocView", typeof(RegistoAdHocView));
        #endregion
    }
}