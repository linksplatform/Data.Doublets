namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using Platform::Collections::Methods::Lists::RelativeCircularDoublyLinkedListMethods;
    template<typename TLinkAddress>
    class InternalLinksSourcesLinkedListMethods : public RelativeCircularDoublyLinkedListMethods<TLinkAddress>
    {
        private:
        std::uint8_t* _linksDataParts;
        std::uint8_t* _linksIndexParts;
        protected:
        TLinkAddress Break;
        TLinkAddress Continue;

        public: InternalLinksSourcesLinkedListMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual RawLinkDataPart<TLinkAddress>& GetLinkDataPartReference(TLinkAddress link)
        { 
            return &(*(_linksDataParts + (RawLinkDataPart<TLinkAddress>.SizeInBytes * link)));
        }

        protected: virtual RawLinkIndexPart<TLinkAddress>&& GetLinkIndexPartReference(TLinkAddress link) 
        { 
            return RawLinkIndexPart<TLinkAddress>>{ _linksIndexParts + (RawLinkIndexPart<TLinkAddress>.SizeInBytes * link) };
        }

        protected: TLinkAddress GetFirst(TLinkAddress head) override 
        {
            return this->GetLinkIndexPartReference(head)->RootAsSource; 
        }

        protected: TLinkAddress GetLast(TLinkAddress head) override
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

        protected: TLinkAddress GetPrevious(TLinkAddress element) override
        {
             return this->GetLinkIndexPartReference(element)->LeftAsSource; 
        }

        protected: TLinkAddress GetNext(TLinkAddress element) override
        {
             return this->GetLinkIndexPartReference(element)->RightAsSource; 
        }

        protected: TLinkAddress GetSize(TLinkAddress head) override
        {
             return this->GetLinkIndexPartReference(head)->SizeAsSource; 
        }

        protected: void SetFirst(TLinkAddress head, TLinkAddress element) override
        {
             this->GetLinkIndexPartReference(head)->RootAsSource = element; 
        }

        protected: void SetLast(TLinkAddress head, TLinkAddress element) override
        {
        }

        protected: void SetPrevious(TLinkAddress element, TLinkAddress previous) override
        {
             this->GetLinkIndexPartReference(element)->LeftAsSource = previous; 
        }

        protected: void SetNext(TLinkAddress element, TLinkAddress next) override
        {
             this->GetLinkIndexPartReference(element)->RightAsSource = next; 
        }

        protected: void SetSize(TLinkAddress head, TLinkAddress size) override
        {
             this->GetLinkIndexPartReference(head)->SizeAsSource = size; 
        }

        public: TLinkAddress CountUsages(TLinkAddress head) { return this->GetSize(head); }

    protected: Interfaces::CArray auto GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link{ linkIndex, link.Source, link.Target };
        }

        public: TLinkAddress EachUsage(TLinkAddress source, auto&& handler)
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
