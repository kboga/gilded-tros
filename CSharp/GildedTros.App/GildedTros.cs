using System;
using System.Collections.Generic;

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
            foreach (var item in items)
            {
                var (qualityDiff, sellInDiff) = item.GetDiffs();

                item.Quality = item.GetNewBoundedQuality(qualityDiff);
                item.SellIn += sellInDiff;
            }
        }

        private static (int qualityDiff, int sellInDiff) GetDiffs(this Item item) =>
            item.Name switch
            {
                "Good Wine" => (item.SellIn <= 0 ? 2 : 1, -1),
                "B-DAWG Keychain" => (0, 0),
                "Duplicate Code" or "Long Methods" or "Ugly Variable Names" => (item.SellIn <= 0 ? -4 : -2, -1),
                "Backstage passes for Re:factor" or "Backstage passes for HAXX" =>
                    (
                        item.SellIn switch
                        {
                            <= 0 => -item.Quality,
                            <= 5 => 3,
                            <= 10 => 2,
                            _ => 1
                        }
                        ,
                        -1
                    ),
                _ => (item.SellIn <= 0 ? -2 : -1, -1)
            };

        private static int GetNewBoundedQuality(this Item item, int qualityDiff) => Math.Clamp(item.Quality + qualityDiff, 0, Math.Max(item.Quality, 50));
    }
}