namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template <typename TLinksOptions>
    class InternalLinksSourcesRecursionlessSizeBalancedTreeMethods
        : public InternalLinksRecursionlessSizeBalancedTreeMethodsBase<
              InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinksOptions>, TLinksOptions>
    {
      public:
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using WriteHandlerType = typename LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
        // Intentional CRTP customization of the base tree algorithm.
        // cppcheck-suppress duplInheritedMember
        static constexpr auto Constants = LinksOptionsType::Constants;
        using base = InternalLinksRecursionlessSizeBalancedTreeMethodsBase<
            InternalLinksSourcesRecursionlessSizeBalancedTreeMethods<TLinksOptions>, TLinksOptions>;

      public:
        InternalLinksSourcesRecursionlessSizeBalancedTreeMethods(std::byte* linksDataParts, std::byte* linksIndexParts,
                                                                 std::byte* header)
            : base(linksDataParts, linksIndexParts, header)
        {
        }

      public:
        LinkAddressType* GetLeftReference(LinkAddressType node)
        {
            return &(this->GetLinkIndexPartReference(node).LeftAsSource);
        }

      public:
        LinkAddressType* GetRightReference(LinkAddressType node)
        {
            return &(this->GetLinkIndexPartReference(node).RightAsSource);
        }

      public:
        LinkAddressType GetLeft(LinkAddressType node)
        {
            return this->GetLinkIndexPartReference(node).LeftAsSource;
        }

      public:
        LinkAddressType GetRight(LinkAddressType node)
        {
            return this->GetLinkIndexPartReference(node).RightAsSource;
        }

      public:
        void SetLeft(LinkAddressType node, LinkAddressType left)
        {
            this->GetLinkIndexPartReference(node).LeftAsSource = left;
        }

      public:
        void SetRight(LinkAddressType node, LinkAddressType right)
        {
            this->GetLinkIndexPartReference(node).RightAsSource = right;
        }

      public:
        LinkAddressType GetSize(LinkAddressType node)
        {
            return this->GetLinkIndexPartReference(node).SizeAsSource;
        }

      public:
        void SetSize(LinkAddressType node, LinkAddressType size)
        {
            this->GetLinkIndexPartReference(node).SizeAsSource = size;
        }

      public:
        // Intentional CRTP customization of the base tree algorithm.
        // cppcheck-suppress duplInheritedMember
        LinkAddressType GetTreeRoot(LinkAddressType link)
        {
            return this->GetLinkIndexPartReference(link).RootAsSource;
        }

      public:
        // Intentional CRTP customization of the base tree algorithm.
        // cppcheck-suppress duplInheritedMember
        LinkAddressType GetBasePartValue(LinkAddressType link)
        {
            return this->GetLinkDataPartReference(link).Source;
        }

      public:
        // Intentional CRTP customization of the base tree algorithm.
        // cppcheck-suppress duplInheritedMember
        LinkAddressType GetKeyPartValue(LinkAddressType link)
        {
            return this->GetLinkDataPartReference(link).Target;
        }

      public:
        void ClearNode(LinkAddressType node)
        {
            auto& link = this->GetLinkIndexPartReference(node);
            link.LeftAsSource = 0;
            link.RightAsSource = 0;
            link.SizeAsSource = 0;
        }

      public:
        // Intentional CRTP customization of the base tree algorithm.
        // cppcheck-suppress duplInheritedMember
        LinkAddressType Search(LinkAddressType source, LinkAddressType target)
        {
            return this->SearchCore(this->GetTreeRoot(source), target);
        }
    };
} // namespace Platform::Data::Doublets::Memory::Split::Generic
