namespace Platform::Data::Doublets::Ffi
{
    template<typename TLinkOptions, typename ...TBase>
    class Links : public LinksBase<Links<TLinkOptions, TBase...>, TLinkOptions, TBase...>
    {
        public:
        using base = LinksBase<Links<TLinkOptions, TBase...>, TLinkOptions, TBase...>;
        using base::base;
    }; 
}
