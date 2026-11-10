using System.Globalization;

namespace SystemsDataStructures.Core.Parsing;

/// <summary>
/// Stack-only representation of a parsed telemetry wire frame:
/// Format: "SENSOR_TAG|TIMESTAMP_UNIX_MS|VALUE|QUALITY"
/// Cannot be boxed or escaped to the managed heap.
/// </summary>
public readonly ref struct TelemetryFrameSlice
{
    public ReadOnlySpan<char> SensorTag { get; init; }
    public long TimestampUnixMs { get; init; }
    public double Value { get; init; }
    public ReadOnlySpan<char> Quality { get; init; }
}

/// <summary>
/// Zero-allocation stack-only line enumerator over a ReadOnlySpan&lt;char&gt; buffer.
/// </summary>
public ref struct SpanLineEnumerator
{
    private ReadOnlySpan<char> _remaining;
    private ReadOnlySpan<char> _current;
    private bool _isActive;

    public SpanLineEnumerator(ReadOnlySpan<char> buffer)
    {
        _remaining = buffer;
        _current = default;
        _isActive = !buffer.IsEmpty;
    }

    public readonly ReadOnlySpan<char> Current => _current;

    public readonly SpanLineEnumerator GetEnumerator() => this;

    public bool MoveNext()
    {
        while (_isActive)
        {
            if (_remaining.IsEmpty)
            {
                _isActive = false;
                return false;
            }

            var newlineIdx = _remaining.IndexOf('\n');
            ReadOnlySpan<char> line;

            if (newlineIdx >= 0)
            {
                line = _remaining[..newlineIdx];
                _remaining = _remaining[(newlineIdx + 1)..];
            }
            else
            {
                line = _remaining;
                _remaining = default;
                _isActive = false;
            }

            if (!line.IsEmpty && line[^1] == '\r')
            {
                line = line[..^1];
            }

            line = line.Trim();
            if (!line.IsEmpty)
            {
                _current = line;
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// High-throughput zero-allocation wire-protocol parser using ReadOnlySpan&lt;char&gt; slicing.
/// Avoids string.Split and intermediate string/array allocations on the GC heap.
/// </summary>
public static class SpanTelemetryParser
{
    public static bool TryParseFrame(ReadOnlySpan<char> rawLine, out TelemetryFrameSlice frame)
    {
        frame = default;
        var span = rawLine.Trim();
        if (span.IsEmpty)
            return false;

        // 1. Extract SENSOR_TAG
        var sep1 = span.IndexOf('|');
        if (sep1 <= 0)
            return false;

        var sensorTag = span[..sep1].Trim();
        if (sensorTag.IsEmpty)
            return false;

        var rest = span[(sep1 + 1)..];

        // 2. Extract TIMESTAMP_UNIX_MS
        var sep2 = rest.IndexOf('|');
        if (sep2 <= 0)
            return false;

        var tsSlice = rest[..sep2].Trim();
        if (!long.TryParse(tsSlice, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestampMs) || timestampMs < 0)
            return false;

        rest = rest[(sep2 + 1)..];

        // 3. Extract VALUE
        var sep3 = rest.IndexOf('|');
        if (sep3 <= 0)
            return false;

        var valSlice = rest[..sep3].Trim();
        if (!double.TryParse(valSlice, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || double.IsNaN(value) || double.IsInfinity(value))
            return false;

        // 4. Extract QUALITY
        var qualitySlice = rest[(sep3 + 1)..].Trim();
        if (qualitySlice.IsEmpty || qualitySlice.IndexOf('|') >= 0)
            return false;

        frame = new TelemetryFrameSlice
        {
            SensorTag = sensorTag,
            TimestampUnixMs = timestampMs,
            Value = value,
            Quality = qualitySlice
        };

        return true;
    }

    /// <summary>
    /// Scans a multi-line wire payload in-place without heap allocations, aggregating statistics
    /// for a target sensor tag where Quality equals "GOOD" (case-insensitive).
    /// </summary>
    public static bool TryAggregateGoodReadings(
        ReadOnlySpan<char> multiLinePayload,
        ReadOnlySpan<char> targetTag,
        out int count,
        out double average,
        out double min,
        out double max)
    {
        count = 0;
        average = 0.0;
        min = double.MaxValue;
        max = double.MinValue;
        double sum = 0.0;

        foreach (var line in new SpanLineEnumerator(multiLinePayload))
        {
            if (!TryParseFrame(line, out var frame))
                continue;

            if (!frame.SensorTag.Equals(targetTag, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!frame.Quality.Equals("GOOD", StringComparison.OrdinalIgnoreCase))
                continue;

            count++;
            sum += frame.Value;
            if (frame.Value < min) min = frame.Value;
            if (frame.Value > max) max = frame.Value;
        }

        if (count == 0)
        {
            min = 0.0;
            max = 0.0;
            return false;
        }

        average = sum / count;
        return true;
    }
}
