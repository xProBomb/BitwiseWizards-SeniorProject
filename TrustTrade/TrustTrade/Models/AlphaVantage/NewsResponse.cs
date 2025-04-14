using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrustTrade.Models.AlphaVantage
{
    /// <summary>
    /// Represents the response from Alpha Vantage NEWS_SENTIMENT API
    /// </summary>
    public class NewsResponse
    {
        /// <summary>
        /// Contains metadata about the API response
        /// </summary>
        [JsonPropertyName("metadata")]
        public NewsMetadata Metadata { get; set; }

        /// <summary>
        /// Collection of news feed items
        /// </summary>
        [JsonPropertyName("feed")]
        public List<NewsFeedItem> Feed { get; set; }
    }

    /// <summary>
    /// Metadata about the Alpha Vantage news response
    /// </summary>
    public class NewsMetadata
    {
        /// <summary>
        /// Information about the API response
        /// </summary>
        [JsonPropertyName("information")]
        public string Information { get; set; }

        /// <summary>
        /// Count of feed items returned
        /// </summary>
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    /// <summary>
    /// Individual news feed item from Alpha Vantage
    /// </summary>
    public class NewsFeedItem
    {
        /// <summary>
        /// Article title
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; set; }

        /// <summary>
        /// URL to the original article
        /// </summary>
        [JsonPropertyName("url")]
        public string Url { get; set; }

        /// <summary>
        /// Publication timestamp in UTC
        /// </summary>
        [JsonPropertyName("time_published")]
        public string TimePublishedStr { get; set; }

        /// <summary>
        /// Article authors
        /// </summary>
        [JsonPropertyName("authors")]
        public List<string> Authors { get; set; }

        /// <summary>
        /// Article summary
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        /// <summary>
        /// News source
        /// </summary>
        [JsonPropertyName("source")]
        public string Source { get; set; }

        /// <summary>
        /// Category-specific sentiment data
        /// </summary>
        [JsonPropertyName("overall_sentiment_score")]
        public float OverallSentimentScore { get; set; }

        /// <summary>
        /// Sentiment label (Bearish, Somewhat Bearish, Neutral, Somewhat Bullish, Bullish)
        /// </summary>
        [JsonPropertyName("overall_sentiment_label")]
        public string OverallSentimentLabel { get; set; }

        /// <summary>
        /// Topics related to the article with relevance scores
        /// </summary>
        [JsonPropertyName("topics")]
        public List<NewsTopic> Topics { get; set; }

        /// <summary>
        /// Ticker-specific sentiment data
        /// </summary>
        [JsonPropertyName("ticker_sentiment")]
        public List<TickerSentiment> TickerSentiments { get; set; }

        /// <summary>
        /// Converts string time to DateTime
        /// </summary>
        public DateTime GetTimePublished()
        {
            // Alpha Vantage format is YYYYMMDDTHHMMSS, e.g. 20220104T000000
            if (DateTime.TryParseExact(TimePublishedStr, "yyyyMMddTHHmmss", 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime result))
            {
                return result;
            }
            return DateTime.UtcNow; // Fallback to current time if parsing fails
        }
    }

    /// <summary>
    /// Topic information for a news item
    /// </summary>
    public class NewsTopic
    {
        /// <summary>
        /// Topic name
        /// </summary>
        [JsonPropertyName("topic")]
        public string Topic { get; set; }

        /// <summary>
        /// Relevance score from 0-1
        /// </summary>
        [JsonPropertyName("relevance_score")]
        public float RelevanceScore { get; set; }
    }

    /// <summary>
    /// Stock or cryptocurrency ticker sentiment information
    /// </summary>
    public class TickerSentiment
    {
        /// <summary>
        /// Ticker symbol
        /// </summary>
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }

        /// <summary>
        /// Relevance score from 0-1
        /// </summary>
        [JsonPropertyName("relevance_score")]
        public float RelevanceScore { get; set; }

        /// <summary>
        /// Sentiment score for this ticker
        /// </summary>
        [JsonPropertyName("ticker_sentiment_score")]
        public float SentimentScore { get; set; }

        /// <summary>
        /// Sentiment label (Bearish, Somewhat Bearish, Neutral, Somewhat Bullish, Bullish)
        /// </summary>
        [JsonPropertyName("ticker_sentiment_label")]
        public string SentimentLabel { get; set; }
    }
}