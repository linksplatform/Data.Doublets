using System;
using System.Collections.Generic;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    public abstract class LinksDecoratorBase<TLink> : LinksOperatorBase<TLink>, ILinks<TLink>
    {
        public LinksConstants<TLink> Constants { get; }

        private ILinks<TLink> _facade;

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

        public virtual TLink Count(IList<TLink> restrictions) => Links.Count(restrictions);

        public virtual TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions) => Links.Each(handler, restrictions);

        public virtual TLink Create(IList<TLink> restrictions) => Links.Create(restrictions);

        public virtual TLink Update(IList<TLink> restrictions, IList<TLink> substitution) => Links.Update(restrictions, substitution);

        public virtual void Delete(IList<TLink> restrictions) => Links.Delete(restrictions);
    }
}
