using CardapioCupCake.Core.Service;
using CardapioCupCake.Core.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class MainViewModel : BaseViewModel
    {
        private readonly HubService _hubService = new();

        [ObservableProperty]
        private string email = string.Empty;

        [ObservableProperty]
        private string senha = string.Empty;

        public IAsyncRelayCommand LoginCommand { get; }

        public MainViewModel()
        {
            LoginCommand = new AsyncRelayCommand(OnLoginClicked);
        }

        private async Task OnLoginClicked()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Senha))
            {
                await Shell.Current.DisplayAlert("Aviso!", "Por favor, preencha o e-mail e a senha.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var (ok, erro) = await _hubService.LoginAsync(Email.Trim(), Senha);
                if (!ok)
                {
                    await Shell.Current.DisplayAlert("Aviso!", erro ?? "E-mail ou senha inválidos. Tente novamente.", "OK");
                    return;
                }

                Senha = string.Empty;
                await Shell.Current.CurrentPage.Navigation.PushAsync(new CardapioPage(new CardapioViewModel()));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
