namespace SystemsDataStructures.Core.Graphs;

public sealed record RouteResult(
    bool IsReachable,
    double TotalCost,
    IReadOnlyList<string> Path);

/// <summary>
/// Directed weighted graph router using Dijkstra's shortest-path algorithm
/// backed by the custom BinaryMinHeap&lt;TElement, TPriority&gt;.
/// Models minimum-resistance or minimum-pressure-drop routing across industrial process networks.
/// </summary>
public sealed class DijkstraRouter
{
    private readonly Dictionary<string, List<(string Target, double Weight)>> _adjacency = new(StringComparer.Ordinal);

    public void AddDirectedEdge(string from, string to, double weight)
    {
        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentException("Source node cannot be empty.", nameof(from));
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Target node cannot be empty.", nameof(to));
        if (weight < 0 || double.IsNaN(weight) || double.IsInfinity(weight))
            throw new ArgumentOutOfRangeException(nameof(weight), "Dijkstra edge weights must be non-negative finite numbers.");

        if (!_adjacency.TryGetValue(from, out var edges))
        {
            edges = new List<(string, double)>();
            _adjacency[from] = edges;
        }

        if (!_adjacency.ContainsKey(to))
        {
            _adjacency[to] = new List<(string, double)>();
        }

        edges.Add((to, weight));
    }

    public RouteResult FindShortestPath(string startNode, string targetNode)
    {
        if (!_adjacency.ContainsKey(startNode) || !_adjacency.ContainsKey(targetNode))
        {
            return new RouteResult(false, double.PositiveInfinity, Array.Empty<string>());
        }

        var distances = new Dictionary<string, double>(StringComparer.Ordinal);
        var previous = new Dictionary<string, string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var node in _adjacency.Keys)
        {
            distances[node] = double.PositiveInfinity;
        }
        distances[startNode] = 0.0;

        var pq = new BinaryMinHeap<string, double>();
        pq.Enqueue(startNode, 0.0);

        while (!pq.IsEmpty)
        {
            var (current, currentDist) = pq.DequeueMin();

            if (!visited.Add(current))
                continue;

            if (string.Equals(current, targetNode, StringComparison.Ordinal))
                break;

            foreach (var (neighbor, weight) in _adjacency[current])
            {
                if (visited.Contains(neighbor))
                    continue;

                var candidateDist = currentDist + weight;
                if (candidateDist < distances[neighbor])
                {
                    distances[neighbor] = candidateDist;
                    previous[neighbor] = current;
                    pq.Enqueue(neighbor, candidateDist);
                }
            }
        }

        if (double.IsPositiveInfinity(distances[targetNode]))
        {
            return new RouteResult(false, double.PositiveInfinity, Array.Empty<string>());
        }

        var path = new List<string>();
        var cursor = targetNode;
        while (true)
        {
            path.Add(cursor);
            if (string.Equals(cursor, startNode, StringComparison.Ordinal))
                break;
            cursor = previous[cursor];
        }
        path.Reverse();

        return new RouteResult(true, distances[targetNode], path);
    }
}
