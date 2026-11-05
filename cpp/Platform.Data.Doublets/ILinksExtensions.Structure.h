#include <concepts>
#include <ostream>
#include <sstream>
#include <string>
#include <unordered_set>

namespace Platform::Data::Doublets
{
    /// Appends a structure, sharing visited addresses across calls. Callbacks receive available endpoint links.
    /// Self references are numeric leaves; repeated links use '*' and missing links use '~' in debug mode.
    template <typename TStorage, typename TIsElement, typename TAppendElement>
    void AppendStructure(TStorage& storage, std::ostream& output,
                         std::unordered_set<typename TStorage::LinkAddressType>& visited,
                         typename TStorage::LinkAddressType address, const TIsElement& isElement,
                         const TAppendElement& appendElement, bool renderIndex = false, bool renderDebug = false)
    {
        using Address = typename TStorage::LinkAddressType;
        const auto& constants = storage.Constants;
        if (address == constants.Null || address == constants.Any || address == constants.Itself)
            return;
        if (!Exists(storage, address))
        {
            if (renderDebug)
                output << '~';
            output << std::to_string(address);
            return;
        }
        if (!visited.insert(address).second)
        {
            if (renderDebug)
                output << '*';
            output << std::to_string(address);
            return;
        }
        const Link<Address> link{GetLink(storage, address)};
        output << '(';
        if (renderIndex)
            output << std::to_string(address) << ':';
        auto appendReference = [&](Address reference) {
            if (reference == address)
            {
                output << std::to_string(reference);
                return;
            }
            // Missing raw sequence values must not be dereferenced (issue #242).
            if (reference != constants.Null && reference != constants.Any && reference != constants.Itself &&
                Exists(storage, reference))
            {
                const Link<Address> element{GetLink(storage, reference)};
                if (isElement(element))
                {
                    appendElement(output, element);
                    return;
                }
            }
            AppendStructure(storage, output, visited, reference, isElement, appendElement, renderIndex, renderDebug);
        };
        appendReference(link.Source);
        output << ' ';
        appendReference(link.Target);
        output << ')';
    }

    /// Formats a structure with custom rendering for links accepted by isElement.
    template <typename TStorage, typename TIsElement, typename TAppendElement>
        requires std::invocable<const TAppendElement&, std::ostream&, const Link<typename TStorage::LinkAddressType>&>
    std::string FormatStructure(TStorage& storage, typename TStorage::LinkAddressType address,
                                const TIsElement& isElement, const TAppendElement& appendElement,
                                bool renderIndex = false, bool renderDebug = false)
    {
        std::ostringstream output;
        std::unordered_set<typename TStorage::LinkAddressType> visited;
        AppendStructure(storage, output, visited, address, isElement, appendElement, renderIndex, renderDebug);
        return output.str();
    }

    /// Formats a structure as parenthesized source/target pairs, matching the C# formatter.
    template <typename TStorage, typename TIsElement>
    std::string FormatStructure(TStorage& storage, typename TStorage::LinkAddressType address,
                                const TIsElement& isElement, bool renderIndex = false, bool renderDebug = false)
    {
        auto appendElement = [](std::ostream& output, const Link<typename TStorage::LinkAddressType>& element) {
            output << std::to_string(element.Index);
        };
        return FormatStructure(storage, address, isElement, appendElement, renderIndex, renderDebug);
    }
} // namespace Platform::Data::Doublets
