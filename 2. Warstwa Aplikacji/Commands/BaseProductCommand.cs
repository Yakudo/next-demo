using System;
using System.Collections.Generic;
using Yakudo.Next.Abstractions.Messaging;
using Yakudo.Next.Application.Common.ExpandoModels.Services.Editor;
using Yakudo.Next.Framework.Abstractions.Files;
using Yakudo.Next.Domain.Products.Products;

namespace Yakudo.Next.Application.Products.Products.Commands
{
    public class BaseProductCommand : Command
    {
        public string Name { get; set; }

        public string Code { get; set; }

        public string CatalogNumber { get; set; }
        public string PkwiuCode { get; set; }
        public string CnCode { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? SizeUnitId { get; set; }
        public Guid? WeightUnitId { get; set; }

        public IFormFile File { get; set; }

        public string Description { get; set; }

        public WarehousingStrategy WarehousingStrategy { get; set; }

        public List<ProductAlternativeUnit> AlternativeUnits { get; set; } = new List<ProductAlternativeUnit>();

        public Guid? AttributesDataModelId { get; set; }
        public List<ExpandoEditorFieldValue> Attributes { get; set; } = new List<ExpandoEditorFieldValue>();

        public bool IsFreezable { get; set; } = false;
        public bool IsPerishable { get; set; } = true;

        public int DefaultUseByDate { get; set; }
        public int DefaultFreezeDate { get; set; }

        public decimal DefaultQuantity { get; set; }

        public Guid? NumberGeneratorTemplateId { get; set; }

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

        public Guid? PurchaseTaxRateId { get; set; }
        public Guid? SalesTaxRateId { get; set; }
    }
}