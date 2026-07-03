using CareConnect.Mobile.Views;
using CareConnect.Mobile.Views.Auth;

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
        #endregion
    }
}