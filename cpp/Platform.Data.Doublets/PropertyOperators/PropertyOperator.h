namespace Platform::Data::Doublets::PropertyOperators
{
    template <typename ...> class PropertyOperator;
    template <typename TLinkAddress> class PropertyOperator<TLinkAddress> : public LinksOperatorBase<TLinkAddress>, IProperty<TLinkAddress, TLinkAddress>
    {
        private: TLinkAddress _propertyMarker = 0;
        private: TLinkAddress _propertyValueMarker = 0;

        public: PropertyOperator(ILinks<TLinkAddress> &storage, TLinkAddress propertyMarker, TLinkAddress propertyValueMarker) : base(storage)
        {
            _propertyMarker = propertyMarker;
            _propertyValueMarker = propertyValueMarker;
        }

        public: TLinkAddress Get(TLinkAddress link)
        {
            auto property = _links.SearchOrDefault(link, _propertyMarker);
            return this->GetValue(this->GetContainer(property));
        }

        private: TLinkAddress GetContainer(TLinkAddress property)
        {
            auto valueContainer = this->0(TLinkAddress);
            if (property == 0)
            {
                return valueContainer;
            }
            auto storage = _links;
            auto constants = storage.Constants;
            auto countinueConstant = constants.Continue;
            auto breakConstant = constants.Break;
            auto anyConstant = constants.Any;
            auto query = Link<TLinkAddress>(anyConstant, property, anyConstant);
            storage.Each(candidate =>
            {
                auto candidateTarget = storage.GetTarget(candidate);
                auto valueTarget = storage.GetTarget(candidateTarget);
                if (valueTarget == _propertyValueMarker)
                {
                    valueContainer = storage.GetIndex(candidate);
                    return breakConstant;
                }
                return countinueConstant;
            }, query);
            return valueContainer;
        }

        private: TLinkAddress GetValue(TLinkAddress container) { return container == 0 ? 0 : _links.GetTarget(container); }

        public: void Set(TLinkAddress link, TLinkAddress value)
        {
            auto storage = _links;
            auto property = storage.GetOrCreate(link, _propertyMarker);
            auto container = this->GetContainer(property);
            if (container == 0)
            {
                storage.GetOrCreate(property, value);
            }
            else
            {
                storage.Update(container, property, value);
            }
        }
    };
}
