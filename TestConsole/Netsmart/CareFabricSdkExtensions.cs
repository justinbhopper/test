using System.Globalization;
using Netsmart.Bedrock.CareFabric.Cdm.Entities;
using Netsmart.Bedrock.CareFabric.Core.Enumerations;
using NodaTime;
using ApolloDateRange = RH.Apollo.Contracts.Models.DateRange;

namespace TestConsole.Netsmart;

public static class CareFabricSdkExtensions
{
    private static readonly CultureInfo s_usCulture = new("en-US");

    public static IPayload ToPayload<T>(this T payload)
        where T : PayloadBase
    {
        return payload.ToPayload(IMessage<T>.Name);
    }

    public static IPayload ToPayload<T>(this T payload, string messageName)
        where T : PayloadBase
    {
        return new Payload(messageName, payload);
    }

    public static IMessage ToEvent<T>(this T message, string eventName)
        where T : SdkBase
    {
        return new Message(eventName, CareFabricMessageType.Event, message);
    }

    public static bool FromCareFabric(this string? value, out LocalDate output)
    {
        if (!value.FromCareFabric(out DateTime datetime))
        {
            output = default;
            return false;
        }

        output = LocalDate.FromDateTime(datetime);
        return true;
    }

    public static bool FromCareFabric(this string? value, out DateTime? output)
    {
        if (!value.FromCareFabric(out DateTime datetime))
        {
            output = null;
            return false;
        }

        output = datetime;
        return true;
    }

    public static bool FromCareFabric(this string? value, out DateTime output)
    {
        if (string.IsNullOrEmpty(value))
        {
            output = default;
            return false;
        }

        output = DateTime.Parse(value);
        return true;
    }

    public static bool Overlaps(this ApolloDateRange range, string? effectiveStart, string? effectiveEnd)
    {
        if (effectiveStart is null && effectiveEnd is null)
            return true;

        var start = range.Start ?? LocalDate.MinIsoValue;
        var end = range.End ?? LocalDate.MaxIsoValue;

        if (!effectiveStart.FromCareFabric(out LocalDate effectiveStartDate))
            effectiveStartDate = LocalDate.MinIsoValue;

        if (!effectiveEnd.FromCareFabric(out LocalDate effectiveEndDate))
            effectiveEndDate = LocalDate.MaxIsoValue;

        // Start is between effective range
        if (start >= effectiveStartDate && start <= effectiveEndDate)
            return true;

        // End is between existing range
        if (end >= effectiveStartDate && end <= effectiveEndDate)
            return true;

        // Start and end overlap existing range
        if (start <= effectiveStartDate && end >= effectiveEndDate)
            return true;

        return false;
    }

    [return: NotNullIfNotNull("value")]
    public static string? ToCareFabric(this DateTime? value)
    {
        return value?.ToCareFabric();
    }

    public static string ToCareFabric(this DateTime value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ss.fff", s_usCulture) + value.ToString("zzz", s_usCulture);
    }

    [return: NotNullIfNotNull("value")]
    public static string? ToCareFabric(this DateTimeOffset? value)
    {
        return value?.ToCareFabric();
    }

    public static string ToCareFabric(this DateTimeOffset value)
    {
        return value.ToString("yyyy-MM-ddTHH:mm:ss.fff", s_usCulture) + value.ToString("zzz", s_usCulture);
    }

    [return: NotNullIfNotNull("dateRange")]
    public static DateRange? ToCareFabric(this ApolloDateRange? dateRange)
    {
        if (dateRange == null)
            return null;

        return new DateRange
        {
            FromDate = dateRange.Start?.ToCareFabric(),
            ToDate = dateRange.End?.ToCareFabric(),
        };
    }

    public static string ToCareFabric(this Instant value)
    {
        return value.ToDateTimeOffset().ToCareFabric();
    }

    public static string? ToCareFabric(this Instant? value)
    {
        return value?.ToCareFabric();
    }

    public static string ToCareFabric(this ZonedDateTime value)
    {
        return value.ToInstant().ToCareFabric();
    }

    public static string? ToCareFabric(this ZonedDateTime? value)
    {
        return value?.ToCareFabric();
    }

    public static string ToCareFabric(this LocalDate value)
    {
        return value.AtMidnight().InUtc().ToCareFabric();
    }

    public static string? ToCareFabric(this LocalDate? value)
    {
        return value?.ToCareFabric();
    }

    public static bool HasMoreResults(this PayloadBase response, int? pageSize, int previousCount, int addedCount)
    {
        // If we just got zero items, then assume done
        if (addedCount == 0)
            return false;

        // If TotalRecordCount is supported
        if (response.TotalRecordCount.HasValue)
        {
            if (response.TotalRecordCount.Value == 0)
                return false;

            return (previousCount + addedCount) < response.TotalRecordCount.Value;
        }

        // If TotalRecordCount is not supported, assume there are more pages if we just got a full page
        return addedCount == pageSize;
    }

    private class IMessage<T>
    {
        public static readonly string Name = GetMessageName();

        private static string GetMessageName()
        {
            const string suffix = "InputPayload";
            var request = typeof(T).Name;
            return request.EndsWith(suffix) ? request[..^suffix.Length] : request;
        }
    }

    private record Message : IMessage
    {
        public Message(string messageName, CareFabricMessageType messageType, object value)
        {
            MessageName = messageName;
            MessageType = messageType;
            Value = value;
        }

        public string MessageId { get; set; } = Guid.NewGuid().ToString();

        public string Method { get; set; } = "POST";

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        public string MessageName { get; }

        public string? RequesterId { get; set; }

        public CareFabricMessageType MessageType { get; }

        public object Value { get; }

        public IMessage OnBehalfOf(string? requesterId)
        {
            return this with { RequesterId = requesterId };
        }
    }

    private record Payload : Message, IPayload
    {
        private readonly PayloadBase _value;

        public Payload(string messageName, PayloadBase value)
            : base(messageName, CareFabricMessageType.Action, value)
        {
            _value = value;
        }

        PayloadBase IPayload.Value => _value;

        IPayload IPayload.OnBehalfOf(string? requesterId)
        {
            return this with { RequesterId = requesterId };
        }
    }
}
