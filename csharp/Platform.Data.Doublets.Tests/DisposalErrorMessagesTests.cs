using System;
using System.IO;
using Xunit;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// Tests to verify that better error messages are shown when database access is disposed
    /// right after the access is created (GitHub issue #174)
    /// </summary>
    public static class DisposalErrorMessagesTests
    {
        [Fact]
        public static void DisposedLinksThrowObjectDisposedExceptionWithMeaningfulMessage()
        {
            var links = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory());
            
            // Create a link and then dispose
            var link = links.Create();
            links.Dispose();
            
            // Test Count method
            var ex = Assert.Throws<ObjectDisposedException>(() => links.Count());
            Assert.Contains("Cannot access a disposed Links instance", ex.Message);
            Assert.Contains("database connection has been closed", ex.Message);
            
            // Test Create method
            ex = Assert.Throws<ObjectDisposedException>(() => links.Create());
            Assert.Contains("Cannot access a disposed Links instance", ex.Message);
            
            // Test Delete method
            ex = Assert.Throws<ObjectDisposedException>(() => links.Delete(link));
            Assert.Contains("Cannot access a disposed Links instance", ex.Message);
            
            // Test Each method  
            ex = Assert.Throws<ObjectDisposedException>(() => links.Each(handler: l => links.Constants.Continue));
            Assert.Contains("Cannot access a disposed Links instance", ex.Message);
            
            // Test Update method
            ex = Assert.Throws<ObjectDisposedException>(() => links.Update(new[] { link }, new[] { link }));
            Assert.Contains("Cannot access a disposed Links instance", ex.Message);
        }
        
        [Fact]
        public static void DisposedLinksObjectNameIsCorrectInException()
        {
            var links = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory());
            links.Dispose();
            
            var ex = Assert.Throws<ObjectDisposedException>(() => links.Count());
            Assert.Contains("UnitedMemoryLinks", ex.ObjectName);
        }
        
        [Fact] 
        public static void NonDisposedLinksWorkNormally()
        {
            using var links = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory());
            
            // These should all work normally without throwing ObjectDisposedException
            var count = links.Count();
            var link = links.Create();
            links.Each(handler: l => links.Constants.Continue);
            // Test update - create another link to update the first one
            var link2 = links.Create();
            links.Update(new[] { link }, new[] { link, link2, link2 });
            links.Delete(link);
            links.Delete(link2);
        }
    }
}