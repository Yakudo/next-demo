using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yakudo.Cqrs;
using Yakudo.Next.Application.Products.PriceLists.Queries;
using Yakudo.Next.Application.Products.Products.Commands;
using Yakudo.Next.Application.Products.Products.Queries;
using Yakudo.Next.Domain.Products.ExternalBarcodeMappings;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.Next.Framework.Abstractions.Messaging;
using Yakudo.Next.Server.Service.Areas.Products.Products.Components;
using Yakudo.Next.Server.Service.Controllers;
using Yakudo.Next.Shared.Web.Mvc;
using Yakudo.UI.DataTable;
using Yakudo.UI.DataTable.Models;

//
// Kontroler jest punktem wejścia - czysto technicznym kawałkiem kodu który przypina do danego adresu URL konkretne akcje: komendy/zapytania.
// Akcja w kodzie jest metodą kontrolera która dostaje komendę/zapytanie jako argument i zwraca odpowiedź HTTP.
// Komendy i zapytania są obiektami typu DTO - de facto strukturami danych (obiektami z samymi polami, nie zawierającymi logiki biznesowej).
// Komendy zmieniają stan systemu, zapytania odczytują stan systemu. Róznica ma charakter wyłącznie logiczny - jest to tzw. CQRS. 
// Pod spodem używamy wzorca mediator - mediator to pośrednik który uruchamia handler do danej komendy/zapytania poprzez wywołanie metody Handle(<co>) i waliduje poprzez Validate(<co>). 
// Używamy własnej implementacji mediatora, opensourcowym odpowiednikiem będzie biblioteka MediatR
//
// Z kontrolera zrobionego w ten sposób można automatycznie wygenerować bibliotekę klienckją oraz dokumentację - po to sa atrybuty ProducesResponseType. 
// Uzywamy Swaggera i Swashbuckle do tego.
//
namespace Yakudo.Next.Server.Service.Areas.Products.Products.Controllers
{
    [Area("Products")]
    [Route("products/products")]
    public class ProductsController : BaseController
    {
        public ProductsController(BaseControllerContext context) : base(context)
        {
        }

        #region DataTable

        [HttpPost("get-index")]
        public async Task<IActionResult> GetIndex()
        {
            var dataTable = new ProductsDataTable();
            var config = await dataTable.GetConfig();

            config.Url = Url.AbsoluteAction<ProductsController>(m => m.GetIndexData(null));
            return Json(config);
        }

        [HttpPost("get-index-data")]
        public async Task<IActionResult> GetIndexData([CqrsRequest] DataTablePageRequest request)
        {
            var dataTable = new ProductsDataTable();

            var data = await dataTable.GetDataAsync(DbContext.Set<Product>().Include(m => m.Category), request);
            return Json(data);
        }

        #endregion DataTable

        #region Get all

        [HttpPost("get-all")]
        [ProducesResponseType(typeof(List<GetProductResponse>), StatusCodes.Status200OK)]
        public Task<IActionResult> GetProducts(CancellationToken cancellationToken = default) => this.Handle(new GetAllProductsRequest());

        #endregion Get all

        #region Get one

        [HttpPost("get-by-id")]
        [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
        public Task<IActionResult> GetProductById([CqrsRequest] GetProductByIdRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        #endregion Get one

        #region Add

        [HttpPost("add/form")]
        [ProducesResponseType(typeof(GetAddProductFormResponse), StatusCodes.Status200OK)]
        public Task<IActionResult> AddProduct([CqrsRequest] GetAddProductFormRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("add")]
        [ProducesResponseType(typeof(CreateResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> AddProduct([CqrsRequest] AddProductRequest request, CancellationToken cancellationToken = default) => this.Handle<CreateResult>(request, cancellationToken);

        [HttpPost("add/validation")]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status200OK)]
        public Task<IActionResult> AddProductValidate([CqrsRequest] AddProductRequest request) => this.Validate(request);

        #endregion Add

        #region Change

        [HttpPost("change/form")]
        [ProducesResponseType(typeof(GetChangeProductFormResponse), StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeProduct([CqrsRequest] GetChangeProductFormRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("change")]
        [ProducesResponseType(typeof(CreateResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> ChangeProduct([CqrsRequest] ChangeProductRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("change/validation")]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeProductValidate([CqrsRequest] ChangeProductRequest request) => this.Validate(request);

        #endregion Change

        #region Delete

        [HttpPost("delete")]
        [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status400BadRequest)]
        public Task<IActionResult> DeleteProduct([CqrsRequest] DeleteProductRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("delete/validation")]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status200OK)]
        public Task<IActionResult> DeleteProductValidate([CqrsRequest] DeleteProductRequest request) => this.Validate(request);

        #endregion Delete

        #region PriceList

        [HttpPost("price-list")]
        [ProducesResponseType(typeof(GetChangeProductPriceListFormResponse), StatusCodes.Status200OK)]
        public Task<IActionResult> EditPriceList([CqrsRequest] GetChangeProductPriceListFormRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("price-list/change")]
        [ProducesResponseType(typeof(CreateResult), StatusCodes.Status200OK)]
        public Task<IActionResult> ChangePriceList([CqrsRequest] ChangeProductPriceListRequest request, CancellationToken cancellationToken = default) => this.Handle(request, cancellationToken);

        [HttpPost("get-missing-entries")]
        [ProducesResponseType(typeof(GetProductPriceListEntriesResponse), StatusCodes.Status200OK)]
        public Task<IActionResult> AddPriceListValidate([CqrsRequest] GetProductPriceListEntriesRequest request) => this.Handle(request);

        [HttpPost("price-list/change/validation")]
        [ProducesResponseType(typeof(ValidationSummary), StatusCodes.Status200OK)]
        public Task<IActionResult> ChangeProductPriceListValidate([CqrsRequest] ChangeProductPriceListRequest request) => this.Validate(request);

        #endregion PriceList

        #region External Barcode Mappings

        [HttpPost("get-external-barcode-mappings-index/{productId}")]
        public async Task<IActionResult> GetIndexPerProduct(Guid productId)
        {
            var dataTable = new ProductExternalBarcodeMappingsDataTable();
            var config = await dataTable.GetConfig();

            config.Url = Url.AbsoluteAction<ProductsController>(m => m.GetIndexDataPerProduct(null, productId));
            return Json(config);
        }

        [HttpPost("get-external-barcode-mappings-index-data/{productId}")]
        public async Task<IActionResult> GetIndexDataPerProduct([CqrsRequest] DataTablePageRequest request, Guid productId)
        {
            var dataTable = new ProductExternalBarcodeMappingsDataTable();

            var data = await dataTable.GetDataAsync(DbContext.Set<ExternalBarcodeMapping>().Where(m => m.ProductId == productId), request);
            return Json(data);
        }

        #endregion External Barcode Mappings
    }
}
