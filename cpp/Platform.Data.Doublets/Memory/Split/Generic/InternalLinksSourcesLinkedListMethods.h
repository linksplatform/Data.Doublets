

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    public unsafe class InternalLinksSourcesLinkedListMethods<TLink> : public RelativeCircularDoublyLinkedListMethods<TLink>
    {
        private: static readonly UncheckedConverter<TLink, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLink, std::int64_t>.Default;
        private: readonly std::uint8_t* _linksDataParts;
        private: readonly std::uint8_t* _linksIndexParts;
        protected: TLink Break = 0;
        protected: TLink Continue = 0;

        public: InternalLinksSourcesLinkedListMethods(LinksConstants<TLink> constants, std::uint8_t* linksDataParts, std::uint8_t* linksIndexParts)
        {
            _linksDataParts = linksDataParts;
            _linksIndexParts = linksIndexParts;
            Break = constants.Break;
            Continue = constants.Continue;
        }

        protected: virtual ref RawLinkDataPart<TLink> GetLinkDataPartReference(TLink link) { return ref AsRef<RawLinkDataPart<TLink>>(_linksDataParts + (RawLinkDataPart<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: virtual ref RawLinkIndexPart<TLink> GetLinkIndexPartReference(TLink link) { return ref AsRef<RawLinkIndexPart<TLink>>(_linksIndexParts + (RawLinkIndexPart<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: TLink GetFirst(TLink head) override { return this->GetLinkIndexPartReference(head)->RootAsSource; }

        protected: TLink GetLast(TLink head) override
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

        protected: TLink GetPrevious(TLink element) override { return this->GetLinkIndexPartReference(element)->LeftAsSource; }

        protected: TLink GetNext(TLink element) override { return this->GetLinkIndexPartReference(element)->RightAsSource; }

        protected: TLink GetSize(TLink head) override { return this->GetLinkIndexPartReference(head)->SizeAsSource; }

        protected: void SetFirst(TLink head, TLink element) override { this->GetLinkIndexPartReference(head)->RootAsSource = element; }

        protected: void SetLast(TLink head, TLink element) override
        {
        }

        protected: void SetPrevious(TLink element, TLink previous) override { this->GetLinkIndexPartReference(element)->LeftAsSource = previous; }

        protected: void SetNext(TLink element, TLink next) override { this->GetLinkIndexPartReference(element)->RightAsSource = next; }

        protected: void SetSize(TLink head, TLink size) override { this->GetLinkIndexPartReference(head)->SizeAsSource = size; }

        public: TLink CountUsages(TLink head) { return this->GetSize(head); }

        protected: virtual IList<TLink> GetLinkValues(TLink linkIndex)
        {
            auto* link = GetLinkDataPartReference(linkIndex);
            return Link<TLink>(linkIndex, link.Source, link.Target);
        }

        public: TLink EachUsage(TLink source, Func<IList<TLink>, TLink> handler)
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
