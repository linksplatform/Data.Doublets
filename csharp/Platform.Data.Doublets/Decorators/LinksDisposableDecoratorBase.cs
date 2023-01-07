using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Disposables;
using IDisposable = System.IDisposable;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable CA1063 // Implement IDisposable Correctly

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links disposable decorator base.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
/// <seealso cref="ILinks{TLinkAddress}" />
/// <seealso cref="System.IDisposable" />
public abstract class LinksDisposableDecoratorBase<TLinkAddress> : LinksDecoratorBase<TLinkAddress>, ILinks<TLinkAddress>, IDisposable where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         The disposable.
    ///     </para>
    ///     <para></para>
    /// </summary>
    protected readonly DisposableWithMultipleCallsAllowed Disposable;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksDisposableDecoratorBase" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected LinksDisposableDecoratorBase(ILinks<TLinkAddress> links) : base(links: links)
    {
        Disposable = new DisposableWithMultipleCallsAllowed(disposal: Dispose);
    }

    /// <summary>
    ///     <para>
    ///         Disposes this instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        Disposable.Dispose();
    }

    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    ~LinksDisposableDecoratorBase()
    {
        Disposable.Destruct();
    }

    /// <summary>
    ///     <para>
    ///         Disposes the manual.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="manual">
    ///     <para>The manual.</para>
    ///     <para></para>
    /// </param>
    /// <param name="wasDisposed">
    ///     <para>The was disposed.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual void Dispose(bool manual, bool wasDisposed)
    {
        if (!wasDisposed)
        {
            _links.DisposeIfPossible();
        }
    }

    /// <summary>
    ///     <para>
    ///         Represents the disposable with multiple calls allowed.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <seealso cref="Disposable" />
    protected class DisposableWithMultipleCallsAllowed : Disposable
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new <see cref="DisposableWithMultipleCallsAllowed" /> instance.
        ///     </para>
        ///     <para></para>
        /// </summary>
        /// <param name="disposal">
        ///     <para>A disposal.</para>
        ///     <para></para>
        /// </param>
        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        public DisposableWithMultipleCallsAllowed(Disposal disposal) : base(disposal: disposal) { }

        /// <summary>
        ///     <para>
        ///         Gets the allow multiple dispose calls value.
        ///     </para>
        ///     <para></para>
        /// </summary>
        protected override bool AllowMultipleDisposeCalls
        {
            [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
            get => true;
        }
    }
}
