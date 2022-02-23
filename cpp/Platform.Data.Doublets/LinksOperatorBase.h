namespace Platform::Data::Doublets
{
    template <typename ...> class LinksOperatorBase;
    template <typename TLink> class LinksOperatorBase<TLink>
    {
        protected: ILinks<TLink> *_links;

        public: ILinks<TLink> Links()
        {
            return _links;
        }

        protected: LinksOperatorBase(ILinks<TLink> &storage) { _links = storage; }
    };
}
