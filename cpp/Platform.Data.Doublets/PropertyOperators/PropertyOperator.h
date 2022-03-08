namespace Platform::Data::Doublets::PropertyOperators
{
    template <typename ...> class PropertyOperator;
    template <typename TLink> class PropertyOperator<TLink> : public LinksOperatorBase<TLink>, IProperty<TLink, TLink>
    {
        private: TLink _propertyMarker = 0;
        private: TLink _propertyValueMarker = 0;

        public: PropertyOperator(ILinks<TLink> &storage, TLink propertyMarker, TLink propertyValueMarker) : base(storage)
        {
            _propertyMarker = propertyMarker;
            _propertyValueMarker = propertyValueMarker;
        }

        public: TLink Get(TLink link)
        {
            auto property = _links.SearchOrDefault(link, _propertyMarker);
            return this->GetValue(this->GetContainer(property));
        }

        private: TLink GetContainer(TLink property)
        {
            auto valueContainer = this->0(TLink);
            if (property == 0)
            {
                return valueContainer;
            }
            auto storage = _links;
            auto constants = storage.Constants;
            auto countinueConstant = constants.Continue;
            auto breakConstant = constants.Break;
            auto anyConstant = constants.Any;
            auto query = Link<TLink>(anyConstant, property, anyConstant);
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

        private: TLink GetValue(TLink container) { return container == 0 ? 0 : _links.GetTarget(container); }

        public: void Set(TLink link, TLink value)
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
