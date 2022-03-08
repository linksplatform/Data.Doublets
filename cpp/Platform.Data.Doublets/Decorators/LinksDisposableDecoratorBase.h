namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksDisposableDecoratorBase;
    template <typename TLink> class LinksDisposableDecoratorBase<TLink> : public LinksDecoratorBase<TLink>, ILinks<TLink>, System::IDisposable
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

        protected: LinksDisposableDecoratorBase(ILinks<TLink> &storage) : base(storage) { return Disposable = DisposableWithMultipleCallsAllowed(Dispose); }

        ~LinksDisposableDecoratorBase() { Disposable.Destruct(); }

        public: void Dispose() { Disposable.Dispose(); }

        protected: virtual void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                _links.DisposeIfPossible();
            }
        }
    };
}
