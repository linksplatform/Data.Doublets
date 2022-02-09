using System.Collections.Generic;
using System.IO;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators
{
    public class LoggingDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> 
    {
        private readonly Stream _logStream;
        private readonly StreamWriter _logStreamWriter;
        public LoggingDecorator(ILinks<TLinkAddress> links, Stream logStream) : base(links)
        {
            _logStream = logStream;
            _logStreamWriter = new StreamWriter(_logStream);
            _logStreamWriter.AutoFlush = true;
        }

        public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            WriteHandlerState<TLinkAddress> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Create(substitution, (before, after) =>
            {
                handlerState.Handle(before, after);
                _logStreamWriter.WriteLine($"Create. Before: {new Link<TLinkAddress>(before)}. After: {new Link<TLinkAddress>(after)}");
                return _constants.Continue;
            });
        }

        public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            WriteHandlerState<TLinkAddress> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Update(restriction, substitution, (before, after) =>
            {
                handlerState.Handle(before, after);
                _logStreamWriter.WriteLine($"Update. Before: {new Link<TLinkAddress>(before)}. After: {new Link<TLinkAddress>(after)}");
                return _constants.Continue;
            });
        }

        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            WriteHandlerState<TLinkAddress> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Delete(restriction, (before, after) =>
            {
                handlerState.Handle(before, after);
                _logStreamWriter.WriteLine($"Delete. Before: {new Link<TLinkAddress>(before)}. After: {new Link<TLinkAddress>(after)}");
                return _constants.Continue;
            });
        }
    }
}


