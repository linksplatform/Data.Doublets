namespace Platform::Data::Doublets::PropertyOperators
{
    template <typename ...> class PropertiesOperator;
    template <typename TLink> class PropertiesOperator<TLink> : public LinksOperatorBase<TLink>, IProperties<TLink, TLink, TLink>
    {
        public: PropertiesOperator(ILinks<TLink> &storage) : base(storage) { }

        public: TLink GetValue(TLink object, TLink property)
        {
            auto storage = _links;
            auto objectProperty = storage.SearchOrDefault(object, property);
            if (objectProperty == 0)
            {
                return 0;
            }
            auto constants = storage.Constants;
            auto any = constants.Any;
            auto query = Link<TLink>(any, objectProperty, any);
            auto valueLink = storage.SingleOrDefault(query);
            if (valueLink == nullptr)
            {
                return 0;
            }
            return storage.GetTarget(valueLink[constants.IndexPart]);
        }

        public: void SetValue(TLink object, TLink property, TLink value)
        {
            auto storage = _links;
            auto objectProperty = storage.GetOrCreate(object, property);
            storage.DeleteMany(storage.AllIndices(storage.Constants.Any, objectProperty));
            storage.GetOrCreate(objectProperty, value);
        }
    };
}
