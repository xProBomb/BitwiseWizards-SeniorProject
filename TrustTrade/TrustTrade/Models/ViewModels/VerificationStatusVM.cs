using System;
using System.Collections.Generic;

namespace TrustTrade.Models.ViewModels
{
    /// <summary>
    /// View model for verification status information
    /// </summary>
    public class VerificationStatusViewModel
    {
        /// <summary>
        /// Current verification status
        /// </summary>
        public bool IsVerified { get; set; }

        /// <summary>
        /// When the user was first verified, if ever
        /// </summary>
        public DateTime? FirstVerifiedDate { get; set; }

        /// <summary>
        /// When the user was most recently verified, if ever
        /// </summary>
        public DateTime? MostRecentVerifiedDate { get; set; }

        /// <summary>
        /// Total time the user has been verified
        /// </summary>
        public TimeSpan TotalVerifiedDuration { get; set; }

        /// <summary>
        /// Formatted string representation of the verified duration
        /// </summary>
        public string FormattedVerifiedDuration
        {
            get
            {
                if (TotalVerifiedDuration == TimeSpan.Zero)
                {
                    return "Not verified yet";
                }

                // Format the timespan in a human-readable way
                if (TotalVerifiedDuration.TotalDays >= 365)
                {
                    int years = (int)(TotalVerifiedDuration.TotalDays / 365);
                    int remainingDays = (int)(TotalVerifiedDuration.TotalDays % 365);
                    return $"{years} {(years == 1 ? "year" : "years")}{(remainingDays > 0 ? $", {remainingDays} days" : "")}";
                }
                else if (TotalVerifiedDuration.TotalDays >= 30)
                {
                    int months = (int)(TotalVerifiedDuration.TotalDays / 30);
                    int remainingDays = (int)(TotalVerifiedDuration.TotalDays % 30);
                    return $"{months} {(months == 1 ? "month" : "months")}{(remainingDays > 0 ? $", {remainingDays} days" : "")}";
                }
                else if (TotalVerifiedDuration.TotalDays >= 1)
                {
                    return $"{(int)TotalVerifiedDuration.TotalDays} {((int)TotalVerifiedDuration.TotalDays == 1 ? "day" : "days")}";
                }
                else
                {
                    return $"{(int)TotalVerifiedDuration.TotalHours} {((int)TotalVerifiedDuration.TotalHours == 1 ? "hour" : "hours")}";
                }
            }
        }

        /// <summary>
        /// Verification history records
        /// </summary>
        public List<VerificationHistoryItem> History { get; set; } = new List<VerificationHistoryItem>();
    }
}