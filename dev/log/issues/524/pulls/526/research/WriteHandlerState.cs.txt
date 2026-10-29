using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;

namespace Platform.Data
{
    public struct WriteHandlerState<TLinkAddress>  where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        public TLinkAddress Result;
        public WriteHandler<TLinkAddress>? Handler;
        private TLinkAddress Break;

        public WriteHandlerState(TLinkAddress @continue, TLinkAddress @break, WriteHandler<TLinkAddress>? handler)
        {
            Break = @break;
            Result = @continue;
            Handler = handler;
        }

        public void Apply(TLinkAddress result)
        {
            var isAlreadyBreak = (Break == Result);
            var isCurrentlyBreak = (Break == result);
            if (isAlreadyBreak || !isCurrentlyBreak)
            {
                return;
            }
            Handler = null;
            Result = Break;

        }

        public TLinkAddress Handle(IList<TLinkAddress> before, IList<TLinkAddress> after)
        {
            if (Handler != null)
            {
                Apply(Handler(before, after));
            }
            return Result;
        }
    }
}


