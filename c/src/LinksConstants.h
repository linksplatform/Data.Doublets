#define DECLARE_LINKS_CONSTANTS($TLinkAddress, $Prefix, $PrefixUpperCase) \
    typedef struct $Prefix##LinksConstantsType                            \
    {                                                                     \
        $TLinkAddress IndexPart;                                         \
        $TLinkAddress SourcePart;                                        \
        $TLinkAddress TargetPart;                                        \
        $TLinkAddress Null;                                               \
        $TLinkAddress Continue;                                          \
        $TLinkAddress Break;                                             \
        $TLinkAddress Skip;                                               \
        $TLinkAddress Any;                                                \
        $TLinkAddress Itself;                                             \
        $TLinkAddress Error;                                              \
        $Prefix##Range InternalRange;                                    \
        $Prefix##Range ExternalRange;                                    \
        bool _opt_marker;                                                 \
    } $Prefix##LinksConstantsType;                                        \
                                                                          \
    $Prefix##LinksConstantsType Default##$Prefix##LinksConstants = {      \
        .IndexPart = 0,                                                  \
        .SourcePart = 1,                                                 \
        .TargetPart = 2,                                                 \
        .Null = 0,                                                        \
        .Continue = $PrefixUpperCase##_MAX,                              \
        .Break = $PrefixUpperCase##_MAX - 1,                                                      \
        .Skip = $PrefixUpperCase##_MAX - 2,                               \
        .Any = $PrefixUpperCase##_MAX - 3,                                \
        .Itself = $PrefixUpperCase##_MAX - 4,                             \
        .Error = $PrefixUpperCase##_MAX - 5,                                                       \
        .InternalRange = {                                               \
            .Minimum = 0,                                                 \
            .Maximum = $PrefixUpperCase##_MAX - 6,                        \
        },                                                                \
        .ExternalRange = {                                               \
            .Minimum = ($PrefixUpperCase##_MAX / 2) + 1,                  \
            .Maximum = $PrefixUpperCase##_MAX,                            \
        },                                                                \
    };
