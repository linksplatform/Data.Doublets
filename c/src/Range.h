#define DECLARE_RANGE($TLinkAddress, $Prefix) \
    typedef struct $Prefix##Range             \
    {                                         \
        $TLinkAddress Minimum;                \
        $TLinkAddress Maximum;                \
    } $Prefix##Range;
