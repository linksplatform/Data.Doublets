namespace Platform::Data::Doublets::Stacks
{
    template <typename ...> class Stack;
    template <typename TLink> class Stack<TLink> : public LinksOperatorBase<TLink>, IStack<TLink>
    {
        private: TLink _stack = 0;

        public: bool IsEmpty()
        {
            return this->Peek() == _stack;
        }

        public: Stack(ILinks<TLink> &links, TLink stack) : base(links) { return _stack = stack; }

        private: TLink GetStackMarker() { return _links.GetSource(_stack); }

        private: TLink GetTop() { return _links.GetTarget(_stack); }

        public: TLink Peek() { return _links.GetTarget(this->GetTop()); }

        public: TLink Pop()
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

        public: void Push(TLink element) { _links.Update(_stack, this->GetStackMarker(), _links.GetOrCreate(this->GetTop(), element)); }
    };
}
