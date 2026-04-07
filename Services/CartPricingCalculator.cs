using System;
using System.Collections.Generic;
using System.Linq;

namespace ShoeStore.Services
{
    public static class CartPricingCalculator
    {
        private const int PairDiscountThreshold = 2;
        private const decimal PairDiscountPercent = 10m;
        private const decimal ShippingThreshold = 3000m;
        private const decimal ShippingFeeAmount = 300m;

        public static CartPricingSummary CalculateBaseTotals(IEnumerable<CartPricingItem> items)
        {
            var materialized = items?.ToList() ?? new List<CartPricingItem>();
            var subtotal = 0m;

            foreach (var item in materialized)
            {
                if (item.Quantity <= 0)
                {
                    continue;
                }

                var discountedUnit = ClampCurrency(item.UnitPrice * (1 - item.DiscountPercent / 100m));
                subtotal += ClampCurrency(discountedUnit * item.Quantity);
            }

            subtotal = ClampCurrency(subtotal);
            var totalQuantity = materialized.Sum(i => Math.Max(0, i.Quantity));
            var pairDiscount = totalQuantity >= PairDiscountThreshold
                ? ClampCurrency(subtotal * PairDiscountPercent / 100m)
                : 0m;

            return new CartPricingSummary
            {
                TotalQuantity = totalQuantity,
                Subtotal = subtotal,
                PairDiscountAmount = pairDiscount
            };
        }

        public static decimal CalculateShippingFee(decimal netTotal)
        {
            if (netTotal <= 0)
            {
                return 0m;
            }

            return netTotal < ShippingThreshold ? ShippingFeeAmount : 0m;
        }

        public sealed class CartPricingSummary
        {
            public int TotalQuantity { get; init; }

            public decimal Subtotal { get; init; }

            public decimal PairDiscountAmount { get; init; }

            public decimal NetTotal => Math.Max(0m, ClampCurrency(Subtotal - PairDiscountAmount));
        }

        public sealed class CartPricingItem
        {
            public decimal UnitPrice { get; init; }

            public decimal DiscountPercent { get; init; }

            public int Quantity { get; init; }
        }

        private static decimal ClampCurrency(decimal value) =>
            decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }
}
