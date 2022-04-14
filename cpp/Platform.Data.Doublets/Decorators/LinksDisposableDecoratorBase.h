namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksDisposableDecoratorBase;
    template <typename TLinkAddress> class LinksDisposableDecoratorBase<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>, ILinks<TLinkAddress>, System::IDisposable
    {
        class DisposableWithMultipleCallsAllowed : public Disposable
        {
            public: DisposableWithMultipleCallsAllowed(std::function<Disposal> disposal) : base(disposal) { }

            protected: override bool AllowMultipleDisposeCalls
            {
                get => true;
            }
        }

        protected: DisposableWithMultipleCallsAllowed Disposable = 0;

        protected: LinksDisposableDecoratorBase(ILinks<TLinkAddress> &storage) : base(storage) { return Disposable = DisposableWithMultipleCallsAllowed(Dispose); }

        ~LinksDisposableDecoratorBase() { Disposable.Destruct(); }

        public: void Dispose() { Disposable.Dispose(); }

        protected: virtual void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                this->decorated().DisposeIfPossible();
            }
        }
    };
}
