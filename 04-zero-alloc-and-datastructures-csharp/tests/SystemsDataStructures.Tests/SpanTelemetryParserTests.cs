using System;
using SystemsDataStructures.Core.Parsing;
using Xunit;

namespace SystemsDataStructures.Tests;

public sealed class SpanTelemetryParserTests
{
    [Fact]
    public void TryParseFrame_ValidFrame_ExtractsSlicesAndParsesValues()
    {
        const string raw = "REACTOR-TEMP-01|1730678400123|372.45|GOOD";

        bool success = SpanTelemetryParser.TryParseFrame(raw.AsSpan(), out TelemetryFrameSlice frame);

        Assert.True(success);
        Assert.Equal("REACTOR-TEMP-01", frame.SensorTag.ToString());
        Assert.Equal(1730678400123L, frame.TimestampUnixMs);
        Assert.Equal(372.45, frame.Value, precision: 5);
        Assert.Equal("GOOD", frame.Quality.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("REACTOR-TEMP-01|1730678400123|372.45")] // Missing quality
    [InlineData("REACTOR-TEMP-01|1730678400123|372.45|GOOD|EXTRA")] // Extra column
    [InlineData("REACTOR-TEMP-01|NOT_A_TIMESTAMP|372.45|GOOD")] // Invalid timestamp
    [InlineData("|1730678400123|372.45|GOOD")] // Empty sensor tag
    [InlineData("REACTOR-TEMP-01|1730678400123|NaN|GOOD")] // NaN rejected
    [InlineData("REACTOR-TEMP-01|1730678400123|Infinity|GOOD")] // Infinity rejected
    public void TryParseFrame_MalformedFrames_ReturnsFalse(string malformedLine)
    {
        bool success = SpanTelemetryParser.TryParseFrame(malformedLine.AsSpan(), out _);

        Assert.False(success);
    }

    [Fact]
    public void TryAggregateGoodReadings_MultiLinePayload_ComputesMetricsCorrectly()
    {
        const string payload =
            "PRESS-01|1730678400000|10.5|GOOD\r\n" +
            "PRESS-01|1730678400100|99.9|FAULT\n" +
            "TEMP-99|1730678400150|500.0|GOOD\n" +
            "\n" +
            "PRESS-01|1730678400200|14.5|good\r\n" +
            "MALFORMED_LINE_SKIPPED\n" +
            "PRESS-01|1730678400300|11.0|Good";

        bool found = SpanTelemetryParser.TryAggregateGoodReadings(
            payload.AsSpan(),
            "PRESS-01".AsSpan(),
            out int validCount,
            out double average,
            out double min,
            out double max);

        Assert.True(found);
        Assert.Equal(3, validCount);
        Assert.Equal(12.0, average, precision: 5);
        Assert.Equal(10.5, min, precision: 5);
        Assert.Equal(14.5, max, precision: 5);
    }

    [Fact]
    public void TryAggregateGoodReadings_ExecutesWithZeroManagedHeapAllocations()
    {
        const string payload =
            "TEMP-01|1730678400000|350.12|GOOD\n" +
            "TEMP-01|1730678400050|351.88|GOOD\n" +
            "TEMP-01|1730678400100|349.50|WARN\n" +
            "TEMP-01|1730678400150|352.00|GOOD\r\n" +
            "TEMP-01|1730678400200|350.00|GOOD";

        ReadOnlySpan<char> spanPayload = payload.AsSpan();
        ReadOnlySpan<char> targetTag = "TEMP-01".AsSpan();

        // Warm-up JIT compilation before measuring thread allocations
        _ = SpanTelemetryParser.TryAggregateGoodReadings(spanPayload, targetTag, out _, out _, out _, out _);

        long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
        int accumulatedCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            if (SpanTelemetryParser.TryAggregateGoodReadings(
                spanPayload,
                targetTag,
                out int count,
                out double avg,
                out double min,
                out double max) && max >= min && avg > 0)
            {
                accumulatedCount += count;
            }
        }

        long bytesAfter = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(4000, accumulatedCount);
        Assert.Equal(0L, bytesAfter - bytesBefore);
    }
}
