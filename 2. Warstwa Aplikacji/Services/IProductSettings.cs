using System;
using System.Collections.Generic;
using System.ComponentModel;
using Yakudo.Next.Domain.Products.Products;
using Yakudo.MagicSettings;
using Yakudo.Next.Domain.Consts;

// Magiczne ustawienia - nie mają implementacji - implementacja jest generowana automatycznie przy starcie aplikacji
// poprzez obiekt proxy.
namespace Yakudo.Next.Application.Products.Products.Services
{
    public interface IProductSettings : ISettings
    {
        Guid? DefaultBaseUnitId { get; set; }
        Guid? DefaultSizeUnitId { get; set; }
        Guid? DefaultWeightUnitId { get; set; }

        [DefaultValue(DefaultHasWeightBehaviour.True)]
        DefaultHasWeightBehaviour DefaultHasWeightBehaviour { get; set; }

        [DefaultValue(DefaultHasSizeBehaviour.False)]
        DefaultHasSizeBehaviour DefaultHasSizeBehaviour { get; set; }

        Guid? SalesTaxRateId { get; set; }
        Guid? PurchaseTaxRateId { get; set; }

        [DefaultValue(BuiltInNumberGeneratorTemplatesInvariants.ProductCode)]
        string CodeGeneratorTemplate { get; set; }

        [DefaultValue(BuiltInNumberGeneratorTemplatesInvariants.BatchNumber)]
        string DefaultBatchNumberGeneratorTemplate { get; set; }

        Guid? DefaultCurrencyId { get; set; }
    }

    public static class IProductSettingsExtensions
    {
        public static List<QuantityInputModes> GetDefaultQuantityInputModes(this IProductSettings settings)
        {
            return new List<QuantityInputModes>
            {
                QuantityInputModes.FromScale,
                QuantityInputModes.FromUserInput
            };
        }

        public static List<WeightInputModes> GetDefaultWeightInputModes(this IProductSettings settings)
        {
            return new List<WeightInputModes>
            {
                WeightInputModes.FromScale,
                WeightInputModes.FromUserInput
            };
        }

        public static List<SizeInputModes> GetDefaultSizeInputModes(this IProductSettings settings)
        {
            return new List<SizeInputModes>();
        }

        public static bool ResolveHasWeight(this IProductSettings settings, bool isWeighted)
        {
            if (settings.DefaultHasWeightBehaviour == DefaultHasWeightBehaviour.True)
            {
                return true;
            }
            else if (settings.DefaultHasWeightBehaviour == DefaultHasWeightBehaviour.False)
            {
                return false;
            }
            else if (settings.DefaultHasWeightBehaviour == DefaultHasWeightBehaviour.TrueIfWeighted)
            {
                return isWeighted;
            }

            throw new NotImplementedException(settings.DefaultHasWeightBehaviour.ToString());
        }

        public static bool ResolveHasSize(this IProductSettings settings)
        {
            if (settings.DefaultHasSizeBehaviour == DefaultHasSizeBehaviour.True)
            {
                return true;
            }
            else if (settings.DefaultHasSizeBehaviour == DefaultHasSizeBehaviour.False)
            {
                return false;
            }

            throw new NotImplementedException(settings.DefaultHasWeightBehaviour.ToString());
        }
    }

    public enum DefaultHasWeightBehaviour
    {
        False,
        True,
        TrueIfWeighted,
    }

    public enum DefaultHasSizeBehaviour
    {
        False,
        True,
    }
}
