using System.Collections.ObjectModel;
using CardapioCupCake.Core.Models;

namespace CardapioCupCake.Core.Service
{
    /// <summary>
    /// Carrinho único do app, compartilhado entre as telas de detalhes, cardápio e carrinho.
    /// (No PI I cada tela criava sua própria lista, por isso o carrinho aparecia sempre vazio.)
    /// </summary>
    public static class CarrinhoService
    {
        public const int QuantidadeMaxima = 50;

        public static ObservableCollection<ItensModel> Itens { get; } = new();

        public static event EventHandler? Alterado;

        public static int QuantidadeTotal => Itens.Sum(i => i.Quantity);
        public static double Subtotal => Itens.Sum(i => i.TotalPrice);

        public static void Adicionar(CupcakeModel cupcake, int quantidade)
        {
            if (cupcake == null || quantidade < 1) return;

            var existente = Itens.FirstOrDefault(i => i.Item.Id == cupcake.Id);
            if (existente != null)
            {
                var indice = Itens.IndexOf(existente);
                Itens[indice] = new ItensModel
                {
                    Item = cupcake,
                    Quantity = Math.Min(QuantidadeMaxima, existente.Quantity + quantidade)
                };
            }
            else
            {
                Itens.Add(new ItensModel { Item = cupcake, Quantity = Math.Min(QuantidadeMaxima, quantidade) });
            }
            Alterado?.Invoke(null, EventArgs.Empty);
        }

        public static void AlterarQuantidade(ItensModel item, int delta)
        {
            var indice = Itens.IndexOf(item);
            if (indice < 0) return;

            var nova = item.Quantity + delta;
            if (nova < 1)
                Itens.RemoveAt(indice);
            else
                Itens[indice] = new ItensModel { Item = item.Item, Quantity = Math.Min(QuantidadeMaxima, nova) };

            Alterado?.Invoke(null, EventArgs.Empty);
        }

        public static void Limpar()
        {
            Itens.Clear();
            Alterado?.Invoke(null, EventArgs.Empty);
        }
    }
}
