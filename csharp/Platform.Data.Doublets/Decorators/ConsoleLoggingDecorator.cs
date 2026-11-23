using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators;

public class ConsoleLoggingDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    public ConsoleLoggingDecorator(ILinks<TLinkAddress> links) : base(links: links)
    {
    }

    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Create(substitution: substitution, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            System.Console.WriteLine(value: $"Create. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }

    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Update(restriction: restriction, substitution: substitution, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            System.Console.WriteLine(value: $"Update. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }

    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Delete(restriction: restriction, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            System.Console.WriteLine(value: $"Delete. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }

    public override TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        var result = base.Count(restriction: restriction);
        var restrictionStr = restriction == null ? "null" : $"[{string.Join(", ", restriction)}]";
        System.Console.WriteLine(value: $"Count. Restriction: {restrictionStr}. Result: {result}");
        return result;
    }
}