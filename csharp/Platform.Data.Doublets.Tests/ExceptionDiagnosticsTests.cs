using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class ExceptionDiagnosticsTests
{
    [Fact]
    public void ErrorConversionPreservesDefaultBehaviorAndSupportsOptInDiagnosticsAndFiltering()
    {
        using var memory = new HeapResizableDirectMemory();
        using var store = new UnitedMemoryLinks<ulong>(memory);
        var decorator = new NoExceptionsDecorator<ulong>(new ThrowingLinks(store));
        Assert.Null(decorator.ExceptionHandler);
        Assert.Equal(store.Constants.Error, decorator.Count(null));
        var errors = new List<Exception>();
        decorator.ExceptionHandler = errors.Add;
        Assert.Equal(store.Constants.Error, decorator.Count(null));
        Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        decorator.ExceptionFilter = _ => false;
        Assert.Throws<InvalidOperationException>(() => decorator.Count(null));
        Assert.Single(errors);
    }

    [Fact]
    public void ExplicitTransactionDisposalReportsFailureToPersistCommitState()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var logPath = Path.Combine(directory, "transactions.log");
            using var memory = new HeapResizableDirectMemory();
            using var store = new UnitedMemoryLinks<ulong>(memory);
            var owner = new DisposalProbe(store);
            var layer = new UInt64LinksTransactionsLayer(owner, logPath);
            // Inject a real I/O failure after construction without changing filesystem
            // permissions (which behave differently for root and on Windows).
            var field = typeof(UInt64LinksTransactionsLayer).GetField("_log", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            Assert.IsType<FileStream>(field.GetValue(layer)).Dispose();
            using var exclusive = new FileStream(logPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.ThrowsAny<IOException>(() => layer.Dispose());
            Assert.True(owner.IsDisposed);
            GC.SuppressFinalize(layer);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void DamagedTransactionLogKeepsItsOriginalDiagnosticDuringConstructorCleanup()
    {
        var path = Path.GetTempFileName();
        try
        {
            var bytes = new byte[checked((int)UInt64LinksTransactionsLayer.Transition.Size * 2)];
            bytes[UInt64LinksTransactionsLayer.Transition.Size] = 1;
            File.WriteAllBytes(path, bytes);
            using var memory = new HeapResizableDirectMemory();
            using var store = new UnitedMemoryLinks<ulong>(memory);
            Assert.Throws<NotSupportedException>(() => new UInt64LinksTransactionsLayer(store, path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private class DisposalProbe : LinksDecoratorBase<ulong>, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public DisposalProbe(ILinks<ulong> storage) : base(storage) { }
        public void Dispose()
        {
            IsDisposed = true;
            ((IDisposable)_links).Dispose();
        }
    }

    private class ThrowingLinks : LinksDecoratorBase<ulong>
    {
        public ThrowingLinks(ILinks<ulong> storage) : base(storage) { }
        public override ulong Count(IList<ulong>? restriction) => throw new InvalidOperationException("reproduction");
    }
}
