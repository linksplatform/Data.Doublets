using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;
using Platform.Exceptions;

namespace Platform.Data.Doublets.Decorators;

/// <summary>Returns the error constant when an operation throws.</summary>
public class NoExceptionsDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>Initializes the decorator with its underlying links.</summary>
    public NoExceptionsDecorator(ILinks<TLinkAddress> storage) : base(links: storage) { }

    /// <inheritdoc/>
    public override TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        try
        {
            return base.Count(restriction: restriction);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    /// <inheritdoc/>
    public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Each(restriction: restriction, handler: handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    /// <inheritdoc/>
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Create(substitution: substitution, handler: handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    /// <inheritdoc/>
    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Update(restriction: restriction, substitution: substitution, handler: handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    /// <inheritdoc/>
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Delete(restriction: restriction, handler: handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }
}

