using CardapioCupCake.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CardapioCupCake.Core.Service
{
    public partial class ListCupCakeRepository
    {
        public ObservableCollection<CupcakeModel> Cupcakes { get; } = new ObservableCollection<CupcakeModel>();
        public ObservableCollection<CadastroModel> User { get; } = new ObservableCollection<CadastroModel>();

        public ListCupCakeRepository()
        {

        }
        public void AddCupcakesCarrinho(CupcakeModel cupcake)
        {
            if (cupcake != null)
            {
                cupcake.Id = Cupcakes.Count + 1;
                Cupcakes.Add(cupcake);
            }
        }
        public void AddCupcakesCarrinho(CadastroModel user)
        {
            if (user != null)
            {
                user.Id = User.Count + 1;
                User.Add(user);
            }
        }
        public List<CupcakeModel> GetAll()
        {
            try
            {
                return Cupcakes.ToList();
            }
            catch (Exception)
            {
                return new List<CupcakeModel>();
            }
        }
        public void UpdateUser(CadastroModel user)
        {
            var index = User.IndexOf(User.FirstOrDefault(u => u.Id == user.Id));
            if (index >= 0)
            {
                User[index] = user;
            }
        }

        /// <summary>
        /// Valida os dados do perfil antes de enviar para a API.
        /// Correções do PI I: o campo Senha era validado como CPF (sempre falhava) e o telefone
        /// exigia a máscara "(41) 99999-9999", mas o cadastro grava só os dígitos.
        /// </summary>
        public async Task<bool> ValidateUserAsync(CadastroModel user)
        {
            string? erro = null;

            if (string.IsNullOrWhiteSpace(user.Nome) || user.Nome.Trim().Length < 3)
                erro = "Informe o nome completo.";
            else if (string.IsNullOrWhiteSpace(user.Email) || !Regex.IsMatch(user.Email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                erro = "Informe um e-mail válido.";
            else if (!IsValidPhoneNumber(user.Telefone))
                erro = "Número de telefone inválido. Use DDD + número.";
            else if (string.IsNullOrWhiteSpace(user.Endereco))
                erro = "Por favor, preencha o endereço.";

            if (erro != null)
            {
                await Shell.Current.DisplayAlert("Erro", erro, "OK");
                return false;
            }
            return true;
        }

        private static bool IsValidPhoneNumber(string? number)
        {
            var digitos = Regex.Replace(number ?? string.Empty, @"\D", "");
            return digitos.Length is 10 or 11;
        }
    }
}
