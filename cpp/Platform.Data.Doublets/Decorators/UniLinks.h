namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class UniLinks;
    template <std::integral TLinkAddress> class UniLinks<TLinkAddress> : public DecoratorBase<TFacade, TDecorated>, IUniLinks<TLinkAddress>
    {
        public: UniLinks(ILinks<TLinkAddress> &storage) : base(storage) { }

        struct Transition
        {
            public: IList<TLinkAddress> *Before;
            public: IList<TLinkAddress> *After;

            public: Transition( const LinkType& before,  const LinkType& after)
            {
                Before = before;
                After = after;
            }
        }

        public: TLinkAddress Trigger(const  LinkType& restriction, Func<IList<TLinkAddress>, IList<TLinkAddress>, TLinkAddress> matchedHandler, const LinkType& substitution, Func<IList<TLinkAddress>, IList<TLinkAddress>, TLinkAddress> substitutedHandler)
        {
            return _constants.Continue;
        }

        public: TLinkAddress Trigger( const LinkType& patternOrCondition, Func<IList<TLinkAddress>, TLinkAddress> matchHandler, const LinkType& substitution, Func<IList<TLinkAddress>, IList<TLinkAddress>, TLinkAddress> substitutionHandler)
        {
            auto constants = _constants;
            if (patternOrCondition.IsNullOrEmpty() && substitution.IsNullOrEmpty())
            {
                return constants.Continue;
            }
            else if (patternOrCondition.EqualTo(substitution))
            {
                throw std::logic_error("Not implemented exception.");
            }
            else if (!substitution.IsNullOrEmpty())
            {
                auto before = Array.Empty<TLinkAddress>();
                if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                {
                    return constants.Break;
                }
                auto after = (IList<TLinkAddress>)substitution.ToArray();
                if (after[0] == 0)
                {
                    auto newLink = this->decorated().TDecorated::Create();
                    after[0] = newLink;
                }
                if (substitution.Count() == 1)
                {
                    after = this->decorated().TDecorated::GetLink(substitution[0]);
                }
                else if (substitution.Count() == 3)
                {
                }
                else
                {
                    throw std::logic_error("Not supported exception.");
                }
                if (matchHandler != nullptr)
                {
                    return this->substitutionHandler(before, after);
                }
                return constants.Continue;
            }
            else if (!patternOrCondition.IsNullOrEmpty())
            {
                if (patternOrCondition.Count() == 1)
                {
                    auto linkToDelete = patternOrCondition[0];
                    auto before = this->decorated().TDecorated::GetLink(linkToDelete);
                    if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                    {
                        return constants.Break;
                    }
                    auto after = Array.Empty<TLinkAddress>();
                    this->decorated().TDecorated::Update(linkToDelete, constants.Null, constants.Null);
                    this->decorated().TDecorated::Delete(linkToDelete);
                    if (matchHandler != nullptr)
                    {
                        return this->substitutionHandler(before, after);
                    }
                    return constants.Continue;
                }
                else
                {
                    throw std::logic_error("Not supported exception.");
                }
            }
            else
            {
                if (patternOrCondition.Count() == 1)
                {
                    auto linkToUpdate = patternOrCondition[0];
                    auto before = this->decorated().TDecorated::GetLink(linkToUpdate);
                    if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                    {
                        return constants.Break;
                    }
                    auto after = (IList<TLinkAddress>)substitution.ToArray();
                    if (after[0] == 0)
                    {
                        after[0] = linkToUpdate;
                    }
                    if (substitution.Count() == 1)
                    {
                        if (!substitution[0] == linkToUpdate)
                        {
                            after = this->decorated().TDecorated::GetLink(substitution[0]);
                            this->decorated().TDecorated::Update(linkToUpdate, constants.Null, constants.Null);
                            this->decorated().TDecorated::Delete(linkToUpdate);
                        }
                    }
                    else if (substitution.Count() == 3)
                    {
                    }
                    else
                    {
                        throw std::logic_error("Not supported exception.");
                    }
                    if (matchHandler != nullptr)
                    {
                        return this->substitutionHandler(before, after);
                    }
                    return constants.Continue;
                }
                else
                {
                    throw std::logic_error("Not supported exception.");
                }
            }
        }

        public: IList<IList<IList<TLinkAddress>>> Trigger( const LinkType& condition, const LinkType& substitution)
        {
            auto changes = List<IList<IList<TLinkAddress>>>();
            auto continue = _constants.Continue;
            Trigger(condition, AlwaysContinue, substitution, (before, after) =>
            {
                auto change = new[] { before, after };
                changes.Add(change);
                return continue;
            });
            return changes;
        }

        private: TLinkAddress AlwaysContinue( const LinkType& linkToMatch) { return _constants.Continue; }
    };
}
