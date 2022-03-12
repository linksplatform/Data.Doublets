#define DECLARE_LINKS_CONSTANTS($TLinkAddress, $Prefix, $PrefixUpperCase) \
    typedef struct $Prefix##LinksConstantsType                            \
    {                                                                     \
        $TLinkAddress Index_part;                                         \
        $TLinkAddress Source_part;                                        \
        $TLinkAddress Target_part;                                        \
        $TLinkAddress Null;                                               \
        $TLinkAddress Continue;                                          \
        $TLinkAddress Break;                                             \
        $TLinkAddress Skip;                                               \
        $TLinkAddress Any;                                                \
        $TLinkAddress Itself;                                             \
        $TLinkAddress Error;                                              \
        $Prefix##Range Internal_range;                                    \
        $Prefix##Range External_range;                                    \
        bool _opt_marker;                                                 \
    } $Prefix##LinksConstantsType;                                        \
                                                                          \
    $Prefix##LinksConstantsType Default##$Prefix##LinksConstants = {      \
        .Index_part = 0,                                                  \
        .Source_part = 1,                                                 \
        .Target_part = 2,                                                 \
        .Null = 0,                                                        \
        .Continue = $PrefixUpperCase##_MAX,                              \
        .Break = $PrefixUpperCase##_MAX - 1,                                                      \
        .Skip = $PrefixUpperCase##_MAX - 2,                               \
        .Any = $PrefixUpperCase##_MAX - 3,                                \
        .Itself = $PrefixUpperCase##_MAX - 4,                             \
        .Error = $PrefixUpperCase##_MAX - 5,                                                       \
        .Internal_range = {                                               \
            .Minimum = 0,                                                 \
            .Maximum = $PrefixUpperCase##_MAX - 6,                        \
        },                                                                \
        .External_range = {                                               \
            .Minimum = ($PrefixUpperCase##_MAX / 2) + 1,                  \
            .Maximum = $PrefixUpperCase##_MAX,                            \
        },                                                                \
    };
