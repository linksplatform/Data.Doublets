namespace Platform::Data::Doublets::Stacks
{
    template <typename ...> class Stack;
    template <typename TLinkAddress> class Stack<TLinkAddress> : public LinksOperatorBase<TLinkAddress>, IStack<TLinkAddress>
    {
        private: TLinkAddress _stack = 0;

        public: bool IsEmpty()
        {
            return this->Peek() == _stack;
        }

        public: Stack(ILinks<TLinkAddress> &storage, TLinkAddress stack) : base(storage) { return _stack = stack; }

        private: TLinkAddress GetStackMarker() { return _links.GetSource(_stack); }

        private: TLinkAddress GetTop() { return _links.GetTarget(_stack); }

        public: TLinkAddress Peek() { return _links.GetTarget(this->GetTop()); }

        public: TLinkAddress Pop()
        {
            auto element = this->Peek();
            if (!element == _stack)
            {
                auto top = this->GetTop();
                auto previousTop = _links.GetSource(top);
                _links.Update(_stack, this->GetStackMarker(), previousTop);
                _links.Delete(top);
            }
            return element;
        }

        public: void Push(TLinkAddress element) { _links.Update(_stack, this->GetStackMarker(), _links.GetOrCreate(this->GetTop(), element)); }
    };
}
