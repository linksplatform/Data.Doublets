namespace Platform::Data::Doublets
{
    template <typename ...> struct Link;
    template <typename TLink> struct Link<TLink> : public IEquatable<Link<TLink>>, IReadOnlyList<TLink>, IList<TLink>
    {
        public: inline static Link<TLink> Null;

        private: static readonly LinksConstants<TLink> _constants = Default<LinksConstants<TLink>>.Instance;

        private: inline static const std::int32_t Length = 3;

        public: TLink Index = 0;
        public: TLink Source = 0;
        public: TLink Target = 0;

        public: Link(params TLink values[]) { this->SetValues(values, Index, Source, out Target); }

        public: Link(IList<TLink> &values) { this->SetValues(values, Index, Source, out Target); }

        public: Link(void *other)
        {
            if (other is Link<TLink> otherLink)
            {
                this->SetValues(otherLink, Index, Source, out Target);
            }
            else this->if(other is IList<TLink> &otherList)
            {
                this->SetValues(otherList, Index, Source, out Target);
            }
            else
            {
                throw std::logic_error("Not supported exception.");
            }
        }

        public: Link(ref Link<TLink> other) { this->SetValues(other, Index, Source, out Target); }

        public: Link(TLink index, TLink source, TLink target)
        {
            Index = index;
            Source = source;
            Target = target;
        }

        private: static void SetValues(ref Link<TLink> other, out TLink index, out TLink source, out TLink target)
        {
            index = other.Index;
            source = other.Source;
            target = other.Target;
        }

        private: static void SetValues(IList<TLink> &values, out TLink index, out TLink source, out TLink target)
        {
            switch (values.Count())
            {
                case 3:
                    index = values[0];
                    source = values[1];
                    target = values[2];
                    break;
                case 2:
                    index = values[0];
                    source = values[1];
                    target = 0;
                    break;
                case 1:
                    index = values[0];
                    source = 0;
                    target = 0;
                    break;
                0:
                    index = 0;
                    source = 0;
                    target = 0;
                    break;
            }
        }

        public: bool IsNull() => Index == _constants.Null
                             && Source == _constants.Null
                             && Target == _constants.Null;

        public: bool Equals(void *other) override { return other is Link<TLink> && this->Equals((Link<TLink>)other); }

        public: bool Equals(Link<TLink> other) => Index == other.Index
                                              && Source == other.Source
                                              && Target == other.Target;

        public: static std::string ToString(TLink index, TLink source, TLink target) { return std::string("(").append(Platform::Converters::To<std::string>(index)).append(": ").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }

        public: static std::string ToString(TLink source, TLink target) { return std::string("(").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }

        public: operator TLink[]() const { return link.ToArray(); }

        public: Link(TLink linkArray[]) : Link(linkArray) { }

        public: operator std::string() const { return Index == _constants.Null ? ToString(Source, Target) : ToString(Index, Source, Target); }

        public: friend std::ostream & operator <<(std::ostream &out, const Link<TLink> &obj) { return out << (std::string)obj; }

        public: std::int32_t Count()
        {
            return Length;
        }

        public: bool IsReadOnly()
        {
            return true;
        }

        public: TLink this[std::int32_t index]
        {
            get
            {
                Platform::Ranges::EnsureExtensions::ArgumentInRange(Platform::Exceptions::Ensure::OnDebug, index, Range<std::int32_t>(0, Length - 1), "index");
                if (index == _constants.IndexPart)
                {
                    return Index;
                }
                if (index == _constants.SourcePart)
                {
                    return Source;
                }
                if (index == _constants.TargetPart)
                {
                    return Target;
                }
                throw std::logic_error("Not supported exception.");
            }
            set => throw std::logic_error("Not supported exception.");
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public: IEnumerator<TLink> GetEnumerator()
        {
            yield return Index;
            yield return Source;
            yield return Target;
        }

        public: void Add(TLink item) { throw std::logic_error("Not supported exception."); }

        public: void Clear() { throw std::logic_error("Not supported exception."); }

        public: bool Contains(TLink item) { return this->IndexOf(item) >= 0; }

        public: void CopyTo(TLink array[], std::int32_t arrayIndex)
        {
            Platform::Exceptions::EnsureExtensions::ArgumentNotNull(Platform::Exceptions::Ensure::OnDebug, array, "array");
            Platform::Ranges::EnsureExtensions::ArgumentInRange(Platform::Exceptions::Ensure::OnDebug, arrayIndex, Range<std::int32_t>(0, array.Length - 1), "arrayIndex");
            if (arrayIndex + Length > array.Length)
            {
                throw std::runtime_error();
            }
            array[arrayIndex++] = Index;
            array[arrayIndex++] = Source;
            array[arrayIndex] = Target;
        }

        public: bool Remove(TLink item) { return Throw.A.NotSupportedExceptionAndReturn<bool>(); }

        public: std::int32_t IndexOf(TLink item)
        {
            if (Index == item)
            {
                return _constants.IndexPart;
            }
            if (Source == item)
            {
                return _constants.SourcePart;
            }
            if (Target == item)
            {
                return _constants.TargetPart;
            }
            return -1;
        }

        public: void Insert(std::int32_t index, TLink item) { throw std::logic_error("Not supported exception."); }

        public: void RemoveAt(std::int32_t index) { throw std::logic_error("Not supported exception."); }
    };
}

namespace std
{
    template <typename TLink>
    struct hash<Platform::Data::Doublets::Link<TLink>>
    {
        std::size_t operator()(const Platform::Data::Doublets::Link<TLink> &obj) const
        {
            return Platform::Hashing::Hash(obj.Index, obj.Source, obj.Target);
        }
    };
}
