using Xunit;

namespace GildedTros.App
{
    public class GildedTrosTest
    {
        //  At the end of each day our system lowers both values for every item
        [Fact]
        public void GivenRegularItem_WhenDayPasses_ThenSellInAndQualityDecrease()
        {
            // given regular item
            var item = new Item { Name = "Regular Item", SellIn = 1, Quality = 1 };

            // when day passes
            WhenDayPasses(item);

            // then Quality and SellIn decrease
            Assert.Equal(0, item.Quality);
            Assert.Equal(0, item.SellIn);
        }

        // Once the sell by date has passed, Quality degrades twice as fast
        [Fact]
        public void GivenRegularItemWithSellByDatePassed_WhenDayPasses_ThenQualityDegradesTwiceAsFast()
        {
            // given regular item with sell by date passed
            var item = new Item { Name = "Regular Item (sell by date passed)", SellIn = -1, Quality = 40 };

            // when day passes
            WhenDayPasses(item);

            // then Quality decreases twice as fast
            Assert.Equal(38, item.Quality);
            Assert.Equal(-2, item.SellIn);
        }

        // The Quality of an item is never negative
        [Fact]
        public void GivenRegularItemWithQualityZero_WhenDayPasses_ThenQualityDoesNotDecreaseUnderZero()
        {
            // given regular item with quality zero
            var item = new Item { Name = "Regular Item (quality zero)", SellIn = 1, Quality = 0 };

            // when day passes
            WhenDayPasses(item);

            // then Quality does not decrease under zero
            Assert.Equal(0, item.Quality);
            Assert.Equal(0, item.SellIn);
        }

        
        // "Good Wine" actually increases in Quality the older it gets
        [Fact]
        public void GivenGoodWine_WhenDayPasses_ThenQualityIncreases()
        {
            // given special "Good Wine" item
            var item = new Item { Name = "Good Wine", SellIn = 1, Quality = 0 };

            // when day passes
            WhenDayPasses(item);

            // then Quality increases
            Assert.Equal(1, item.Quality);
            Assert.Equal(0, item.SellIn);
        }

        // The Quality of an item is never more than 50
        [Fact]
        public void GivenGoodWine_WhenDayPasses_ThenQualityIncreasesUpUntil50Max()
        {
            // given special "Good Wine" item (with max quality 50 already)
            var item = new Item { Name = "Good Wine", SellIn = 1, Quality = 50 };

            // when day passes
            WhenDayPasses(item);

            // then Quality is max 50
            Assert.Equal(50, item.Quality);
            Assert.Equal(0, item.SellIn);
        }

        // "B-DAWG Keychain", being a legendary item, never has to be sold or decreases in Quality
        [Fact]
        public void GivenLegendaryItem_WhenDayPasses_ThenNothingHappens()
        {
            // given special "B-DAWG Keychain" item
            var item = new Item { Name = "B-DAWG Keychain", SellIn = 1, Quality = 80 };

            // when day passes
            WhenDayPasses(item);

            // then nothing is changed
            Assert.Equal(80, item.Quality);
            Assert.Equal(1, item.SellIn);
        }

        [Theory]
        // quality + 1
        [InlineData("Backstage passes for Re:factor", 100, 40, 41)] // "Backstage passes" for very interesting conferences increases in Quality as its SellIn value approaches;
        [InlineData("Backstage passes for HAXX", 100, 40, 41)] // "Backstage passes" for very interesting conferences increases in Quality as its SellIn value approaches;
        [InlineData("Backstage passes for Re:factor", 11, 40, 41)] // "Backstage passes" for very interesting conferences increases in Quality as its SellIn value approaches;
        [InlineData("Backstage passes for HAXX", 11, 40, 41)] // "Backstage passes" for very interesting conferences increases in Quality as its SellIn value approaches;
        // quality + 2
        [InlineData("Backstage passes for Re:factor", 10, 40, 42)] // Quality increases by 2 when there are 10 days or less
        [InlineData("Backstage passes for HAXX", 10, 40, 42)] // Quality increases by 2 when there are 10 days or less
        [InlineData("Backstage passes for Re:factor", 9, 40, 42)] // Quality increases by 2 when there are 10 days or less
        [InlineData("Backstage passes for HAXX", 9, 40, 42)] // Quality increases by 2 when there are 10 days or less
        [InlineData("Backstage passes for Re:factor", 6, 40, 42)] // Quality increases by 2 when there are 10 days or less
        [InlineData("Backstage passes for HAXX", 6, 40, 42)] // Quality increases by 2 when there are 10 days or less
        // quality + 3
        [InlineData("Backstage passes for Re:factor", 5, 40, 43)] // Quality increases by 3 when there are 5 days or less
        [InlineData("Backstage passes for HAXX", 5, 40, 43)] // Quality increases by 3 when there are 5 days or less
        [InlineData("Backstage passes for Re:factor", 4, 40, 43)] // Quality increases by 3 when there are 5 days or less
        [InlineData("Backstage passes for HAXX", 4, 40, 43)] // Quality increases by 3 when there are 5 days or less
        [InlineData("Backstage passes for Re:factor", 1, 40, 43)] // Quality increases by 3 when there are 5 days or less
        [InlineData("Backstage passes for HAXX", 1, 40, 43)] // Quality increases by 3 when there are 5 days or less
        // quality = 0
        [InlineData("Backstage passes for Re:factor", 0, 40, 0)] // Quality drops to 0 after the conference
        [InlineData("Backstage passes for HAXX", 0, 40, 0)] // Quality drops to 0 after the conference
        [InlineData("Backstage passes for Re:factor", -1, 40, 0)] // Quality drops to 0 after the conference
        [InlineData("Backstage passes for HAXX", -1, 40, 0)] // Quality drops to 0 after the conference
        public void GivenBackStagePassesWithSellIn_WhenDayPasses_ThenQualityIsAsExpected(string backStagePassName, int sellIn, int quality, int expectedQualityAfterDay)
        {
            // given special "Backstage passes" item
            var item = new Item { Name = backStagePassName, SellIn = sellIn, Quality = quality };

            // when day passes
            WhenDayPasses(item);

            Assert.Equal(expectedQualityAfterDay, item.Quality);
            Assert.Equal(sellIn - 1, item.SellIn);
        }

        // Smelly items ("Duplicate Code", "Long Methods", "Ugly Variable Names") degrade in Quality twice as fast as normal items
        [Theory]
        [InlineData("Duplicate Code")]
        [InlineData("Long Methods")]
        [InlineData("Ugly Variable Names")]
        public void GivenSmellyItem_WhenDayPasses_ThenQualityDecreasesTwiceAsFast(string smellyItemName)
        {
            // given special "Smelly" item
            var item = new Item { Name = smellyItemName, SellIn = 1, Quality = 40 };

            // when day passes
            WhenDayPasses(item);

            // then quality drops by 2
            Assert.Equal(38, item.Quality);
            Assert.Equal(0, item.SellIn);
        }

        private static void WhenDayPasses(Item item) => new GildedTros([item]).UpdateQuality();
    }
}