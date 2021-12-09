namespace Platform::Data::Doublets
{
    class UInt64LinksExtensions
    {
        public: static readonly LinksConstants<std::uint64_t> Constants = Default<LinksConstants<std::uint64_t>>.Instance;

        public: static bool AnyLinkIsAny(ILinks<std::uint64_t> &links, params std::uint64_t sequence[])
        {
            if (sequence == nullptr)
            {
                return false;
            }
            auto constants = links.Constants;
            for (auto i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == constants.Any)
                {
                    return true;
                }
            }
            return false;
        }

        public: static std::string FormatStructure(ILinks<std::uint64_t> &links, std::uint64_t linkIndex, Func<Link<std::uint64_t>, bool> isElement, bool renderIndex = false, bool renderDebug = false)
        {
            std::string sb;
            auto visited = HashSet<std::uint64_t>();
            links.AppendStructure(sb, visited, linkIndex, isElement, (innerSb, link) { return innerSb.Append(link.Index), renderIndex, renderDebug); }
            return sb;
        }

        public: static std::string FormatStructure(ILinks<std::uint64_t> &links, std::uint64_t linkIndex, Func<Link<std::uint64_t>, bool> isElement, Action<StringBuilder, Link<std::uint64_t>> appendElement, bool renderIndex = false, bool renderDebug = false)
        {
            std::string sb;
            auto visited = HashSet<std::uint64_t>();
            links.AppendStructure(sb, visited, linkIndex, isElement, appendElement, renderIndex, renderDebug);
            return sb;
        }

        public: static void AppendStructure(ILinks<std::uint64_t> &links, std::string& sb, HashSet<std::uint64_t> visited, std::uint64_t linkIndex, Func<Link<std::uint64_t>, bool> isElement, Action<StringBuilder, Link<std::uint64_t>> appendElement, bool renderIndex = false, bool renderDebug = false)
        {
            if (sb == nullptr)
            {
                throw std::invalid_argument("sb");
            }
            if (linkIndex == Constants.Null || linkIndex == Constants.Any || linkIndex == Constants.Itself)
            {
                return;
            }
            if (links.Exists(linkIndex))
            {
                if (visited.Add(linkIndex))
                {
                    sb.append(Platform::Converters::To<std::string>('('));
                    auto link = Link<std::uint64_t>(links.GetLink(linkIndex));
                    if (renderIndex)
                    {
                        sb.append(Platform::Converters::To<std::string>(link.Index));
                        sb.append(Platform::Converters::To<std::string>(':'));
                    }
                    if (link.Source == link.Index)
                    {
                        sb.append(Platform::Converters::To<std::string>(link.Index));
                    }
                    else
                    {
                        auto source = Link<std::uint64_t>(links.GetLink(link.Source));
                        if (isElement(source))
                        {
                            appendElement(sb, source);
                        }
                        else
                        {
                            links.AppendStructure(sb, visited, source.Index, isElement, appendElement, renderIndex);
                        }
                    }
                    sb.append(Platform::Converters::To<std::string>(' '));
                    if (link.Target == link.Index)
                    {
                        sb.append(Platform::Converters::To<std::string>(link.Index));
                    }
                    else
                    {
                        auto target = Link<std::uint64_t>(links.GetLink(link.Target));
                        if (isElement(target))
                        {
                            appendElement(sb, target);
                        }
                        else
                        {
                            links.AppendStructure(sb, visited, target.Index, isElement, appendElement, renderIndex);
                        }
                    }
                    sb.append(Platform::Converters::To<std::string>('))');
                }
                else
                {
                    if (renderDebug)
                    {
                        sb.append(Platform::Converters::To<std::string>('*'));
                    }
                    sb.append(Platform::Converters::To<std::string>(linkIndex));
                }
            }
            else
            {
                if (renderDebug)
                {
                    sb.append(Platform::Converters::To<std::string>('~'));
                }
                sb.append(Platform::Converters::To<std::string>(linkIndex));
            }
        }
    };
}
