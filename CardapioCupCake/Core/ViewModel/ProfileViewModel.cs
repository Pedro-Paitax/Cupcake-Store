using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CardapioCupCake.Core.View;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class ProfileViewModel : BaseViewModel
    {
        private readonly HubService _hubService = new();
        private readonly ListCupCakeRepository _validador = new();

        [ObservableProperty]
        private CadastroModel userProfile;

        [ObservableProperty]
        private string profileImageUrl = "perfil.png";

        public ProfileViewModel()
        {
            var atual = Sessao.Usuario ?? new CadastroModel();
            // Edita uma cópia: se o usuário desistir, a sessão não muda.
            UserProfile = new CadastroModel
            {
                Id = atual.Id,
                Nome = atual.Nome,
                Email = atual.Email,
                Telefone = atual.Telefone,
                Endereco = atual.Endereco
            };
        }

        [RelayCommand]
        public async Task Update()
        {
            if (IsBusy) return;
            if (!await _validador.ValidateUserAsync(UserProfile)) return;

            IsBusy = true;
            try
            {
                var (ok, erro) = await _hubService.AtualizarPerfilAsync(UserProfile);
                if (!ok)
                {
                    await Shell.Current.DisplayAlert("Aviso", erro ?? "Não foi possível salvar.", "OK");
                    return;
                }
                await Shell.Current.DisplayAlert("Sucesso!", "Perfil atualizado com sucesso.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task MeusPedidos()
        {
            await Shell.Current.CurrentPage.Navigation.PushAsync(new MeusPedidosPage(new MeusPedidosViewModel()));
        }

        [RelayCommand]
        private async Task Sair()
        {
            var confirmar = await Shell.Current.DisplayAlert("Sair", "Deseja sair da sua conta?", "Sair", "Cancelar");
            if (!confirmar) return;
            Sessao.Encerrar();
            await Shell.Current.CurrentPage.Navigation.PopToRootAsync();
        }

        [RelayCommand]
        private async Task SelectPhoto()
        {
            try
            {
                var result = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Selecione a foto de perfil"
                });
                if (result != null)
                    ProfileImageUrl = result.FullPath;
            }
            catch (Exception)
            {
                await Shell.Current.DisplayAlert("Erro", "Seleção de foto não suportada neste dispositivo.", "OK");
            }
        }
    }
}
