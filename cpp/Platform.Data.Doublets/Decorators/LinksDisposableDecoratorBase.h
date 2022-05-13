namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksDisposableDecoratorBase;
    template <typename TLinkAddress> class LinksDisposableDecoratorBase<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>, ILinks<TLinkAddress>, System::IDisposable
    {
        class DisposableWithMultipleCallsAllowed : public Disposable
        {
            public: DisposableWithMultipleCallsAllowed(std::function<Disposal> disposal) : base(disposal) { }

            public: bool AllowMultipleDisposeCalls
            {
                get => true;
            }
        }

        public: DisposableWithMultipleCallsAllowed Disposable = 0;

        public: LinksDisposableDecoratorBase(ILinks<TLinkAddress> &storage) : base(storage) { return Disposable = DisposableWithMultipleCallsAllowed(Dispose); }

        ~LinksDisposableDecoratorBase() { Disposable.Destruct(); }

        public: void Dispose() { Disposable.Dispose(); }

        public: virtual void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                this->decorated().DisposeIfPossible();
            }
        }
    };
}
