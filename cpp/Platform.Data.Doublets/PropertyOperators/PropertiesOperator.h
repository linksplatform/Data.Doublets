namespace Platform::Data::Doublets::PropertyOperators
{
    template <typename ...> class PropertiesOperator;
    template <typename TLink> class PropertiesOperator<TLink> : public LinksOperatorBase<TLink>, IProperties<TLink, TLink, TLink>
    {
        public: PropertiesOperator(ILinks<TLink> &links) : base(links) { }

        public: TLink GetValue(TLink object, TLink property)
        {
            auto links = _links;
            auto objectProperty = links.SearchOrDefault(object, property);
            if (objectProperty == 0)
            {
                return 0;
            }
            auto constants = links.Constants;
            auto any = constants.Any;
            auto query = Link<TLink>(any, objectProperty, any);
            auto valueLink = links.SingleOrDefault(query);
            if (valueLink == nullptr)
            {
                return 0;
            }
            return links.GetTarget(valueLink[constants.IndexPart]);
        }

        public: void SetValue(TLink object, TLink property, TLink value)
        {
            auto links = _links;
            auto objectProperty = links.GetOrCreate(object, property);
            links.DeleteMany(links.AllIndices(links.Constants.Any, objectProperty));
            links.GetOrCreate(objectProperty, value);
        }
    };
}
