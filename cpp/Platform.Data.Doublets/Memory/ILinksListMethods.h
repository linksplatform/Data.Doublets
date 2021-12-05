namespace Platform::Data::Doublets::Memory
{
    template <typename ...> class ILinksListMethods;
    template <typename TLink> class ILinksListMethods<TLink>
    {
    public:
        virtual void Detach(TLink freeLink) = 0;

        virtual void AttachAsFirst(TLink link) = 0;
    };
}
