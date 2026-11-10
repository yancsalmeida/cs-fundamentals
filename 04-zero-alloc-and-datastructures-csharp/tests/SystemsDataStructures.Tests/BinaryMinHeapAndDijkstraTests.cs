using System;
using SystemsDataStructures.Core.Graphs;
using Xunit;

namespace SystemsDataStructures.Tests;

public sealed class BinaryMinHeapAndDijkstraTests
{
    [Fact]
    public void BinaryMinHeap_EnqueueAndDequeue_MaintainsNonDecreasingPriorityOrder()
    {
        var heap = new BinaryMinHeap<string, int>(initialCapacity: 4);

        heap.Enqueue("task-low", 50);
        heap.Enqueue("task-critical", 1);
        heap.Enqueue("task-medium", 20);
        heap.Enqueue("task-high", 5);
        heap.Enqueue("task-urgent", 2);

        Assert.Equal(5, heap.Count);
        var (topElem, topPrio) = heap.Peek();
        Assert.Equal("task-critical", topElem);
        Assert.Equal(1, topPrio);

        int[] expectedPriorities = { 1, 2, 5, 20, 50 };
        foreach (int expected in expectedPriorities)
        {
            var (_, actualPriority) = heap.DequeueMin();
            Assert.Equal(expected, actualPriority);
        }

        Assert.True(heap.IsEmpty);
        Assert.Throws<InvalidOperationException>(() => heap.DequeueMin());
    }

    [Fact]
    public void DijkstraRouter_FindsMinimumCostPathAcrossWeightedPipelineTopology()
    {
        var router = new DijkstraRouter();

        // Direct path FEED -> PRODUCT costs 15.0
        // Multi-hop path FEED -> PUMP-A -> HEX-1 -> REACTOR -> PRODUCT costs 2.0 + 3.5 + 4.0 + 1.5 = 11.0
        router.AddDirectedEdge("FEED", "PRODUCT", 15.0);
        router.AddDirectedEdge("FEED", "PUMP-A", 2.0);
        router.AddDirectedEdge("PUMP-A", "HEX-1", 3.5);
        router.AddDirectedEdge("PUMP-A", "REACTOR", 9.0);
        router.AddDirectedEdge("HEX-1", "REACTOR", 4.0);
        router.AddDirectedEdge("REACTOR", "PRODUCT", 1.5);

        RouteResult result = router.FindShortestPath("FEED", "PRODUCT");

        Assert.True(result.IsReachable);
        Assert.Equal(11.0, result.TotalCost, precision: 5);
        Assert.Equal(new[] { "FEED", "PUMP-A", "HEX-1", "REACTOR", "PRODUCT" }, result.Path);
    }

    [Fact]
    public void DijkstraRouter_UnreachableTarget_ReturnsUnreachableResult()
    {
        var router = new DijkstraRouter();
        router.AddDirectedEdge("NODE-A", "NODE-B", 1.0);
        router.AddDirectedEdge("ISOLATED-X", "ISOLATED-Y", 2.0);

        RouteResult result = router.FindShortestPath("NODE-A", "ISOLATED-Y");

        Assert.False(result.IsReachable);
        Assert.Equal(double.PositiveInfinity, result.TotalCost);
        Assert.Empty(result.Path);
    }

    [Fact]
    public void DijkstraRouter_NegativeEdgeWeight_ThrowsArgumentOutOfRangeException()
    {
        var router = new DijkstraRouter();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            router.AddDirectedEdge("A", "B", -0.1));
    }
}
