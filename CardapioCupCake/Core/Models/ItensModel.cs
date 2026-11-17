using System.Globalization;

namespace CardapioCupCake.Core.Models
{
    public class ItensModel
    {
        public CupcakeModel Item { get; set; } = new();
        public int Quantity { get; set; }

        public double TotalPrice => Quantity * Item.Price;

        public string DisplayName => $"{Quantity}x {Item.Name}";
        public string DisplayTotal => TotalPrice.ToString("C2", new CultureInfo("pt-BR"));
    }
}
