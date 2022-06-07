using System;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.UI.DataTable.Models;

namespace Yakudo.Next.Server.Service.Areas.Products.Products.Components
{
    public class ProductsDataTableRow : DataTableRow
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public ProductType ProductType { get; set; }

        public string CategoryName { get; set; }

        public string CatalogNumber { get; set; }
    }
}
