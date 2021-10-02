namespace Platform::Data::Doublets::Memory
{
    template<typename TLink>
    struct ILinksTreeMethods
    {
        virtual TLink CountUsages(TLink root) = 0;

        virtual TLink Search(TLink source, TLink target) = 0;

        virtual TLink EachUsage(TLink root, const std::function<TLink(const std::vector<TLink>&)>& handler) = 0;

        virtual void Detach(TLink& root, TLink linkIndex) = 0;

        virtual void Attach(TLink& root, TLink linkIndex) = 0;
    };
}
