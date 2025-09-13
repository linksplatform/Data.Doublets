using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Algorithms
{
    /// <summary>
    /// <para>
    /// Represents an operation type in the diff algorithm.
    /// </para>
    /// <para></para>
    /// </summary>
    public enum DiffOperationType
    {
        /// <summary>
        /// <para>
        /// Elements are equal in both sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        Equal,
        /// <summary>
        /// <para>
        /// Element was inserted in the target sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        Insert,
        /// <summary>
        /// <para>
        /// Element was deleted from the source sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        Delete,
        /// <summary>
        /// <para>
        /// Element was replaced in the target sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        Replace
    }

    /// <summary>
    /// <para>
    /// Represents a single operation in a sequence diff.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The type of link addresses.</para>
    /// <para></para>
    /// </typeparam>
    public struct DiffOperation<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// <para>
        /// The type of operation.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly DiffOperationType Operation;
        
        /// <summary>
        /// <para>
        /// The element from the source sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly TLinkAddress? SourceElement;
        
        /// <summary>
        /// <para>
        /// The element from the target sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly TLinkAddress? TargetElement;
        
        /// <summary>
        /// <para>
        /// The position in the source sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly int SourcePosition;
        
        /// <summary>
        /// <para>
        /// The position in the target sequence.
        /// </para>
        /// <para></para>
        /// </summary>
        public readonly int TargetPosition;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="DiffOperation{TLinkAddress}"/> struct.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="operation">
        /// <para>The operation type.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourceElement">
        /// <para>The source element.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetElement">
        /// <para>The target element.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourcePosition">
        /// <para>The source position.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPosition">
        /// <para>The target position.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DiffOperation(DiffOperationType operation, TLinkAddress? sourceElement, TLinkAddress? targetElement, int sourcePosition, int targetPosition)
        {
            Operation = operation;
            SourceElement = sourceElement;
            TargetElement = targetElement;
            SourcePosition = sourcePosition;
            TargetPosition = targetPosition;
        }
    }

    /// <summary>
    /// <para>
    /// Implements a diff algorithm to find differences between two sequences of links.
    /// Uses Myers' algorithm for optimal diff calculation.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The type of link addresses.</para>
    /// <para></para>
    /// </typeparam>
    public class SequenceDiffAlgorithm<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        private readonly IEqualityComparer<TLinkAddress> _comparer;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="SequenceDiffAlgorithm{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceDiffAlgorithm() : this(EqualityComparer<TLinkAddress>.Default)
        {
        }

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="SequenceDiffAlgorithm{TLinkAddress}"/> class.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="comparer">
        /// <para>The equality comparer to use for elements.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SequenceDiffAlgorithm(IEqualityComparer<TLinkAddress> comparer)
        {
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
        }

        /// <summary>
        /// <para>
        /// Computes the diff between two sequences using Myers' algorithm.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target sequence.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of diff operations.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<DiffOperation<TLinkAddress>> ComputeDiff(IList<TLinkAddress> source, IList<TLinkAddress> target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var N = source.Count;
            var M = target.Count;
            
            // Handle edge case where both sequences are empty
            if (N == 0 && M == 0)
                return new List<DiffOperation<TLinkAddress>>();
            
            var MAX = N + M;

            var V = new int[2 * MAX + 1];
            var trace = new List<int[]>();

            for (var d = 0; d <= MAX; d++)
            {
                for (var k = -d; k <= d; k += 2)
                {
                    int x;
                    if (k == -d || (k != d && V[k - 1 + MAX] < V[k + 1 + MAX]))
                    {
                        x = V[k + 1 + MAX];
                    }
                    else
                    {
                        x = V[k - 1 + MAX] + 1;
                    }

                    var y = x - k;

                    while (x < N && y < M && _comparer.Equals(source[x], target[y]))
                    {
                        x++;
                        y++;
                    }

                    V[k + MAX] = x;

                    if (x >= N && y >= M)
                    {
                        trace.Add((int[])V.Clone());
                        return BuildPath(source, target, trace, N, M);
                    }
                }

                trace.Add((int[])V.Clone());
            }

            throw new InvalidOperationException("Unable to compute diff");
        }

        /// <summary>
        /// <para>
        /// Builds the path of operations from the trace.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="trace">
        /// <para>The trace from Myers' algorithm.</para>
        /// <para></para>
        /// </param>
        /// <param name="N">
        /// <para>Length of source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="M">
        /// <para>Length of target sequence.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of diff operations.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IList<DiffOperation<TLinkAddress>> BuildPath(IList<TLinkAddress> source, IList<TLinkAddress> target, 
            List<int[]> trace, int N, int M)
        {
            var operations = new List<DiffOperation<TLinkAddress>>();
            var x = N;
            var y = M;

            for (var d = trace.Count - 1; d >= 0; d--)
            {
                var V = trace[d];
                var MAX = N + M;
                var k = x - y;

                int prevK;
                if (k == -d || (k != d && V[k - 1 + MAX] < V[k + 1 + MAX]))
                {
                    prevK = k + 1;
                }
                else
                {
                    prevK = k - 1;
                }

                var prevX = V[prevK + MAX];
                var prevY = prevX - prevK;

                while (x > prevX && y > prevY)
                {
                    operations.Insert(0, new DiffOperation<TLinkAddress>(
                        DiffOperationType.Equal, source[x - 1], target[y - 1], x - 1, y - 1));
                    x--;
                    y--;
                }

                if (d > 0)
                {
                    if (x > prevX)
                    {
                        operations.Insert(0, new DiffOperation<TLinkAddress>(
                            DiffOperationType.Delete, source[x - 1], default, x - 1, y));
                        x--;
                    }
                    else if (y > prevY)
                    {
                        operations.Insert(0, new DiffOperation<TLinkAddress>(
                            DiffOperationType.Insert, default, target[y - 1], x, y - 1));
                        y--;
                    }
                }
            }

            return operations;
        }

        /// <summary>
        /// <para>
        /// Computes a simplified diff that combines consecutive operations of the same type.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target sequence.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of simplified diff operations.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<DiffOperation<TLinkAddress>> ComputeSimplifiedDiff(IList<TLinkAddress> source, IList<TLinkAddress> target)
        {
            var operations = ComputeDiff(source, target);
            var simplified = new List<DiffOperation<TLinkAddress>>();

            for (var i = 0; i < operations.Count; i++)
            {
                var currentOp = operations[i];
                
                if (currentOp.Operation == DiffOperationType.Equal)
                {
                    simplified.Add(currentOp);
                }
                else
                {
                    var startIndex = i;
                    var endIndex = i;
                    
                    while (endIndex + 1 < operations.Count && 
                           operations[endIndex + 1].Operation == currentOp.Operation)
                    {
                        endIndex++;
                    }
                    
                    simplified.Add(new DiffOperation<TLinkAddress>(
                        currentOp.Operation,
                        currentOp.SourceElement,
                        currentOp.TargetElement,
                        operations[startIndex].SourcePosition,
                        operations[startIndex].TargetPosition));
                    
                    i = endIndex;
                }
            }

            return simplified;
        }

        /// <summary>
        /// <para>
        /// Computes the edit distance (Levenshtein distance) between two sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target sequence.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The edit distance between the sequences.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ComputeEditDistance(IList<TLinkAddress> source, IList<TLinkAddress> target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var n = source.Count;
            var m = target.Count;

            if (n == 0) return m;
            if (m == 0) return n;

            var dp = new int[n + 1, m + 1];

            for (var i = 0; i <= n; i++)
                dp[i, 0] = i;

            for (var j = 0; j <= m; j++)
                dp[0, j] = j;

            for (var i = 1; i <= n; i++)
            {
                for (var j = 1; j <= m; j++)
                {
                    var cost = _comparer.Equals(source[i - 1], target[j - 1]) ? 0 : 1;

                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[n, m];
        }

        /// <summary>
        /// <para>
        /// Computes the longest common subsequence between two sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source sequence.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target sequence.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The longest common subsequence.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IList<TLinkAddress> ComputeLongestCommonSubsequence(IList<TLinkAddress> source, IList<TLinkAddress> target)
        {
            var operations = ComputeDiff(source, target);
            return operations
                .Where(op => op.Operation == DiffOperationType.Equal && op.SourceElement != null)
                .Select(op => op.SourceElement!)
                .ToList();
        }
    }
}