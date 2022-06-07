using System;
using System.Collections.Generic;
using System.Linq;
using Yakudo.Cqrs.Exceptions;
using Yakudo.Next.Domain.Common.Base;
using Yakudo.Next.Domain.Common.ExpandoModels;
using Yakudo.Next.Domain.Common.Shared;
using Yakudo.Next.Domain.Products.ProductCategories;
using Yakudo.Next.Domain.Products.Products.Events;
using Yakudo.Next.Domain.Products.ProductTypes;
using Yakudo.Next.Framework.Abstractions;
using Yakudo.Next.Framework.Abstractions.Models;

// Obiekt reprezentujący produkt w bazie danych. Ma on kilka ról:
//
// 1. Waliduje wprowadzane dane np. poprzez CheckState.Verify lub wyjątek
// 2. Generuje zdarzenia które zapewniają audytowalność lub robia coś w systemie np. ChangeName<ProductNameChanged>(name);
//
namespace Yakudo.Next.Domain.Products.Products
{
    public class Product : BaseCodeNameAggregateRoot
    {
        public const string AttributesModelUseCaseGood = "Product.Good.Attributes";
        public const string AttributesModelUseCaseContainer = "Product.Container.Attributes";
        public const string AttributesModelUseCaseService = "Product.Service.Attributes";
        public const string AttributesModelUseCaseCompound = "Product.Compound.Attributes";

        public Guid? AttributesDataId { get; set; }

        public Guid? CategoryId { get; set; }
        public virtual ProductCategory Category { get; set; }

        public ProductType ProductType { get; private set; }

        /// <summary>
        /// Bazowa jednostka w której są stany magazynowe
        /// </summary>
        public Guid? BaseUnitId { get; private set; }

        /// <summary>
        /// Bazowa jednostka w której są stany magazynowe
        /// </summary>
        public virtual Unit BaseUnit { get; set; }

        public Guid? WeightUnitId { get; private set; }

        /// <summary>
        /// Jednostka masy
        /// </summary>
        public virtual Unit WeightUnit { get; set; }

        public Guid? SizeUnitId { get; private set; }

        /// <summary>
        /// Jednostka rozmiaru
        /// </summary>
        public virtual Unit SizeUnit { get; set; }

        public WarehousingStrategy WarehousingStrategy { get; private set; }

        public string ImageFileName { get; private set; }

        public byte[] ImageData { get; private set; }

        /// <summary>
        /// Opis produktu (dowolny tekst)
        /// </summary>
        public string Description { get; set; }

        public string CatalogNumber { get; private set; }

        /// <summary>
        /// Numer w klasywikacji PKWIU
        /// </summary>
        public string PkwiuCode { get; private set; }

        /// <summary>
        /// Kod CN - usystematyzowana klasyfikacja towarów wykorzystywana na potrzeby ustalenia odpowiedniej stawki cła
        /// </summary>
        public string CnCode { get; private set; }

        protected virtual List<ProductAlternativeUnit> _alternativeUnits { get; set; } = new List<ProductAlternativeUnit>();

        public IReadOnlyList<ProductAlternativeUnit> AlternativeUnits => _alternativeUnits.ToList().AsReadOnly();

        public Guid? NumberGeneratorTemplateId { get; private set; }

        public QuantityInputModes QuantityInputModes { get; private set; }
        public bool HasWeight { get; private set; }
        public WeightInputModes WeightInputModes { get; private set; }
        public bool HasSize { get; private set; }
        public SizeInputModes SizeInputModes { get; private set; }

        public decimal NetWeight { get; private set; }
        public decimal Tare { get; private set; }
        public decimal Width { get; private set; }
        public decimal Height { get; private set; }
        public decimal Depth { get; private set; }

        public int DefaultUseByDate { get; private set; }
        public int DefaultFreezeDate { get; private set; }

        public double QuantityStep { get; set; } = 0.001;
        public decimal DefaultQuantity { get; set; }

        /// <summary>
        /// Freezable product has freeze date
        /// </summary>
        public bool IsFreezable { get; private set; }

        /// <summary>
        /// Perishable product has use by date
        /// </summary>
        public bool IsPerishable { get; private set; }

        public double? MinWeight { get; set; }
        public double? MaxWeight { get; set; }

        public double? MinStorageTemperature { get; set; }
        public double? MaxStorageTemperature { get; set; }

        public Guid PurchaseTaxRateId { get; set; }
        public virtual TaxRate PurchaseTaxRate { get; set; }

        public Guid SalesTaxRateId { get; set; }
        public virtual TaxRate SalesTaxRate { get; set; }

        public Guid? HandlingUnitItemDataModelId { get; set; }
        public virtual ExpandoModel HandlingUnitItemDataModel { get; set; }

        protected Product() : base()
        {
        }

        public Product(string name, string code, Guid baseUnitId, ProductType productType) : this(IdGenerator.NewId(), name, code, baseUnitId, productType)
        {
        }

        public Product(Guid id, string name, string code, Guid baseUnitId, ProductType productType) : base(id)
        {
            ChangeName(name);
            ChangeCode(code);
            BaseUnitId = baseUnitId;
            ProductType = productType;
        }

        public void ChangeName(string name) => ChangeName<ProductNameChanged>(name);

        public void ChangeCode(string name) => ChangeCode<ProductCodeChanged>(name);

        public void ChangeWarehousingStrategy(WarehousingStrategy strategy)
        {
            var old = WarehousingStrategy;
            if (old == strategy) return;
            WarehousingStrategy = strategy;
            Apply(new ProductWarehousingStrategyChanged(old, strategy));
        }

        public void ChangeCategoryId(Guid? categoryId) => CategoryId = categoryId;

        public void SetAlternativeUnits(IEnumerable<ProductAlternativeUnit> productAlternativeUnits)
        {
            _alternativeUnits.UpdateFrom(productAlternativeUnits,
            update: (source, target) =>
            {
                source.ConversionRatio = target.ConversionRatio;
                source.TargetUnitId = target.TargetUnitId;
            });
        }

        public void ChangeImage(string fileName, byte[] data)
        {
            ImageFileName = fileName;
            ImageData = data;
        }

        public void ChangeDescription(string value) => Description = value;

        public void ChangeCatalogNumber(string value) => CatalogNumber = value;

        public void ChangePkwiuCode(string value) => PkwiuCode = value;

        public void ChangeCnCode(string value) => CnCode = value;

        public void ChangeIsFreezable(bool value) => IsFreezable = value;

        public void ChangeIsPerishable(bool value) => IsPerishable = value;

        public void ChangeDefaultUseByDate(int value) => DefaultUseByDate = value;

        public void ChangeDefaultFreezeDate(int value) => DefaultFreezeDate = value;

        public void ChangeDefaultQuantity(decimal defaultQuantity)
        {
            CheckState.IsGreaterThanOrEqualTo(defaultQuantity, 0, nameof(DefaultQuantity));
            DefaultQuantity = defaultQuantity;
        }

        public void ChangeNumberGeneratorTemplateId(Guid? numberGeneratorTemplateId) => NumberGeneratorTemplateId = numberGeneratorTemplateId;

        public void ChangeQuantityInputModes(Unit baseUnit, QuantityInputModes value)
        {
            if (baseUnit.Id != BaseUnitId) throw new ArgumentException("BaseUnit doesn't match Product's base unit");

            if (!baseUnit.IsWeighted)
            {
                value &= ~QuantityInputModes.FromScale;
            }
            QuantityInputModes = value;
        }

        public void ChangeWeightInputModes(Unit baseUnit, WeightInputModes value)
        {
            if (baseUnit.Id != BaseUnitId) throw new ArgumentException("BaseUnit doesn't match Product's base unit");

            if (baseUnit.IsWeighted)
            {
                value &= ~WeightInputModes.FromQuantity;
            }
            WeightInputModes = value;
        }

        public void ChangeSizeInputModes(Unit baseUnit, SizeInputModes value)
        {
            if (baseUnit.Id != BaseUnitId) throw new ArgumentException("BaseUnit doesn't match Product's base unit");

            SizeInputModes = value;
        }

        public void ChangeHasWeight(bool value) => HasWeight = value;

        public void ChangeHasSize(bool value) => HasSize = value;

        public void ChangeWeight(decimal netWeight, decimal tare)
        {
            CheckState.IsGreaterThanOrEqualTo(netWeight, 0, nameof(NetWeight));
            CheckState.IsGreaterThanOrEqualTo(tare, 0, nameof(Tare));

            NetWeight = netWeight;
            Tare = tare;
        }

        public void ChangeSize(decimal width, decimal height, decimal depth)
        {
            CheckState.IsGreaterThanOrEqualTo(width, 0, nameof(Width));
            CheckState.IsGreaterThanOrEqualTo(height, 0, nameof(Height));
            CheckState.IsGreaterThanOrEqualTo(depth, 0, nameof(Depth));

            Width = width;
            Height = height;
            Depth = depth;
        }

        public void ChangeQuantityStep(double step) => QuantityStep = step;

        public void SetStorageTemperatures(double? min, double? max)
        {
            if (min != null && max != null && min > max)
            {
                throw new ValidationFailedException();
            }

            MinStorageTemperature = min;
            MaxStorageTemperature = max;
        }

        public void SetWeightUnit(Unit unit)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            CheckState.Verify(unit.IsWeighted, "Unit must be weighted", nameof(WeightUnitId));
            WeightUnitId = unit.Id;
        }

        public void SetSizeUnit(Unit unit)
        {
            if (unit == null) throw new ArgumentNullException(nameof(unit));
            CheckState.Verify(!unit.IsWeighted, "Unit must not be weighted", nameof(SizeUnitId));
            SizeUnitId = unit.Id;
        }

        public void SetWeightLimits(double? min, double? max)
        {
            if (min != null && max != null && min > max)
            {
                throw new ValidationFailedException();
            }

            MinWeight = min;
            MaxWeight = max;
        }

        public void ChangePurchaseTaxRate(Guid purchaseTaxRateId) => PurchaseTaxRateId = purchaseTaxRateId;

        public void ChangeSalesTaxRate(Guid salesTaxRateId) => SalesTaxRateId = salesTaxRateId;
    }
}
