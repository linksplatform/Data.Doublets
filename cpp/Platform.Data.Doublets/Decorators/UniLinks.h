namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class UniLinks;
    template <typename TLink> class UniLinks<TLink> : public DecoratorBase<TFacade, TDecorated>, IUniLinks<TLink>
    {
        public: UniLinks(ILinks<TLink> &storage) : base(storage) { }

        struct Transition
        {
            public: IList<TLink> *Before;
            public: IList<TLink> *After;

            public: Transition(CArray<TLinkAddress> auto&& before, CArray<TLinkAddress> auto&& after)
            {
                Before = before;
                After = after;
            }
        }

        public: TLink Trigger(const  LinkType& restriction, Func<IList<TLink>, IList<TLink>, TLink> matchedHandler, const std::vector<LinkAddressType>& substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutedHandler)
        {
            return _constants.Continue;
        }

        public: TLink Trigger(CArray<TLinkAddress> auto&& patternOrCondition, Func<IList<TLink>, TLink> matchHandler, const std::vector<LinkAddressType>& substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutionHandler)
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
                auto before = Array.Empty<TLink>();
                if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                {
                    return constants.Break;
                }
                auto after = (IList<TLink>)substitution.ToArray();
                if (after[0] == 0)
                {
                    auto newLink = this->decorated().Create();
                    after[0] = newLink;
                }
                if (substitution.Count() == 1)
                {
                    after = this->decorated().GetLink(substitution[0]);
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
                    auto before = this->decorated().GetLink(linkToDelete);
                    if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                    {
                        return constants.Break;
                    }
                    auto after = Array.Empty<TLink>();
                    this->decorated().Update(linkToDelete, constants.Null, constants.Null);
                    this->decorated().Delete(linkToDelete);
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
                    auto before = this->decorated().GetLink(linkToUpdate);
                    if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                    {
                        return constants.Break;
                    }
                    auto after = (IList<TLink>)substitution.ToArray();
                    if (after[0] == 0)
                    {
                        after[0] = linkToUpdate;
                    }
                    if (substitution.Count() == 1)
                    {
                        if (!substitution[0] == linkToUpdate)
                        {
                            after = this->decorated().GetLink(substitution[0]);
                            this->decorated().Update(linkToUpdate, constants.Null, constants.Null);
                            this->decorated().Delete(linkToUpdate);
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

        public: IList<IList<IList<TLink>>> Trigger(CArray<TLinkAddress> auto&& condition, const std::vector<LinkAddressType>& substitution)
        {
            auto changes = List<IList<IList<TLink>>>();
            auto continue = _constants.Continue;
            Trigger(condition, AlwaysContinue, substitution, (before, after) =>
            {
                auto change = new[] { before, after };
                changes.Add(change);
                return continue;
            });
            return changes;
        }

        private: TLink AlwaysContinue(CArray<TLinkAddress> auto&& linkToMatch) { return _constants.Continue; }
    };
}
