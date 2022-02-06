using System;
using System.Collections.Generic;
using Platform.Delegates;
using Platform.Exceptions;

namespace Platform.Data.Doublets.Decorators;

public class NoExceptionsDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
{
    public NoExceptionsDecorator(ILinks<TLinkAddress> storage) : base(storage) { }

    public override TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        try
        {
            return base.Count(restriction);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Each(restriction, handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Create(substitution, handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Update(restriction, substitution, handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }

    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        try
        {
            return base.Delete(restriction, handler);
        }
        catch (Exception exception)
        {
            exception.Ignore();
            return Constants.Error;
        }
    }
}
