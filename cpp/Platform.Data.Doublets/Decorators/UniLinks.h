namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class UniLinks;
    template <typename TLink> class UniLinks<TLink> : public LinksDecoratorBase<TLink>, IUniLinks<TLink>
    {
        public: UniLinks(ILinks<TLink> &links) : base(links) { }

        struct Transition
        {
            public: IList<TLink> *Before;
            public: IList<TLink> *After;

            public: Transition(IList<TLink> &before, IList<TLink> &after)
            {
                Before = before;
                After = after;
            }
        }

        public: TLink Trigger(IList<TLink> &restriction, Func<IList<TLink>, IList<TLink>, TLink> matchedHandler, IList<TLink> &substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutedHandler)
        {
            return _constants.Continue;
        }

        public: TLink Trigger(IList<TLink> &patternOrCondition, Func<IList<TLink>, TLink> matchHandler, IList<TLink> &substitution, Func<IList<TLink>, IList<TLink>, TLink> substitutionHandler)
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
                    auto newLink = _links.Create();
                    after[0] = newLink;
                }
                if (substitution.Count() == 1)
                {
                    after = _links.GetLink(substitution[0]);
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
                    auto before = _links.GetLink(linkToDelete);
                    if (matchHandler != nullptr && this->matchHandler(before) == constants.Break)
                    {
                        return constants.Break;
                    }
                    auto after = Array.Empty<TLink>();
                    _links.Update(linkToDelete, constants.Null, constants.Null);
                    _links.Delete(linkToDelete);
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
                    auto before = _links.GetLink(linkToUpdate);
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
                            after = _links.GetLink(substitution[0]);
                            _links.Update(linkToUpdate, constants.Null, constants.Null);
                            _links.Delete(linkToUpdate);
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

        public: IList<IList<IList<TLink>>> Trigger(IList<TLink> &condition, IList<TLink> &substitution)
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

        private: TLink AlwaysContinue(IList<TLink> &linkToMatch) { return _constants.Continue; }
    };
}