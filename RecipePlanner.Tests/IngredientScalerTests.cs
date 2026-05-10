using RecipePlanner.Helpers;

namespace RecipePlanner.Tests
{
    public class IngredientScalerTests
    {
        [Fact]
        public void Scale_FactorOne_ReturnsOriginal()
        {
            var result = IngredientScaler.Scale("200g Mehl", 1.0);
            Assert.Equal("200g Mehl", result);
        }

        [Fact]
        public void Scale_Double_DoublesLeadingNumber()
        {
            var result = IngredientScaler.Scale("200g Mehl", 2.0);
            Assert.Equal("400g Mehl", result);
        }

        [Fact]
        public void Scale_Half_HalvesLeadingNumber()
        {
            var result = IngredientScaler.Scale("4 Eier", 0.5);
            Assert.Equal("2 Eier", result);
        }

        [Fact]
        public void Scale_NoNumber_ReturnsOriginal()
        {
            var result = IngredientScaler.Scale("Salz nach Geschmack", 2.0);
            Assert.Equal("Salz nach Geschmack", result);
        }

        [Fact]
        public void Scale_DecimalInput_ScalesCorrectly()
        {
            var result = IngredientScaler.Scale("1,5 TL Zimt", 2.0);
            Assert.Equal("3 TL Zimt", result);
        }
    }
}