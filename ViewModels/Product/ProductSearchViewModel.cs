using System.Collections.Generic;
using ShoeStore.ViewModels.Home;

namespace ShoeStore.ViewModels.Product
{
    public class ProductSearchViewModel
    {
        public string? Query { get; set; }
        public List<ProductCardViewModel> Results { get; set; } = new();
        public List<ProductCardViewModel> SuggestedProducts { get; set; } = new();

        public bool HasQuery => !string.IsNullOrWhiteSpace(Query);
        public int ResultCount => Results.Count;
    }
}
