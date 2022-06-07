using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yakudo.Cqrs;
using Yakudo.Next.Application.Products.Products.Commands;
using Yakudo.Next.Domain.Common.Shared;
using Yakudo.Next.Domain.Products.ProductCategories;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.Next.Framework.Abstractions.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Yakudo.Next.Domain.Common.GeneratedNumbers;
using Yakudo.Next.Application.Common.ExpandoModels.Services.Config;
using Yakudo.Next.Application.Common.ExpandoModels.Services;
using Yakudo.Next.Application.Common.ExpandoModels.Services.Editor;

// Zapytania pobierające konkretny produkt/produkty z bazy danych
// Zapytania nie modyfikują stanu i implementują IQuery<ZwracanaOdpowiedź>
namespace Yakudo.Next.Application.Products.Products.Queries
{
    [Authorize(Policy = Policies.Products.ProductTypes.Display)]
    public class GetAllProductsRequest : IQuery<List<GetProductResponse>>
    {
    }

    [Authorize(Policy = Policies.Products.ProductTypes.Display)]
    public class GetProductsRequest : IQuery<List<GetProductResponse>>
    {
        public List<Guid> Ids { get; set; }
    }

    [Authorize(Policy = Policies.Products.ProductTypes.Display)]
    public class GetProductByIdRequest : IQuery<GetProductResponse>
    {
        public Guid Id { get; set; }
    }

    [Authorize(Policy = Policies.Products.ProductTypes.Display)]
    public class GetProductByCodeRequest : IQuery<GetProductResponse>
    {
        public string Code { get; set; }
    }

    [Authorize(Policy = Policies.Products.ProductTypes.Display)]
    public class GetProductsByCodeRequest : IQuery<List<GetProductResponse>>
    {
        public List<string> Codes { get; set; }
    }

    public class GetProductResponse
    {
        public Guid Id { get; set; }
        public long Version { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string CatalogNumber { get; set; }
        public string PkwiuCode { get; set; }
        public string CnCode { get; set; }

        public bool IsWeighted { get; set; }
        public Guid? BaseUnitId { get; set; }
        public string BaseUnitSymbol { get; set; }
        public string BaseUnitName { get; set; }

        public ProductType ProductType { get; set; }

        public byte[] Data { get; set; }

        public string FileName { get; set; }

        public string Description { get; set; }

        public Guid? CategoryId { get; set; }
        public string CategoryName { get; set; }

        public WarehousingStrategy WarehousingStrategy { get; set; }

        public List<ProductAlternativeUnit> AlternativeUnits { get; set; } = new List<ProductAlternativeUnit>();

        public Guid? AttributesDataModelId { get; set; }
        public List<ExpandoEditorFieldValue> Attributes { get; set; } = new List<ExpandoEditorFieldValue>();

        public bool IsFreezable { get; set; }
        public bool IsPerishable { get; set; }

        public int DefaultUseByDate { get; set; }
        public int DefaultFreezeDate { get; set; }
        public decimal DefaultQuantity { get; set; }

        public Guid? NumberGeneratorTemplateId { get; set; }

        public string NumberGeneratorTemplateName { get; set; }

        public List<QuantityInputModes> QuantityInputModes { get; set; } = new List<QuantityInputModes>();
        public bool HasWeight { get; set; }
        public List<WeightInputModes> WeightInputModes { get; set; } = new List<WeightInputModes>();
        public bool HasSize { get; set; }
        public List<SizeInputModes> SizeInputModes { get; set; } = new List<SizeInputModes>();

        public decimal NetWeight { get; set; }
        public decimal Tare { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Depth { get; set; }

        public double QuantityStep { get; set; } = 0.001;

        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }

        public double? MinStorageTemperature { get; set; }
        public double? MaxStorageTemperature { get; set; }

        public Guid PurchaseTaxRateId { get; set; }
        public decimal PurchaseTaxRateValue { get; set; }
        public string PurchaseTaxRateName { get; set; }

        public Guid SalesTaxRateId { get; set; }
        public decimal SalesTaxRateValue { get; set; }
        public string SalesTaxRateName { get; set; }
        public string WeightUnitName { get; set; }
        public string SizeUnitName { get; set; }

        public ExpandoEditorConfig EditorConfig { get; set; }
    }

    public class GetProductsQueryHandler :
        IQueryHandler<GetAllProductsRequest, List<GetProductResponse>>,
        IQueryHandler<GetProductsRequest, List<GetProductResponse>>,
        IQueryHandler<GetProductByIdRequest, GetProductResponse>,
        IQueryHandler<GetProductByCodeRequest, GetProductResponse>,
        IQueryHandler<GetProductsByCodeRequest, List<GetProductResponse>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IExpandoDataService _expandoDataService;

        public GetProductsQueryHandler(IApplicationDbContext dbContext, IExpandoDataService expandoDataService)
        {
            _dbContext = dbContext;
            _expandoDataService = expandoDataService;
        }

        public async Task<List<GetProductResponse>> Handle(GetAllProductsRequest query, CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<Product>().Include("_alternativeUnits").ToListAsync(cancellationToken);
            return await Map(items);
        }

        public async Task<List<GetProductResponse>> Handle(GetProductsRequest query, CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<Product>()
                .Include("_alternativeUnits")
                .Where(m => query.Ids.Contains(m.Id))
                .ToListAsync(cancellationToken);

            return await Map(items);
        }

        public async Task<List<GetProductResponse>> Handle(GetProductsByCodeRequest query, CancellationToken cancellationToken = default)
        {
            var items = await _dbContext.Set<Product>()
                .Include("_alternativeUnits")
                .Where(m => query.Codes.Contains(m.Code))
                .ToListAsync(cancellationToken);

            return await Map(items);
        }

        public async Task<GetProductResponse> Handle(GetProductByIdRequest query, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.Set<Product>().Include("_alternativeUnits").GetExistingAsync(m => m.Id == query.Id);
            return await GetOne(item);
        }

        public async Task<GetProductResponse> Handle(GetProductByCodeRequest query, CancellationToken cancellationToken = default)
        {
            var item = await _dbContext.Set<Product>().Include("_alternativeUnits").GetExistingAsync(m => m.Code == query.Code);
            return await GetOne(item);
        }

        private async Task<GetProductResponse> GetOne(Product item)
        {
            var result = await Map(item);

            var useCase = string.Empty;

            switch (result.ProductType)
            {
                case ProductType.Good:
                    useCase = Product.AttributesModelUseCaseGood;
                    break;

                case ProductType.Service:
                    useCase = Product.AttributesModelUseCaseService;
                    break;

                case ProductType.Container:
                    useCase = Product.AttributesModelUseCaseContainer;
                    break;

                case ProductType.Compound:
                    useCase = Product.AttributesModelUseCaseCompound;
                    break;
            }

            var editorConfig = await _expandoDataService.GetEditorConfig(useCase);

            result.EditorConfig = editorConfig;

            return result;
        }

        private async Task<List<GetProductResponse>> Map(List<Product> items)
        {
            var result = new List<GetProductResponse>();
            foreach (var item in items)
            {
                result.Add(await Map(item));
            }
            return result;
        }

        private async Task<GetProductResponse> Map(Product source)
        {
            var (attributesId, attributes) = await GetAttributes(source);

            var unitIds = source.AlternativeUnits.Select(m => m.TargetUnitId).ToList();
            unitIds.Add(source.BaseUnitId.Value);

            var units = await _dbContext.Set<Unit>().AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToListAsync();
            var unitsById = units.ToDictionary(m => m.Id);

            var baseUnit = unitsById[source.BaseUnitId.Value];

            var category = await _dbContext.Set<ProductCategory>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == source.CategoryId);
            var numberGeneratorTemplate = await _dbContext.Set<NumberGeneratorTemplate>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == source.NumberGeneratorTemplateId);
            var taxRates = await _dbContext.Set<TaxRate>().AsNoTracking().Where(p => p.Id == source.PurchaseTaxRateId || p.Id == source.SalesTaxRateId).ToListAsync();

            var purchaseTaxRate = taxRates.FirstOrDefault(p => p.Id == source.PurchaseTaxRateId);
            var salesTaxRate = taxRates.FirstOrDefault(p => p.Id == source.SalesTaxRateId);

            return new GetProductResponse
            {
                Id = source.Id,
                Version = source.Version,
                Name = source.Name,
                Code = source.Code,
                PkwiuCode = source.PkwiuCode,
                CatalogNumber = source.CatalogNumber,
                CnCode = source.CnCode,
                IsFreezable = source.IsFreezable,
                IsPerishable = source.IsPerishable,
                MaxWeight = source.MaxWeight,
                MinWeight = source.MinWeight,
                MaxStorageTemperature = source.MaxStorageTemperature,
                MinStorageTemperature = source.MinStorageTemperature,
                Description = source.Description,
                Data = source.ImageData,
                FileName = source.ImageFileName,
                BaseUnitId = source.BaseUnitId,
                BaseUnitName = baseUnit?.Name,
                BaseUnitSymbol = baseUnit?.Symbol,
                IsWeighted = baseUnit?.IsWeighted ?? false,
                CategoryId = source?.CategoryId,
                CategoryName = category?.Name,
                ProductType = source.ProductType,
                AlternativeUnits = source.AlternativeUnits.Select(p =>
                {
                    var alternativeUnit = ProductAlternativeUnit.Convert(p);
                    alternativeUnit.TargetUnitSymbol = unitsById[alternativeUnit.TargetUnitId].Symbol;
                    return alternativeUnit;
                }).ToList(),
                DefaultUseByDate = source.DefaultUseByDate,
                DefaultFreezeDate = source.DefaultFreezeDate,
                QuantityInputModes = source.QuantityInputModes.UnwrapFlags(),
                HasWeight = source.HasWeight,
                WeightInputModes = source.WeightInputModes.UnwrapFlags(),
                HasSize = source.HasSize,
                SizeInputModes = source.SizeInputModes.UnwrapFlags(),
                NetWeight = source.NetWeight,
                Tare = source.Tare,
                Width = source.Width,
                Height = source.Depth,
                Depth = source.Width,
                WarehousingStrategy = source.WarehousingStrategy,
                DefaultQuantity = source.DefaultQuantity,
                NumberGeneratorTemplateId = source.NumberGeneratorTemplateId,
                NumberGeneratorTemplateName = numberGeneratorTemplate?.Name,
                QuantityStep = source.QuantityStep,
                PurchaseTaxRateId = source.PurchaseTaxRateId,
                PurchaseTaxRateValue = purchaseTaxRate?.Value ?? default,
                PurchaseTaxRateName = purchaseTaxRate?.Name,
                SalesTaxRateId = source.SalesTaxRateId,
                SalesTaxRateValue = salesTaxRate?.Value ?? default,
                SalesTaxRateName = salesTaxRate?.Name,
                AttributesDataModelId = attributesId,
                Attributes = attributes,
                WeightUnitName = source?.WeightUnit.Name,
                SizeUnitName = source?.SizeUnit.Name,
            };
        }

        private async Task<(Guid?, List<ExpandoEditorFieldValue>)> GetAttributes(Product source)
        {
            if (source.AttributesDataId == null)
            {
                return (null, new List<ExpandoEditorFieldValue>());
            }

            var data = await _expandoDataService.GetEditor(source.AttributesDataId.Value);

            return (data.ModelId, data.GetValues());
        }
    }
}