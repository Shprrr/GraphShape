﻿using System.Windows;
using System.Collections.Generic;
using System.Linq;
using GraphShape.Algorithms.EdgeRouting;
using GraphShape.Algorithms.Layout;
using JetBrains.Annotations;
using NUnit.Framework;
using QuikGraph;

namespace GraphShape.Tests
{
    /// <summary>
    /// Base class for tests related to layout algorithms.
    /// </summary>
    internal abstract class LayoutAlgorithmTestBase
    {
        #region Test helpers

        protected class LayoutResults
        {
            public bool PositionsSet { get; set; } = true;

            public int OverlapCount { get; set; }
            public double OverlappedArea { get; set; }

            public Point TopLeft { get; set; }
            public Point BottomRight { get; set; }
            public double Area { get; set; }
            public double Ratio { get; set; }

            public int CrossCount { get; set; }
            public double MinimumEdgeLength { get; set; }
            public double MaximumEdgeLength { get; set; }
            public double AverageEdgeLength { get; set; }

            public void CheckResult(int maxCrossCount)
            {
                Assert.IsTrue(PositionsSet);
                Assert.AreEqual(0, OverlapCount);
                Assert.AreEqual(0, OverlappedArea);
                Assert.LessOrEqual(CrossCount, maxCrossCount);
            }
        }

        [Pure]
        [NotNull]
        protected static IDictionary<TVertex, Size> GetVerticesSizes<TVertex>(
            [NotNull, ItemNotNull] IEnumerable<TVertex> vertices)
        {
            return vertices.ToDictionary(
                vertex => vertex,
                vertex => new Size(20, 20));
        }

        [Pure]
        [NotNull]
        protected static LayoutResults ExecuteLayoutAlgorithm<TVertex, TEdge>(
            [NotNull] ILayoutAlgorithm<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>> algorithm,
            [NotNull] IDictionary<TVertex, Size> vertexSizes)
            where TVertex : class
            where TEdge : IEdge<TVertex>
        {
            var results = new LayoutResults();

            Assert.DoesNotThrow(algorithm.Compute);

            IDictionary<TEdge, Point[]> edgeRoutes =
                algorithm is IEdgeRoutingAlgorithm<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>> routingAlgorithm
                    ? routingAlgorithm.EdgeRoutes
                    : new Dictionary<TEdge, Point[]>();

            // Compute metrics
            var positionsMetric = new PositionsMetricCalculator<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>>(
                algorithm.VisitedGraph,
                algorithm.VerticesPositions,
                vertexSizes,
                edgeRoutes);
            positionsMetric.Calculate();
            results.PositionsSet = positionsMetric.PositionsSet;


            var overlapMetric = new OverlapMetricCalculator<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>>(
                algorithm.VisitedGraph,
                algorithm.VerticesPositions,
                vertexSizes,
                edgeRoutes);
            overlapMetric.Calculate();

            results.OverlapCount = overlapMetric.OverlapCount;
            results.OverlappedArea = overlapMetric.OverlappedArea;


            var areaMetric = new LayoutAreaMetricCalculator<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>>(
                algorithm.VisitedGraph,
                algorithm.VerticesPositions,
                vertexSizes,
                edgeRoutes);
            areaMetric.Calculate();
            results.TopLeft = areaMetric.TopLeft;
            results.BottomRight = areaMetric.BottomRight;
            results.Area = areaMetric.Area;
            results.Ratio = areaMetric.Ratio;

            var edgeMetric = new EdgeCrossingCalculator<TVertex, TEdge, IBidirectionalGraph<TVertex, TEdge>>(
                algorithm.VisitedGraph,
                algorithm.VerticesPositions,
                vertexSizes,
                edgeRoutes);
            edgeMetric.Calculate();
            results.CrossCount = edgeMetric.CrossCount;
            results.MaximumEdgeLength = edgeMetric.MaximumEdgeLength;
            results.MinimumEdgeLength = edgeMetric.MinimumEdgeLength;
            results.AverageEdgeLength = edgeMetric.AverageEdgeLength;

            return results;
        }

        #endregion
    }
}