using System;
using System.Collections.Generic;
using System.Linq;

namespace GildedTros.App
{
    public class GildedTros
    {
        IList<Item> Items;
        public GildedTros(IList<Item> Items)
        {
            this.Items = Items;
        }

        public void UpdateQuality() => Items.UpdateOnDayPassed();
    }

    public static class ItemHelpers
    {
        public static void UpdateOnDayPassed(this IEnumerable<Item> items)
        {
            foreach (var item in items.Select(x => x.ToItemWithItemUpdateStrategy()))
            {
                item.UpdateOnDayPassed();
            }
        }

        private static ItemWithItemUpdateStrategy ToItemWithItemUpdateStrategy(this Item item) => new UpdatableItem(item).ToItemWithItemUpdateStrategy();
        private static ItemWithItemUpdateStrategy ToItemWithItemUpdateStrategy(this IUpdatableItem item) => new(item, item.GetItemUpdateStrategy());

        private interface IReadableItem
        {
            string Name { get; }
            int SellIn { get; }
            int Quality { get; }
        }

        private interface IUpdatableItem : IReadableItem
        {
            void IncreaseQualityBy(int qualityIncrease);
            void DecreaseQualityBy(int qualityDecrease);
            void DecreaseSellIn();
        }

        private interface IItemUpdateStrategy
        {
            void UpdateOnDayPassed(IUpdatableItem item);
        }

        private enum ItemType { Regular, Appreciating, Legendary, BackstagePass }

        private static IItemUpdateStrategy GetItemUpdateStrategy(this IReadableItem item) =>
            item.GetItemType() switch
            {
                ItemType.Regular => new QualityDecreasingItemUpdateStrategy(1, true),
                ItemType.Appreciating => new QualityIncreasingItemUpdateStrategy(1, true),
                ItemType.Legendary => new LegendaryItemUpdateStrategy(),
                ItemType.BackstagePass => new BackStagePassItemUpdateStrategy(),
                _ => throw new InvalidOperationException($"{nameof(ItemType)} {item.GetItemType()} is not supported!"),
            };

        private static ItemType GetItemType(this IReadableItem item) =>
            item.Name switch
            {
                "Good Wine" => ItemType.Appreciating,
                "B-DAWG Keychain" => ItemType.Legendary,
                "Backstage passes for Re:factor" or "Backstage passes for HAXX" => ItemType.BackstagePass,
                _ => ItemType.Regular
            };

        private class ItemWithItemUpdateStrategy(IUpdatableItem Item, IItemUpdateStrategy ItemUpdateStrategy)
        {
            public void UpdateOnDayPassed() => ItemUpdateStrategy.UpdateOnDayPassed(Item);
        }

        private class QualityIncreasingItemUpdateStrategy(int QualityIncrease, bool ShouldDoubleIncreaseRateAfterSellByDatePassed) : IItemUpdateStrategy
        {
            public void UpdateOnDayPassed(IUpdatableItem item)
            {
                var actualQualityIncrease =
                    ShouldDoubleIncreaseRateAfterSellByDatePassed && item.SellIn <= 0
                    ? QualityIncrease * 2
                    : QualityIncrease;

                item.IncreaseQualityBy(actualQualityIncrease);
                item.DecreaseSellIn();
            }
        }

        private class QualityDecreasingItemUpdateStrategy(int QualityDecrease, bool ShouldDoubleDecreaseRateAfterSellByDatePassed) : IItemUpdateStrategy
        {
            public void UpdateOnDayPassed(IUpdatableItem item)
            {
                var actualQualityDecrease =
                    ShouldDoubleDecreaseRateAfterSellByDatePassed && item.SellIn <= 0
                    ? QualityDecrease * 2
                    : QualityDecrease;

                item.DecreaseQualityBy(actualQualityDecrease);
                item.DecreaseSellIn();
            }
        }

        private class LegendaryItemUpdateStrategy : IItemUpdateStrategy
        {
            public void UpdateOnDayPassed(IUpdatableItem item) { } // noop: never decreases in Quality & SellIn (never has to be sold or decreases in Quality)
        }

        private class BackStagePassItemUpdateStrategy : IItemUpdateStrategy
        {
            public void UpdateOnDayPassed(IUpdatableItem item)
            {
                if (item.SellIn <= 0) // Quality drops to 0 after the conference
                {
                    item.DecreaseQualityBy(item.Quality);
                }
                else if (item.SellIn <= 5) // Quality drops by 3 when there are 5 days or less
                {
                    item.IncreaseQualityBy(3);
                }
                else if (item.SellIn <= 10) // Quality increases by 2 when there are 10 days or less
                {
                    item.IncreaseQualityBy(2);
                }
                else // "Backstage passes" for very interesting conferences increases in Quality as its SellIn value approaches;
                {
                    item.IncreaseQualityBy(1);
                }

                item.DecreaseSellIn();
            }
        }

        private static void IncreaseQualityBy(this Item item, int qualityIncrease)
        {
            if (qualityIncrease < 0) throw new InvalidOperationException($"Illegal to increase quality by a negative number {qualityIncrease}");
            item.Quality = Math.Min(item.Quality + qualityIncrease, 50); // The Quality of an item is never more than 50
        }

        private static void DecreaseQualityBy(this Item item, int qualityDecrease)
        {
            if (qualityDecrease < 0) throw new InvalidOperationException($"Illegal to decrease quality by a negative number {qualityDecrease}");
            item.Quality = Math.Max(item.Quality - qualityDecrease, 0); // The Quality of an item is never negative
        }

        private static void DecreaseSellIn(this Item item) => item.SellIn--;
        
        private class UpdatableItem(Item Item) : IUpdatableItem
        {
            public string Name => Item.Name;
            public int SellIn => Item.SellIn;
            public int Quality => Item.Quality;
            public void IncreaseQualityBy(int qualityIncrease) => Item.IncreaseQualityBy(qualityIncrease);
            public void DecreaseQualityBy(int qualityDecrease) => Item.DecreaseQualityBy(qualityDecrease);
            public void DecreaseSellIn() => Item.DecreaseSellIn();
        }
    }
}