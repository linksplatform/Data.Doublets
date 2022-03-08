namespace Platform::Data::Doublets::Memory
{
    template<typename TLink>
    struct ILinksListMethods
    {
        virtual void Detach(TLink freeLink) = 0;

        virtual void AttachAsFirst(TLink link) = 0;
    };
}
