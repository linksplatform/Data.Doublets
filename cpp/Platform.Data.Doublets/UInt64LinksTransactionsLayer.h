namespace Platform::Data::Doublets
{
    class UInt64LinksTransactionsLayer : public LinksDisposableDecoratorBase<std::uint64_t>
    {
        struct Transition : public IEquatable<Transition>
        {
            public: inline static const std::int64_t Size = Structure<Transition>.Size;

            public: std::uint64_t TransactionId = 0;
            public: Link<std::uint64_t> Before;
            public: Link<std::uint64_t> After;
            public: Timestamp Timestamp = 0;

            public: Transition(UniqueTimestampFactory uniqueTimestampFactory, std::uint64_t transactionId, Link<std::uint64_t> before, Link<std::uint64_t> after)
            {
                TransactionId = transactionId;
                Before = before;
                After = after;
                Timestamp = uniqueTimestampFactory.Create();
            }

            public: Transition(UniqueTimestampFactory uniqueTimestampFactory, std::uint64_t transactionId, Link<std::uint64_t> before) : this(uniqueTimestampFactory, transactionId, before, 0) { }

            public: Transition(UniqueTimestampFactory uniqueTimestampFactory, std::uint64_t transactionId) : this(uniqueTimestampFactory, transactionId, 0, 0) { }

            public: override std::string ToString() { return std::string("").append(Platform::Converters::To<std::string>(Timestamp)).append(1, ' ').append(Platform::Converters::To<std::string>(TransactionId)).append(": ").append(Platform::Converters::To<std::string>(Before)).append(" => ").append(Platform::Converters::To<std::string>(After)).append(""); }

            public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(TransactionId, Before, After, Timestamp); }

            public: bool operator ==(const Transition &other) const { return TransactionId == other.TransactionId && Before == other.Before && After == other.After && Timestamp == other.Timestamp; }
        }

        class Transaction : public DisposableBase
        {
            private: Queue<Transition> _transitions;
            private: UInt64LinksTransactionsLayer _layer = 0;
            public: inline bool IsCommitted;
            public: inline bool IsReverted;

            public: Transaction(UInt64LinksTransactionsLayer layer)
            {
                _layer = layer;
                if (_layer._currentTransactionId != 0)
                {
                    throw throw std::logic_error("Not supported exception.");
                }
                IsCommitted = false;
                IsReverted = false;
                _transitions = Queue<Transition>();
                this->SetCurrentTransaction(layer, this);
            }

            public: void Commit()
            {
                this->EnsureTransactionAllowsWriteOperations(this);
                while (_transitions.Count() > 0)
                {
                    auto transition = _transitions.Dequeue();
                    _layer._transitions.Enqueue(transition);
                }
                _layer._lastCommitedTransactionId = _layer._currentTransactionId;
                IsCommitted = true;
            }

            private: void Revert()
            {
                this->EnsureTransactionAllowsWriteOperations(this);
                auto transitionsToRevert = Transition[_transitions.Count()];
                _transitions.CopyTo(transitionsToRevert, 0);
                for (auto i = transitionsToRevert.Length - 1; i >= 0; i--)
                {
                    _layer.RevertTransition(transitionsToRevert[i]);
                }
                IsReverted = true;
            }

            public: static void SetCurrentTransaction(UInt64LinksTransactionsLayer layer, Transaction transaction)
            {
                layer._currentTransactionId = layer._lastCommitedTransactionId + 1;
                layer._currentTransactionTransitions = transaction._transitions;
                layer._currentTransaction = transaction;
            }

            public: static void EnsureTransactionAllowsWriteOperations(Transaction transaction)
            {
                if (transaction.IsReverted)
                {
                    throw std::runtime_error("Transation is reverted.");
                }
                if (transaction.IsCommitted)
                {
                    throw std::runtime_error("Transation is commited.");
                }
            }

            protected: void Dispose(bool manual, bool wasDisposed) override
            {
                if (!wasDisposed && _layer != nullptr && !_layer.Disposable.IsDisposed)
                {
                    if (!IsCommitted && !IsReverted)
                    {
                        this->Revert();
                    }
                    _layer.ResetCurrentTransation();
                }
            }
        }

        public: inline static const TimeSpan DefaultPushDelay = TimeSpan.FromSeconds(0.1);

        private: std::string _logAddress = 0;
        private: FileStream _log = 0;
        private: Queue<Transition> _transitions;
        private: UniqueTimestampFactory _uniqueTimestampFactory = 0;
        private: Task _transitionsPusher = 0;
        private: Transition _lastCommitedTransition = 0;
        private: std::uint64_t _currentTransactionId = 0;
        private: Queue<Transition> _currentTransactionTransitions;
        private: Transaction _currentTransaction = 0;
        private: std::uint64_t _lastCommitedTransactionId = 0;

        public: UInt64LinksTransactionsLayer(ILinks<std::uint64_t> &storage, std::string logAddress)
            : base(storage)
        {
            if (std::string.IsNullOrWhiteSpace(logAddress))
            {
                throw std::invalid_argument("logAddress");
            }
            auto lastCommitedTransition = FileHelpers.ReadFirstOrDefault<Transition>(logAddress);
            auto lastWrittenTransition = FileHelpers.ReadLastOrDefault<Transition>(logAddress);
            if (!lastCommitedTransition.Equals(lastWrittenTransition))
            {
                this->Dispose();
                throw throw std::logic_error("Not supported exception.");
            }
            if (lastCommitedTransition == 0)
            {
                FileHelpers.WriteFirst(logAddress, lastCommitedTransition);
            }
            _lastCommitedTransition = lastCommitedTransition;
            auto allTransitions = FileHelpers.ReadAll<Transition>(logAddress);
            _lastCommitedTransactionId = allTransitions.Length > 0 ? allTransitions.Max(x => x.TransactionId) : 0;
            _uniqueTimestampFactory = this->UniqueTimestampFactory();
            _logAddress = logAddress;
            _log = FileHelpers.Append(logAddress);
            _transitions = Queue<Transition>();
            _transitionsPusher = this->Task(TransitionsPusher);
            _transitionsPusher.Start();
        }

        public: IList<std::uint64_t> GetLinkValue(std::uint64_t link) { return _links.GetLink(link); }

        public: std::uint64_t Create(IList<std::uint64_t> &restrictions) override
        {
            auto createdLinkIndex = _links.Create();
            auto createdLink = Link<std::uint64_t>(_links.GetLink(createdLinkIndex));
            this->CommitTransition(this->Transition(_uniqueTimestampFactory, _currentTransactionId, 0, createdLink));
            return createdLinkIndex;
        }

        public: std::uint64_t Update(IList<std::uint64_t> &restrictions, IList<std::uint64_t> &substitution) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto beforeLink = Link<std::uint64_t>(_links.GetLink(linkIndex));
            linkIndex = _links.Update(restrictions, substitution);
            auto afterLink = Link<std::uint64_t>(_links.GetLink(linkIndex));
            this->CommitTransition(this->Transition(_uniqueTimestampFactory, _currentTransactionId, beforeLink, afterLink));
            return linkIndex;
        }

        public: void Delete(IList<std::uint64_t> &restrictions) override
        {
            auto link = restrictions[_constants.IndexPart];
            auto deletedLink = Link<std::uint64_t>(_links.GetLink(link));
            _links.Delete(link);
            this->CommitTransition(this->Transition(_uniqueTimestampFactory, _currentTransactionId, deletedLink, 0));
        }

        private: Queue<Transition> GetCurrentTransitions() { return _currentTransactionTransitions ?? _transitions; }

        private: void CommitTransition(Transition transition)
        {
            if (_currentTransaction != nullptr)
            {
                Transaction.EnsureTransactionAllowsWriteOperations(_currentTransaction);
            }
            auto transitions = this->GetCurrentTransitions();
            transitions.Enqueue(transition);
        }

        private: void RevertTransition(Transition transition)
        {
            if (transition.After.IsNull())
            {
                _links.Create();
            }
            else if (transition.Before.IsNull())
            {
                _links.Delete(transition.After.Index);
            }
            else
            {
                _links.Update(new[] { transition.After.Index, transition.Before.Source, transition.Before.Target });
            }
        }

        private: void ResetCurrentTransation()
        {
            _currentTransactionId = 0;
            _currentTransactionTransitions = {};
            _currentTransaction = {};
        }

        private: void PushTransitions()
        {
            if (_log == nullptr || _transitions == nullptr)
            {
                return;
            }
            for (auto i = 0; i < _transitions.Count(); i++)
            {
                auto transition = _transitions.Dequeue();

                _log.Write(transition);
                _lastCommitedTransition = transition;
            }
        }

        private: void TransitionsPusher()
        {
            while (!Disposable.IsDisposed && _transitionsPusher != nullptr)
            {
                Thread.Sleep(DefaultPushDelay);
                this->PushTransitions();
            }
        }

        public: Transaction BeginTransaction() { return this->Transaction(this); }

        private: void DisposeTransitions()
        {
            try
            {
                auto pusher = _transitionsPusher;
                if (pusher != nullptr)
                {
                    _transitionsPusher = {};
                    pusher.Wait();
                }
                if (_transitions != nullptr)
                {
                    this->PushTransitions();
                }
                _log.DisposeIfPossible();
                FileHelpers.WriteFirst(_logAddress, _lastCommitedTransition);
            }
            catch (const std::exception& ex)
            {
                Platform::Exceptions::ExceptionExtensions::Ignore(ex);
            }
        }

        protected: void Dispose(bool manual, bool wasDisposed) override
        {
            if (!wasDisposed)
            {
                this->DisposeTransitions();
            }
            base.Dispose(manual, wasDisposed);
        }
    };
}
