namespace TrustTrade.Helpers;

public class FormatCurrencyAbbreviate
{
    /// <summary>
/// Formats a currency value into an abbreviated form (e.g., $12K, $1.2M)
/// </summary>
/// <param name="value">The raw decimal value to format</param>
/// <param name="decimalPlaces">Number of decimal places to show (default 1)</param>
/// <returns>Formatted abbreviated string</returns>
public static string FormatCurrencyAbbreviated(decimal value, int decimalPlaces = 1)
{
    // Handle zero and negative values appropriately
    if (value == 0) return "$0";
    
    // For this implementation, we'll show negative values with a minus sign
    bool isNegative = value < 0;
    value = Math.Abs(value); // Work with absolute value for the formatting logic
    
    // Define the thresholds and corresponding suffixes
    (decimal threshold, string suffix)[] thresholds = {
        (1_000_000_000, "B"), // Billion
        (1_000_000, "M"),     // Million
        (1_000, "K"),         // Thousand
        (1, "")               // No suffix for values under 1000
    };
    
    // Find the appropriate threshold
    foreach (var (threshold, suffix) in thresholds)
    {
        if (value >= threshold)
        {
            // Calculate the scaled value (e.g., 12345 becomes 12.345 for K)
            decimal scaledValue = value / threshold;
            
            // Round to the specified number of decimal places
            string formattedNumber = scaledValue.ToString($"F{decimalPlaces}");
            
            // Remove trailing zeros and decimal point if not needed
            if (decimalPlaces > 0)
            {
                formattedNumber = formattedNumber.TrimEnd('0').TrimEnd('.');
            }
            
            // Apply the negative sign if needed
            string signPrefix = isNegative ? "-" : "";
            
            // Return the formatted currency with prefix, value, and suffix
            return $"{signPrefix}${formattedNumber}{suffix}";
        }
    }
    
    // This should never happen given our thresholds, but as a fallback:
    return value.ToString("C");
}
}