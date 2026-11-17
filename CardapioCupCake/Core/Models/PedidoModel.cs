using System.Globalization;

namespace CardapioCupCake.Core.Models
{
    public class PedidoModel
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusDescricao { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public string EnderecoEntrega { get; set; } = string.Empty;
        public double Subtotal { get; set; }
        public double TaxaEntrega { get; set; }
        public double Total { get; set; }
        public List<ItemPedidoModel> Itens { get; set; } = new();

        public string Titulo => $"Pedido #{Id}";
        public string DataFormatada => CriadoEm.ToLocalTime().ToString("dd/MM/yyyy 'às' HH:mm");
        public string DisplayTotal => Total.ToString("C2", new CultureInfo("pt-BR"));
        public string ResumoItens => string.Join("\n", Itens.Select(i => $"{i.Quantidade}x {i.Nome}"));
        public bool PodeCancelar => Status == "Recebido";

        public Color CorStatus => Status switch
        {
            "Recebido" => Color.FromArgb("#8A5A00"),
            "EmPreparo" => Color.FromArgb("#A3164A"),
            "SaiuParaEntrega" => Color.FromArgb("#1F4F9C"),
            "Entregue" => Color.FromArgb("#1D6B47"),
            _ => Colors.Gray
        };
    }

    public class ItemPedidoModel
    {
        public int CupcakeId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Imagem { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public double PrecoUnitario { get; set; }
        public double Subtotal { get; set; }
    }
}
