

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksSourcesLinkedListMethods<TLinkAddress> : public RelativeCircularDoublyLinkedListMethods<TLinkAddress>
    {
        private: static readonly UncheckedConverter<TLinkAddress, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLinkAddress, std::int64_t>.Default;
        private: readonly std::uint8_t* _linksDataParts;
        private: readonly std::uint8_t* _linksIndexParts;
        protected: TLinkAddress Break = 0;
        protected: TLinkAddress Continue = 0;

        public: InternalLinksSourcesLinkedListMethods(LinksConstants<TLinkAddress> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return ref AsRef<RawLinkDataPart<TLinkAddress>>(_linksDataParts + (RawLinkDataPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual ref RawLinkIndexPart<TLinkAddress> GetLinkIndexPartReference(TLinkAddress link) { return ref AsRef<RawLinkIndexPart<TLinkAddress>>(_linksIndexParts + (RawLinkIndexPart<TLinkAddress>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: TLinkAddress GetFirst(TLinkAddress head) override { return this->GetLinkIndexPartReference(head)->RootAsSource; }

        protected: TLinkAddress GetLast(TLinkAddress head) override
        {
            auto first = this->GetLinkIndexPartReference(head)->RootAsSource;
            if (first == 0)
            {
                return first;
            }
            else
            {
                return this->GetPrevious(first);
            }
        }

        protected: TLinkAddress GetPrevious(TLinkAddress element) override { return this->GetLinkIndexPartReference(element)->LeftAsSource; }

        protected: TLinkAddress GetNext(TLinkAddress element) override { return this->GetLinkIndexPartReference(element)->RightAsSource; }

        protected: TLinkAddress GetSize(TLinkAddress head) override { return this->GetLinkIndexPartReference(head)->SizeAsSource; }

        protected: void SetFirst(TLinkAddress head, TLinkAddress element) override { this->GetLinkIndexPartReference(head)->RootAsSource = element; }

        protected: void SetLast(TLinkAddress head, TLinkAddress element) override
        {
        }

        protected: void SetPrevious(TLinkAddress element, TLinkAddress previous) override { this->GetLinkIndexPartReference(element)->LeftAsSource = previous; }

        protected: void SetNext(TLinkAddress element, TLinkAddress next) override { this->GetLinkIndexPartReference(element)->RightAsSource = next; }

        protected: void SetSize(TLinkAddress head, TLinkAddress size) override { this->GetLinkIndexPartReference(head)->SizeAsSource = size; }

        public: TLinkAddress CountUsages(TLinkAddress head) { return this->GetSize(head); }

        protected: virtual IList<TLinkAddress> GetLinkValues(TLinkAddress linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link<TLinkAddress>(linkIndex, link.Source, link.Target);
        }

        public: TLinkAddress EachUsage(TLinkAddress source, Func<IList<TLinkAddress>, TLinkAddress> handler)
        {
            auto continue = Continue;
            auto break = Break;
            auto current = this->GetFirst(source);
            auto first = current;
            while (current != 0)
            {
                if (this->handler(this->GetLinkValues(current)) == (break))
                {
                    return break;
                }
                current = this->GetNext(current);
                if (current == first)
                {
                    return continue;
                }
            }
            return continue;
        }
    };
}
