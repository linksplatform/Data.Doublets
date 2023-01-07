using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators;

public class LoggingDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly Stream _logStream;
    private readonly StreamWriter _logStreamWriter;

    public LoggingDecorator(ILinks<TLinkAddress> links, Stream logStream) : base(links: links)
    {
        _logStream = logStream;
        _logStreamWriter = new StreamWriter(stream: _logStream);
        _logStreamWriter.AutoFlush = true;
    }

    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Create(substitution: substitution, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            _logStreamWriter.WriteLine(value: $"Create. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }

    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Update(restriction: restriction, substitution: substitution, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            _logStreamWriter.WriteLine(value: $"Update. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }

    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: _constants.Continue, @break: _constants.Break, handler: handler);
        return base.Delete(restriction: restriction, handler: (before, after) =>
        {
            handlerState.Handle(before: before, after: after);
            _logStreamWriter.WriteLine(value: $"Delete. Before: {new Link<TLinkAddress>(values: before)}. After: {new Link<TLinkAddress>(values: after)}");
            return _constants.Continue;
        });
    }
}
