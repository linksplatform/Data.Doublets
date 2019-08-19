using System;
using System.Collections.Generic;
using Platform.Data.Constants;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    public abstract class LinksDecoratorBase<TLink> : LinksOperatorBase<TLink>, ILinks<TLink>
    {
        public LinksCombinedConstants<TLink, TLink, int> Constants { get; }

        public ILinks<TLink> _facade;

        public ILinks<TLink> Facade
        {
            get => _facade;
            private set
            {
                _facade = value;
                if (Links is LinksDecoratorBase<TLink> decorator)
                {
                    decorator.Facade = value;
                }
            }
        }

        protected LinksDecoratorBase(ILinks<TLink> links) : base(links)
        {
            Constants = links.Constants;
            Facade = this;
        }

        public virtual TLink Count(IList<TLink> restriction) => Links.Count(restriction);

        public virtual TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions) => Links.Each(handler, restrictions);

        public virtual TLink Create() => Links.Create();

        public virtual TLink Update(IList<TLink> restrictions) => Links.Update(restrictions);

        public virtual void Delete(TLink link) => Links.Delete(link);
    }
}
