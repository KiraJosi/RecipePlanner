using System.Text.RegularExpressions;

namespace RecipePlanner.Helpers
{
    public static class IngredientScaler
    {
        private static readonly Regex _numberPattern =
            new(@"^(\d+(?:[.,]\d+)?)\s*", RegexOptions.Compiled);

        public static string Scale(string ingredient, double factor)
        {
            if (factor == 1.0)
                return ingredient;

            var match = _numberPattern.Match(ingredient);
            if (!match.Success)
                return ingredient;

            var raw = match.Groups[1].Value.Replace(',', '.');
            if (!double.TryParse(raw,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double amount))
                return ingredient;

            var scaled = amount * factor;

            var formatted = scaled == Math.Floor(scaled)
                ? ((int)scaled).ToString()
                : scaled.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

            return formatted + ingredient[match.Length..];
        }
    }
}
