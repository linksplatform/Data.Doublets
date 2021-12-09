namespace Platform::Data::Doublets::Memory
{
    template <typename ...> class ILinksTreeMethods;
    template <typename TLink> class ILinksTreeMethods<TLink>
    {
    public:
        virtual TLink CountUsages(TLink root) = 0;

        virtual TLink Search(TLink source, TLink target) = 0;

        virtual TLink EachUsage(TLink root, Func<IList<TLink>, TLink> handler) = 0;

        virtual void Detach(TLink* root, TLink linkIndex) = 0;

        virtual void Attach(TLink* root, TLink linkIndex) = 0;
    };
}
