namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Collections::Methods::Lists;
    template<typename TLinksOptions>
    class InternalLinksSourcesLinkedListMethods : public RelativeCircularDoublyLinkedListMethods<InternalLinksSourcesLinkedListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>
    {
    public: using LinksOptionsType = TLinksOptions;
                using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using WriteHandlerType = LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
        public: static constexpr auto Constants = LinksOptionsType::Constants;
        private:
        std::byte* _linksDataParts;
        std::byte* _linksIndexParts;
        protected:
        LinkAddressType Break = Constants.Break;
        LinkAddressType Continue = Constants.Continue;

        public: InternalLinksSourcesLinkedListMethods(std::byte* linksDataParts, std::byte* linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
        }

        protected: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link)
        { 
            return &(*(_linksDataParts + (RawLinkDataPart<LinkAddressType>::SizeInBytes * link)));
        }

        protected: RawLinkIndexPart<LinkAddressType>&& GetLinkIndexPartReference(LinkAddressType link)
        { 
            return RawLinkIndexPart<LinkAddressType>{ _linksIndexParts + (RawLinkIndexPart<LinkAddressType>::SizeInBytes * link) };
        }

        protected: LinkAddressType GetFirst(LinkAddressType head)
        {
            return this->GetLinkIndexPartReference(head)->RootAsSource; 
        }

        protected: LinkAddressType GetLast(LinkAddressType head)
        {
            auto first = this->GetLinkIndexPartReference(head)->RootAsSource;
            if (0 == first)
            {
                return first;
            }
            else
            {
                return this->GetPrevious(first);
            }
        }

        protected: LinkAddressType GetPrevious(LinkAddressType element)
        {
             return this->GetLinkIndexPartReference(element)->LeftAsSource; 
        }

        protected: LinkAddressType GetNext(LinkAddressType element)
        {
             return this->GetLinkIndexPartReference(element)->RightAsSource; 
        }

        protected: LinkAddressType GetSize(LinkAddressType head)
        {
             return this->GetLinkIndexPartReference(head)->SizeAsSource; 
        }

        protected: void SetFirst(LinkAddressType head, LinkAddressType element)
        {
             this->GetLinkIndexPartReference(head)->RootAsSource = element; 
        }

        protected: void SetLast(LinkAddressType head, LinkAddressType element)
        {
        }

        protected: void SetPrevious(LinkAddressType element, LinkAddressType previous)
        {
             this->GetLinkIndexPartReference(element)->LeftAsSource = previous; 
        }

        protected: void SetNext(LinkAddressType element, LinkAddressType next)
        {
             this->GetLinkIndexPartReference(element)->RightAsSource = next; 
        }

        protected: void SetSize(LinkAddressType head, LinkAddressType size)
        {
             this->GetLinkIndexPartReference(head)->SizeAsSource = size; 
        }

        public: LinkAddressType CountUsages(LinkAddressType head) { return this->GetSize(head); }

    protected: Interfaces::CArray auto GetLinkValues(LinkAddressType linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link{ linkIndex, link.Source, link.Target };
        }

        public: LinkAddressType EachUsage(LinkAddressType source, auto&& handler)
        {
            auto current = this->GetFirst(source);
            auto first = current;
            while (current != 0)
            {
                if (this->handler(this->GetLinkValues(current)) == (Break))
                {
                    return Break;
                }
                current = this->GetNext(current);
                if (current == first)
                {
                    return Continue;
                }
            }
            return Continue;
        }
    };
}
