using System;
using UnityEngine;

public static class DateTimeUtility
{
    private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Convert Unix timestamp (seconds) to DateTime
    /// </summary>
    /// <param name="unixTimestamp">Unix timestamp in seconds</param>
    /// <returns>DateTime object</returns>
    public static DateTime FromUnixTimestamp(long unixTimestamp)
    {
        return UnixEpoch.AddSeconds(unixTimestamp);
    }

    /// <summary>
    /// Convert Unix timestamp (milliseconds) to DateTime
    /// </summary>
    /// <param name="unixTimestampMs">Unix timestamp in milliseconds</param>
    /// <returns>DateTime object</returns>
    public static DateTime FromUnixTimestampMs(long unixTimestampMs)
    {
        return UnixEpoch.AddMilliseconds(unixTimestampMs);
    }

    /// <summary>
    /// Convert DateTime to Unix timestamp (seconds)
    /// </summary>
    /// <param name="dateTime">DateTime object</param>
    /// <returns>Unix timestamp in seconds</returns>
    public static long ToUnixTimestamp(DateTime dateTime)
    {
        return (long)(dateTime - UnixEpoch).TotalSeconds;
    }

    /// <summary>
    /// Format date time for display
    /// </summary>
    /// <param name="unixTimestamp">Unix timestamp in seconds</param>
    /// <returns>Formatted date string</returns>
    public static string FormatDateTime(long unixTimestamp)
    {
        if (unixTimestamp == 0) return "N/A";
        
        try
        {
            DateTime dateTime = FromUnixTimestamp(unixTimestamp);
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DateTimeUtility] Error formatting timestamp {unixTimestamp}: {ex.Message}");
            return "Invalid Date";
        }
    }

    /// <summary>
    /// Format date time for display (short format)
    /// </summary>
    /// <param name="unixTimestamp">Unix timestamp in seconds</param>
    /// <returns>Formatted date string (short)</returns>
    public static string FormatDateTimeShort(long unixTimestamp)
    {
        if (unixTimestamp == 0) return "N/A";
        
        try
        {
            DateTime dateTime = FromUnixTimestamp(unixTimestamp);
            return dateTime.ToString("MM/dd/yyyy");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DateTimeUtility] Error formatting timestamp {unixTimestamp}: {ex.Message}");
            return "Invalid";
        }
    }

    /// <summary>
    /// Get relative time string (e.g., "2 hours ago")
    /// </summary>
    /// <param name="unixTimestamp">Unix timestamp in seconds</param>
    /// <returns>Relative time string</returns>
    public static string GetRelativeTime(long unixTimestamp)
    {
        if (unixTimestamp == 0) return "N/A";
        
        try
        {
            DateTime dateTime = FromUnixTimestamp(unixTimestamp);
            TimeSpan timeSpan = DateTime.UtcNow - dateTime;

            if (timeSpan.TotalDays >= 1)
            {
                int days = (int)timeSpan.TotalDays;
                return $"{days} day{(days > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalHours >= 1)
            {
                int hours = (int)timeSpan.TotalHours;
                return $"{hours} hour{(hours > 1 ? "s" : "")} ago";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                int minutes = (int)timeSpan.TotalMinutes;
                return $"{minutes} minute{(minutes > 1 ? "s" : "")} ago";
            }
            else
            {
                return "Just now";
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[DateTimeUtility] Error getting relative time for timestamp {unixTimestamp}: {ex.Message}");
            return "Unknown";
        }
    }
} 