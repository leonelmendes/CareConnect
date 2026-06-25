using CareConnect.Mobile.ViewModels.Auth;
namespace CareConnect.Mobile.Views.Auth;

public partial class RegisterStep1View : ContentPage
{
    private bool _isPasswordHidden = true;

    public RegisterStep1View(RegisterStep1ViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnShowPasswordTapped(object sender, TappedEventArgs e)
    {
        _isPasswordHidden = !_isPasswordHidden;
        
        passwordEntry.IsPassword = _isPasswordHidden;
        confirmPasswordEntry.IsPassword = _isPasswordHidden;
        
    }
}