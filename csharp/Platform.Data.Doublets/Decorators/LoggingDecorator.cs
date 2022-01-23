using System.Collections.Generic;
using System.IO;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators
{
    public class LoggingDecorator<TLink> : LinksDecoratorBase<TLink>
    {
        private readonly Stream _logStream;
        private readonly StreamWriter _logStreamWriter;
        public LoggingDecorator(ILinks<TLink> links, Stream logStream) : base(links)
        {
            _logStream = logStream;
            _logStreamWriter = new StreamWriter(_logStream);
            _logStreamWriter.AutoFlush = true;
        }

        public override TLink Create(IList<TLink> substitution, WriteHandler<TLink> handler)
        {
            WriteHandlerState<TLink> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Create(substitution, (before, after) =>
            {
                if (handlerState.Handler != null)
                {
                    handlerState.Apply(handlerState.Handler(before, after));
                }
                _logStreamWriter.WriteLine($"Create. Before: {new Link<TLink>(before)}. After: {new Link<TLink>(after)}");
                return _constants.Continue;
            });
        }

        public override TLink Update(IList<TLink> restriction, IList<TLink> substitution, WriteHandler<TLink> handler)
        {
            WriteHandlerState<TLink> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Update(restriction, substitution, (before, after) =>
            {
                if (handlerState.Handler != null)
                {
                    handlerState.Apply(handlerState.Handler(before, after));
                }
                _logStreamWriter.WriteLine($"Update. Before: {new Link<TLink>(before)}. After: {new Link<TLink>(after)}");
                return _constants.Continue;
            });
        }

        public override TLink Delete(IList<TLink> restriction, WriteHandler<TLink> handler)
        {
            WriteHandlerState<TLink> handlerState = new(_constants.Continue, _constants.Break, handler);
            return base.Delete(restriction, (before, after) =>
            {
                if (handlerState.Handler != null)
                {
                    handlerState.Apply(handlerState.Handler(before, after));
                }
                _logStreamWriter.WriteLine($"Delete. Before: {new Link<TLink>(before)}. After: {new Link<TLink>(after)}");
                return _constants.Continue;
            });
        }
    }
}


