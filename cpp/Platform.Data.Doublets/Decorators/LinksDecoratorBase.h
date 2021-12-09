namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksDecoratorBase;
    template <typename TLink> class LinksDecoratorBase<TLink> : public LinksOperatorBase<TLink>, ILinks<TLink>
    {
        protected: LinksConstants<TLink> _constants;

        public: LinksConstants<TLink> Constants()
        {
            return _constants;
        }

        protected: ILinks<TLink> *_facade;

        public: ILinks<TLink> Facade
        {
            get => _facade;
            set
            {
                _facade = value;
                if (_links is LinksDecoratorBase<TLink> decorator)
                {
                    decorator.Facade = value;
                }
            }
        }

        protected: LinksDecoratorBase(ILinks<TLink> &links) : base(links)
        {
            _constants = links.Constants;
            Facade = this;
        }

        public: virtual TLink Count(IList<TLink> &restrictions) { return _links.Count()(restrictions); }

        public: virtual TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> &restrictions) { return _links.Each(handler, restrictions); }

        public: virtual TLink Create(IList<TLink> &restrictions) { return _links.Create(restrictions); }

        public: virtual TLink Update(IList<TLink> &restrictions, IList<TLink> &substitution) { return _links.Update(restrictions, substitution); }

        public: virtual void Delete(IList<TLink> &restrictions) { return _links.Delete(restrictions); }
    };
}
