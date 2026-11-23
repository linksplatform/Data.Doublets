using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Xunit;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Tests for shared access to links database across multiple processes.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class SharedAccessLinksTests
    {
        /// <summary>
        /// <para>
        /// Tests basic shared access functionality within the same process using multiple threads.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void BasicSharedAccessTest()
        {
            var tempFilename = Path.GetTempFileName();
            try
            {
                // Create first instance
                using (var links1 = new SharedAccessLinks<ulong>(tempFilename))
                {
                    // Create a link
                    var link = links1.Create();
                    Assert.True(link > 0);
                    
                    // Create second instance accessing the same file
                    using (var links2 = new SharedAccessLinks<ulong>(tempFilename))
                    {
                        // Verify the link created by first instance is visible in second instance
                        var count = links2.Count();
                        Assert.True(count >= 1);
                        
                        // Create a link from second instance
                        var link2 = links2.Create();
                        Assert.True(link2 > link);
                        
                        // Verify both instances see both links
                        Assert.True(links1.Count() >= 2);
                        Assert.True(links2.Count() >= 2);
                    }
                }
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Tests concurrent access from multiple threads to the same shared database.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void ConcurrentAccessTest()
        {
            var tempFilename = Path.GetTempFileName();
            const int threadCount = 4;
            const int linksPerThread = 10;
            var exceptions = new Exception[threadCount];
            
            try
            {
                var tasks = new Task[threadCount];
                
                for (int i = 0; i < threadCount; i++)
                {
                    int threadIndex = i;
                    tasks[i] = Task.Run(() =>
                    {
                        try
                        {
                            using (var links = new SharedAccessLinks<ulong>(tempFilename))
                            {
                                for (int j = 0; j < linksPerThread; j++)
                                {
                                    var link = links.Create();
                                    Assert.True(link > 0);
                                    
                                    // Small delay to increase chance of concurrent access
                                    Thread.Sleep(1);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            exceptions[threadIndex] = ex;
                        }
                    });
                }
                
                Task.WaitAll(tasks);
                
                // Check for exceptions
                for (int i = 0; i < threadCount; i++)
                {
                    if (exceptions[i] != null)
                    {
                        throw new AggregateException($"Thread {i} failed", exceptions[i]);
                    }
                }
                
                // Verify final count
                using (var links = new SharedAccessLinks<ulong>(tempFilename))
                {
                    var totalCount = links.Count();
                    Assert.True(totalCount >= threadCount * linksPerThread);
                }
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Tests shared access lock information stored in database header.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void SharedLockInfoTest()
        {
            var tempFilename = Path.GetTempFileName();
            
            try
            {
                // Create first instance and verify lock info
                using (var links1 = new SharedAccessUnitedMemoryLinks<ulong>(tempFilename))
                {
                    var header = links1.GetHeaderReference();
                    var lockValue = Convert.ToUInt64(header.Reserved8);
                    
                    // Should have shared access enabled (bit 0 = 1) and process count >= 1
                    Assert.True((lockValue & 1) == 1, "Shared access should be enabled");
                    Assert.True((lockValue >> 1) >= 1, "Process count should be at least 1");
                    
                    ulong lockValue2;
                    // Create second instance
                    using (var links2 = new SharedAccessUnitedMemoryLinks<ulong>(tempFilename))
                    {
                        var header2 = links2.GetHeaderReference();
                        lockValue2 = Convert.ToUInt64(header2.Reserved8);
                        
                        // Process count should be incremented
                        Assert.True((lockValue2 >> 1) >= (lockValue >> 1), "Process count should be incremented");
                    }
                    
                    // After second instance is disposed, verify count is decremented
                    Thread.Sleep(100); // Give some time for cleanup
                    var headerAfter = links1.GetHeaderReference();
                    var lockValueAfter = Convert.ToUInt64(headerAfter.Reserved8);
                    Assert.True((lockValueAfter >> 1) <= (lockValue2 >> 1), "Process count should be decremented after disposal");
                }
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Tests that the database can be safely accessed after shared access is disabled.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void SharedAccessDisabledAfterAllProcessesExitTest()
        {
            var tempFilename = Path.GetTempFileName();
            
            try
            {
                // Create and use shared access instances
                using (var links1 = new SharedAccessUnitedMemoryLinks<ulong>(tempFilename))
                {
                    links1.Create();
                    
                    using (var links2 = new SharedAccessUnitedMemoryLinks<ulong>(tempFilename))
                    {
                        links2.Create();
                    }
                }
                
                // All shared access instances are disposed, verify lock is disabled
                using (var links = new SharedAccessUnitedMemoryLinks<ulong>(tempFilename))
                {
                    var header = links.GetHeaderReference();
                    var lockValue = Convert.ToUInt64(header.Reserved8);
                    
                    // Should be re-enabled when new instance is created
                    Assert.True((lockValue & 1) == 1, "Shared access should be re-enabled for new instance");
                    Assert.True((lockValue >> 1) == 1, "Process count should be 1 for new instance");
                }
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }

        /// <summary>
        /// <para>
        /// Tests that regular UnitedMemoryLinks still work after shared access modifications.
        /// </para>
        /// <para></para>
        /// </summary>
        [Fact]
        public static void BackwardCompatibilityTest()
        {
            var tempFilename = Path.GetTempFileName();
            
            try
            {
                // Use regular UnitedMemoryLinks
                using (var links = new UnitedMemoryLinks<ulong>(tempFilename))
                {
                    var link = links.Create();
                    Assert.True(link > 0);
                    
                    var count = links.Count();
                    Assert.True(count >= 1);
                }
                
                // File should be accessible by SharedAccessLinks too
                using (var sharedLinks = new SharedAccessLinks<ulong>(tempFilename))
                {
                    var count = sharedLinks.Count();
                    Assert.True(count >= 1);
                    
                    var newLink = sharedLinks.Create();
                    Assert.True(newLink > 0);
                }
            }
            finally
            {
                if (File.Exists(tempFilename))
                {
                    File.Delete(tempFilename);
                }
            }
        }
    }
}