using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class AutomergedLinksTests
    {
        [Fact]
        public static void ChangeSetBasicTest()
        {
            var changeSet = new ChangeSet<ulong>();
            Assert.Equal(0, changeSet.Count);
            Assert.False(changeSet.IsCompleted);

            changeSet.AddCreate(1UL, new ulong[] { 2UL, 3UL });
            Assert.Equal(1, changeSet.Count);
            Assert.True(changeSet.Contains(1UL));

            var linkState = changeSet.GetLinkState(1UL);
            Assert.NotNull(linkState);
            Assert.Equal(2UL, linkState![0]);
            Assert.Equal(3UL, linkState[1]);

            changeSet.Complete();
            Assert.True(changeSet.IsCompleted);
            Assert.Throws<InvalidOperationException>(() => changeSet.AddCreate(2UL, new ulong[] { 4UL, 5UL }));
        }

        [Fact]
        public static void ChangeSetUpdateSequenceTest()
        {
            var changeSet = new ChangeSet<ulong>();
            
            // Create a link
            changeSet.AddCreate(1UL, new ulong[] { 2UL, 3UL });
            
            // Update the link
            changeSet.AddUpdate(1UL, new ulong[] { 2UL, 3UL }, new ulong[] { 4UL, 5UL });
            
            // The final state should show the create with updated values
            var linkState = changeSet.GetLinkState(1UL);
            Assert.NotNull(linkState);
            Assert.Equal(4UL, linkState![0]);
            Assert.Equal(5UL, linkState[1]);
            
            // Should still only have one change (create with final values)
            Assert.Equal(1, changeSet.Count);
        }

        [Fact]
        public static void ChangeStreamTest()
        {
            var stream = new ChangeStream<ulong>();
            var receivedChangeSets = new System.Collections.Generic.List<IChangeSet<ulong>>();

            using var subscription = stream.Subscribe(changeSet => receivedChangeSets.Add(changeSet));

            var changeSet1 = new ChangeSet<ulong>();
            changeSet1.AddCreate(1UL, new ulong[] { 2UL, 3UL });
            
            var changeSet2 = new ChangeSet<ulong>();
            changeSet2.AddCreate(4UL, new ulong[] { 5UL, 6UL });

            stream.Publish(changeSet1);
            stream.Publish(changeSet2);

            Assert.Equal(2, receivedChangeSets.Count);
            Assert.Equal(changeSet1.Id, receivedChangeSets[0].Id);
            Assert.Equal(changeSet2.Id, receivedChangeSets[1].Id);
        }

        [Fact]
        public static void AutomergedLinksBasicTest()
        {
            using var tempFile = new TempLinksFile<ulong>();
            var baseLinks = new UnitedMemoryLinks<ulong>(tempFile.Memory);
            var snapshot = new LinksSnapshot<ulong>(baseLinks);
            var automergedLinks = new AutomergedLinks<ulong>(snapshot);

            // Test creation in change set
            var link1 = automergedLinks.Create(new ulong[] { 0UL, 0UL }, null);
            Assert.True(link1 > 0UL);

            // Test count includes change sets
            var count = automergedLinks.Count(null);
            Assert.True(count > 0UL);

            // Test merged state retrieval
            var mergedState = automergedLinks.GetMergedLinkState(link1);
            Assert.NotNull(mergedState);
        }

        [Fact]
        public static void SnapshotSwappingTest()
        {
            using var tempFile1 = new TempLinksFile<ulong>();
            using var tempFile2 = new TempLinksFile<ulong>();
            
            var baseLinks1 = new UnitedMemoryLinks<ulong>(tempFile1.Memory);
            var baseLinks2 = new UnitedMemoryLinks<ulong>(tempFile2.Memory);
            
            // Create some data in the first base
            baseLinks1.Create(new ulong[] { 0UL, 0UL }, null);
            
            var snapshot1 = new LinksSnapshot<ulong>(baseLinks1);
            var snapshot2 = new LinksSnapshot<ulong>(baseLinks2);
            
            var automergedLinks = new AutomergedLinks<ulong>(snapshot1);
            
            var initialCount = automergedLinks.Count(null);
            
            // Swap to empty snapshot
            automergedLinks.SwapSnapshot(snapshot2);
            
            var newCount = automergedLinks.Count(null);
            Assert.True(newCount < initialCount);
        }

        [Fact]
        public static void ConcurrentOperationsTest()
        {
            using var tempFile = new TempLinksFile<ulong>();
            var baseLinks = new UnitedMemoryLinks<ulong>(tempFile.Memory);
            var snapshot = new LinksSnapshot<ulong>(baseLinks);
            var automergedLinks = new AutomergedLinks<ulong>(snapshot);

            const int operationsPerTask = 10;
            const int taskCount = 4;

            var tasks = Enumerable.Range(0, taskCount).Select(taskId =>
                Task.Run(() =>
                {
                    for (int i = 0; i < operationsPerTask; i++)
                    {
                        var link = automergedLinks.Create(new ulong[] { 0UL, 0UL }, null);
                        Assert.True(link > 0UL);
                    }
                })
            ).ToArray();

            Task.WaitAll(tasks);

            var finalCount = automergedLinks.Count(null);
            Assert.True(finalCount >= taskCount * operationsPerTask);
        }

        private class TempLinksFile<TLinkAddress> : IDisposable where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            public string FilePath { get; }
            public IResizableDirectMemory Memory { get; }

            public TempLinksFile()
            {
                FilePath = Path.GetTempFileName();
                Memory = new FileMappedResizableDirectMemory(FilePath);
            }

            public void Dispose()
            {
                Memory?.Dispose();
                if (File.Exists(FilePath))
                {
                    File.Delete(FilePath);
                }
            }
        }
    }
}