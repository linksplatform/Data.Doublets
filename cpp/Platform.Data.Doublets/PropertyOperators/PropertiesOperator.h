namespace Platform::Data::Doublets::PropertyOperators
{
    template <typename ...> class PropertiesOperator;
    template <typename TLinkAddress> class PropertiesOperator<TLinkAddress> : public LinksOperatorBase<TLinkAddress>, IProperties<TLinkAddress, TLinkAddress, TLinkAddress>
    {
        public: PropertiesOperator(ILinks<TLinkAddress> &storage) : base(storage) { }

        public: TLinkAddress GetValue(TLinkAddress object, TLinkAddress property)
        {
            auto storage = _links;
            auto objectProperty = storage.SearchOrDefault(object, property);
            if (objectProperty == 0)
            {
                return 0;
            }
            constexpr auto constants = storage.Constants;
            auto any = constants.Any;
            auto query = Link<TLinkAddress>(any, objectProperty, any);
            auto valueLink = storage.SingleOrDefault(query);
            if (valueLink == nullptr)
            {
                return 0;
            }
            return storage.GetTarget(valueLink[constants.IndexPart]);
        }

        public: void SetValue(TLinkAddress object, TLinkAddress property, TLinkAddress value)
        {
            auto storage = _links;
            auto objectProperty = storage.GetOrCreate(object, property);
            storage.DeleteMany(storage.AllIndices(storage.Constants.Any, objectProperty));
            storage.GetOrCreate(objectProperty, value);
        }
    };
}
