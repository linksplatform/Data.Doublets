namespace Platform::Data::Doublets
{
    template <typename ...> class SynchronizedLinks;
    template <std::integral TLinkAddress> class SynchronizedLinks<TLinkAddress> : public ISynchronizedLinks<TLinkAddress>
    {
        public: const LinksConstants<TLinkAddress> Constants;

        public: const ISynchronization *SyncRoot;

        public: const ILinks<TLinkAddress> *Sync;

        public: const ILinks<TLinkAddress> *Unsync;

        public: SynchronizedLinks(ILinks<TLinkAddress> &storage) : this(ReaderWriterLockSynchronization(), storage) { }

        public: SynchronizedLinks(ISynchronization &synchronization, ILinks<TLinkAddress> &storage)
        {
            SyncRoot = synchronization;
            Sync = this;
            Unsync = storage;
            Constants = storage.Constants;
        }

        public: TLinkAddress Count(IList<TLinkAddress> &restriction) { return SyncRoot.ExecuteReadOperation(restriction, Unsync.Count()); }

        public: TLinkAddress Each(Func<IList<TLinkAddress>, TLinkAddress> handler, IList<TLinkAddress> &restriction) { return SyncRoot.ExecuteReadOperation(handler, restriction, (handler1, restriction1) { return Unsync.Each(restriction1, handler1)); } }

        public: TLinkAddress Create(IList<TLinkAddress> &restriction) { return SyncRoot.ExecuteWriteOperation(restriction, Unsync.Create); }

        public: TLinkAddress Update(IList<TLinkAddress> &restriction, IList<TLinkAddress> &substitution) { return SyncRoot.ExecuteWriteOperation(restriction, substitution, Unsync.Update); }

        public: void Delete(IList<TLinkAddress> &restriction) { SyncRoot.ExecuteWriteOperation(restriction, Unsync.Delete); }
    };
}
