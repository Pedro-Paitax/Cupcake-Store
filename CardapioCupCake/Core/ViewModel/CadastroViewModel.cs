using CardapioCupCake.Core.Models;
using CardapioCupCake.Core.Service;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace CardapioCupCake.Core.ViewModel
{
    public partial class CadastroViewModel : BaseViewModel
    {
        private readonly HubService hubService = new();
        private CadastroModel usuario = new();

        public string Nome
        {
            get => usuario.Nome;
            set { usuario.Nome = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => usuario.Email;
            set { usuario.Email = value; OnPropertyChanged(); }
        }

        public string Senha
        {
            get => usuario.Senha;
            set { usuario.Senha = value; OnPropertyChanged(); }
        }

        public string Endereco
        {
            get => usuario.Endereco;
            set { usuario.Endereco = value; OnPropertyChanged(); }
        }

        public string Telefone
        {
            get => usuario.Telefone;
            set { usuario.Telefone = value; OnPropertyChanged(); }
        }

        private string confirmarSenha = string.Empty;
        public string ConfirmarSenha
        {
            get => confirmarSenha;
            set { confirmarSenha = value; OnPropertyChanged(); }
        }

        private string mensagem = string.Empty;
        public string Mensagem
        {
            get => mensagem;
            set
            {
                mensagem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMensagemVisible));
            }
        }

        public bool IsMensagemVisible => !string.IsNullOrWhiteSpace(Mensagem);

        public ICommand CadastrarCommand { get; }

        public CadastroViewModel()
        {
            CadastrarCommand = new AsyncRelayCommand(CadastrarAsync);
        }

        /// <summary>Validação local (resposta imediata). A API valida tudo de novo.</summary>
        public string? Validar()
        {
            if (string.IsNullOrWhiteSpace(Nome) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Senha) ||
                string.IsNullOrWhiteSpace(ConfirmarSenha) ||
                string.IsNullOrWhiteSpace(Telefone))
                return "Preencha todos os campos obrigatórios!";

            if (Nome.Trim().Length < 3)
                return "Informe o nome completo.";

            if (!Regex.IsMatch(Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                return "O formato do E-mail é inválido. Por favor, verifique.";

            var telefoneLimpo = Regex.Replace(Telefone, @"[^\d]", "");
            if (telefoneLimpo.Length < 10 || telefoneLimpo.Length > 11)
                return "Telefone incorreto. Deve conter 10 ou 11 dígitos (com DDD).";

            if (Senha.Length < 8)
                return "A senha deve ter no mínimo 8 caracteres.";

            if (Senha != ConfirmarSenha)
                return "As senhas digitadas não são iguais. Por favor, verifique.";

            return null;
        }

        private async Task CadastrarAsync()
        {
            if (IsBusy) return;
            Mensagem = Validar() ?? string.Empty;
            if (IsMensagemVisible) return;

            IsBusy = true;
            try
            {
                var novoUsuario = new CadastroModel
                {
                    Nome = Nome.Trim(),
                    Email = Email.Trim(),
                    Senha = Senha,                       // a API grava apenas o hash
                    Endereco = Endereco?.Trim() ?? string.Empty,
                    Telefone = Regex.Replace(Telefone, @"[^\d]", "")
                };

                var (ok, erro) = await hubService.CadastrarAsync(novoUsuario);
                if (!ok)
                {
                    Mensagem = erro ?? "Não foi possível concluir o cadastro.";
                    return;
                }

                await Shell.Current.DisplayAlert("Conta criada! 🧁", "Cadastro feito com sucesso. Agora é só entrar com seu e-mail e senha.", "OK");

                usuario = new CadastroModel();
                ConfirmarSenha = string.Empty;
                OnPropertyChanged(nameof(Nome));
                OnPropertyChanged(nameof(Email));
                OnPropertyChanged(nameof(Senha));
                OnPropertyChanged(nameof(Endereco));
                OnPropertyChanged(nameof(Telefone));

                await Shell.Current.CurrentPage.Navigation.PopAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
