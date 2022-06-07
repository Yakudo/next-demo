using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Yakudo.Cqrs;
using Yakudo.Next.Application.Common.ExpandoModels.Services;
using Yakudo.Next.Application.Products.Products.Services;
using Yakudo.Next.Domain.Common.GeneratedNumbers.Services;
using Yakudo.Next.Domain.Common.Shared;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.Next.Framework.Abstractions.DataAnnotations;
using Yakudo.Next.Framework.Abstractions.Messaging;

// Komenda AddProduct -> Dodaje nowy produkt do bazy danych. Komendy implementują ICommand
// Handler obsuguje dany typ komendy. Ten tutaj wrzuca nowy rekord do bazy przy pomocy Entity Framework Core
// Komendy sa walidowane za pomocą biblioteki FluentValidator (na dole walidator)
// Wrzucenie wszystkiego do jednego pliku upraszcza nawigację po projekcie - od razu dostajesz Request (AddProductRequest), handler (AddProductHandler) i walidator (AddProductValidator)
// Jeżeli handler jest duży to wydzielamy część logiki do osobnych serwisów poza handler.
namespace Yakudo.Next.Application.Products.Products.Commands
{
    [Authorize(Policy = Policies.Products.ProductTypes.Add)]
    public class AddProductRequest : BaseProductCommand, ICommand<CreateResult>
    {
        public Guid BaseUnitId { get; set; }
        public ProductType ProductType { get; set; }

        public override string ToString()
        {
            return $"Add product {Id}";
        }
    }

    public class AddProductHandler : ICommandHandler<AddProductRequest, CreateResult>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IExpandoDataService _expandoDataService;
        private readonly IProductSettings _productSettings;
        private readonly INumberGeneratorService _numberGeneratorService;

        public AddProductHandler(IApplicationDbContext dbContext, IExpandoDataService expandoDataService, IProductSettings productSettings, INumberGeneratorService numberGeneratorService)
        {
            _dbContext = dbContext;
            _expandoDataService = expandoDataService;
            _productSettings = productSettings;
            _numberGeneratorService = numberGeneratorService;
        }

        public async Task<CreateResult> Handle(AddProductRequest command, CancellationToken cancellationToken = default)
        {
            var unit = await _dbContext.Set<Unit>().GetExistingAsync(m => m.Id == command.BaseUnitId, cancellationToken);
            var weightUnitId = command.WeightUnitId ?? _productSettings.DefaultWeightUnitId.Value;
            var weightUnit = await _dbContext.Set<Unit>().GetExistingAsync(m => m.Id == weightUnitId, cancellationToken);
            var sizeUnitId = command.SizeUnitId ?? _productSettings.DefaultSizeUnitId.Value;
            var sizeUnit = await _dbContext.Set<Unit>().GetExistingAsync(m => m.Id == sizeUnitId, cancellationToken);

            if (string.IsNullOrWhiteSpace(command.Code))
            {
                command.Code = await _numberGeneratorService.Generate(_productSettings.CodeGeneratorTemplate, null, cancellationToken);
            }

            var aggregate = new Product(
                command.Id,
                command.Name,
                command.Code,
                command.BaseUnitId,
                command.ProductType
            );
            aggregate.SetWeightUnit(weightUnit);
            aggregate.SetSizeUnit(sizeUnit);

            if (command.AttributesDataModelId != null)
            {
                var editor = await _expandoDataService.CreateEditor(command.AttributesDataModelId.Value);

                foreach (var property in command.Attributes)
                {
                    editor.TrySetValue(property.Key, property.Value);
                }

                aggregate.AttributesDataId = editor.DataId;
            }

            if (command.File != null)
            {
                aggregate.ChangeImage(command.File.Name, command.File.ReadArray());
            }

            aggregate.ChangeCategoryId(command.CategoryId);
            aggregate.ChangeWarehousingStrategy(command.WarehousingStrategy);
            aggregate.ChangeDescription(command.Description);
            aggregate.ChangeCatalogNumber(command.CatalogNumber);
            aggregate.ChangePkwiuCode(command.PkwiuCode);
            aggregate.ChangeCnCode(command.CnCode);
            aggregate.ChangeNumberGeneratorTemplateId(command.NumberGeneratorTemplateId);
            aggregate.SetAlternativeUnits(command.AlternativeUnits.Select(ProductAlternativeUnit.Convert));

            aggregate.ChangeQuantityInputModes(unit, command.QuantityInputModes.WrapFlags());
            aggregate.ChangeQuantityStep(command.QuantityStep);
            aggregate.ChangeHasWeight(command.HasWeight);
            aggregate.ChangeWeightInputModes(unit, command.WeightInputModes.WrapFlags());
            aggregate.ChangeWeight(command.NetWeight, command.Tare);
            aggregate.ChangeHasSize(command.HasSize);
            aggregate.ChangeSizeInputModes(unit, command.SizeInputModes.WrapFlags());
            aggregate.ChangeSize(command.Width, command.Height, command.Depth);

            aggregate.ChangeQuantityStep(command.QuantityStep);

            aggregate.ChangeDefaultUseByDate(command.DefaultUseByDate);
            aggregate.ChangeDefaultQuantity(command.DefaultQuantity);

            aggregate.ChangeIsFreezable(command.IsFreezable);
            aggregate.ChangeIsPerishable(command.IsPerishable);
            aggregate.SetWeightLimits(command.MinWeight, command.MaxWeight);
            aggregate.SetStorageTemperatures(command.MinStorageTemperature, command.MaxStorageTemperature);

            aggregate.ChangePurchaseTaxRate(command.PurchaseTaxRateId.Value);
            aggregate.ChangeSalesTaxRate(command.SalesTaxRateId.Value);

            await _dbContext.Set<Product>().AddAsync(aggregate);
            await _dbContext.SaveChangesAsync();

            return new CreateResult(aggregate.Id);
        }
    }

    public class AddProductValidator : AbstractValidator<AddProductRequest>
    {
        private readonly IApplicationDbContext _dbContext;

        public AddProductValidator(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.CnCode).MaximumLength(255);
            RuleFor(x => x.PkwiuCode).MaximumLength(255);
            RuleFor(x => x.CatalogNumber).MaximumLength(255);
            RuleFor(x => x.Code).MaximumLength(255).MustBeUnique(s => CheckCodeIsUnique(s));
            RuleFor(x => x.BaseUnitId).NotEmpty();
            RuleFor(x => x.File).MustHaveImageFile().MustHaveMaxFileSize().MustHaveMaxImageDimensions();
            RuleFor(m => m.AlternativeUnits).MustHaveUniqueItems(m => m.TargetUnitId);
            RuleForEach(m => m.AlternativeUnits).SetValidator(new ProductAlternativeUnitValidator());
            RuleFor(x => x.QuantityStep).GreaterThanOrEqualTo(0);
            RuleFor(x => x.PurchaseTaxRateId).NotEmpty();
            RuleFor(x => x.SalesTaxRateId).NotEmpty();
            RuleFor(x => x.DefaultUseByDate).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Tare).GreaterThanOrEqualTo(0);
            RuleFor(m => m).TargetUnitCannotBeTheSameAsBase();
            RuleFor(m => m.MinStorageTemperature).LessThanOrEqualTo(m => m.MaxStorageTemperature).When(n => n.MaxStorageTemperature != null);
            RuleFor(m => m.MinWeight).GreaterThanOrEqualTo(0);
            RuleFor(m => m.MinWeight).LessThanOrEqualTo(m => m.MaxWeight).When(n => n.MaxWeight != null);
            RuleFor(m => m.MaxWeight).GreaterThanOrEqualTo(0);
            RuleFor(m => m.DefaultUseByDate).GreaterThanOrEqualTo(0);
            RuleFor(m => m.Width).GreaterThanOrEqualTo(0);
            RuleFor(m => m.Height).GreaterThanOrEqualTo(0);
            RuleFor(m => m.Depth).GreaterThanOrEqualTo(0);
        }

        private bool CheckCodeIsUnique(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return true;
            return !_dbContext.Set<Product>().Any(m => m.Code == code);
        }
    }

    public static class AddProductValidatorExtensions
    {
        public static IRuleBuilderOptionsConditions<T, AddProductRequest> TargetUnitCannotBeTheSameAsBase<T>(this IRuleBuilder<T, AddProductRequest> ruleBuilder)
        {
            return ruleBuilder.Custom((list, context) =>
            {
                for (var i = 0; i < list.AlternativeUnits.Count; i++)
                {
                    if (list.BaseUnitId != default && list.AlternativeUnits[i].TargetUnitId == list.BaseUnitId)
                    {
                        var propertyName = $"{nameof(list.AlternativeUnits)}[{i}].{nameof(ProductAlternativeUnit.TargetUnitId)}";
                        context.AddFailure(propertyName, "Additional unit cannot be the same as base.");
                    }
                }
            });
        }
    }
}
