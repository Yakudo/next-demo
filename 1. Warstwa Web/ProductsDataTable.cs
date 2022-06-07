using Yakudo.Next.Domain.Products.Products;
using Yakudo.UI.DataTable;

// Data table - klasa helper do tworzenia sortowalnych/filtrowalnych tabel z automatyczną paginacją. 
// Mapuje obiekt z bazy danych (Product) na wiersz tabeli (ProductsDataTableRow) przy pomocy AddColumn i dodaje autogenerowane
// filtry poprzez AddFilter.
//
// Tutaj akurat jest to trywialna implementacja, ale może być taka która coś robi z danymi żeby je wyświetlić w inny sposób.
namespace Yakudo.Next.Server.Service.Areas.Products.Products.Components
{
    public class ProductsDataTable : DataTable<Product, ProductsDataTableRow>
    {
        public ProductsDataTable() : base()
        {
        }

        protected override void Configure(DataTableBuilder<Product, ProductsDataTableRow> builder)
        {
            builder.AddColumn(s => s.Id, v => v.Id).SetInternal();
            builder.AddColumn(s => s.Name, v => v.Name);
            builder.AddColumn(s => s.Code, v => v.Code);
            builder.AddColumn(s => s.ProductType, v => v.ProductType);
            builder.AddColumn(s => s.Category.Name, v => v.CategoryName);
            builder.AddColumn(s => s.CatalogNumber, v => v.CatalogNumber);

            builder.AddFilter(s => s.Category.Name);
            builder.AddFilter(s => s.Name);
            builder.AddFilter(s => s.Code);
            builder.AddFilter(s => s.ProductType);
            builder.AddFilter(s => s.WarehousingStrategy);
            builder.AddFilter(s => s.IsFreezable);
            builder.AddFilter(s => s.IsPerishable);
            builder.AddFilter(s => s.CatalogNumber);
            builder.AddFilter(s => s.PkwiuCode);
            builder.AddFilter(s => s.CnCode);
            builder.AddFilter(m => m.CreatedDate);
            builder.AddFilter(m => m.LastModified);
        }
    }
}
