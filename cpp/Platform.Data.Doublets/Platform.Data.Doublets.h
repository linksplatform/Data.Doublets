#ifndef PLATFORM_DATA_DOUBLETS
#define PLATFORM_DATA_DOUBLETS

#include <cstdarg>
#include <cstdint>
#include <cstdlib>
#include <ostream>
#include <new>
#include <map>
#include <ranges>
#include <unordered_set>

#include <Platform.Exceptions.h>
#include <Platform.Collections.Methods.h>
#include <Platform.Collections.h>
#include <Platform.Threading.h>
#include <Platform.Memory.h>
#include <Platform.Data.h>
#include <Platform.Interfaces.h>

#include "Doublet.h"
#include "Link.h"
#include "LinksOptions.h"
#include "ILinks.h"

#include "Memory/LinksHeader.h"
#include "Memory/IndexTreeType.h"
#include "Memory/ILinksListMethods.h"
#include "Memory/ILinksTreeMethods.h"

#include "Memory/United/RawLink.h"
#include "Memory/United/Generic/UnusedLinksListMethods.h"

#include "Memory/United/Generic/LinksSizeBalancedTreeMethodsBase.h"
#include "Memory/United/Generic/LinksTargetsSizeBalancedTreeMethods.h"
#include "Memory/United/Generic/LinksSourcesSizeBalancedTreeMethods.h"
#include "Memory/United/Generic/UnitedMemoryLinksBase.h"
#include "Memory/United/Generic/UnitedMemoryLinks.h"
//#include "Ffi/LinksBase.h"
//#include "Ffi/Links.h"


#include "Memory/Split/RawLinkDataPart.h"
#include "Memory/Split/RawLinkIndexPart.h"
#include "Memory/Split/Generic/ExternalLinksRecursionlessSizeBalancedTreeMethodsBase.h"
#include "Memory/Split/Generic/ExternalLinksSizeBalancedTreeMethodsBase.h"
#include "Memory/Split/Generic/ExternalLinksSourcesRecursionlessSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/ExternalLinksSourcesSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/ExternalLinksTargetsRecursionlessSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/ExternalLinksTargetsSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/InternalLinksRecursionlessSizeBalancedTreeMethodsBase.h"
#include "Memory/Split/Generic/InternalLinksSizeBalancedTreeMethodsBase.h"
#include "Memory/Split/Generic/InternalLinksSourcesLinkedListMethods.h"
#include "Memory/Split/Generic/InternalLinksSourcesRecursionlessSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/InternalLinksSourcesSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/InternalLinksTargetsRecursionlessSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/InternalLinksTargetsSizeBalancedTreeMethods.h"
#include "Memory/Split/Generic/UnusedLinksListMethods.h"
#include "Memory/Split/Generic/SplitMemoryLinksBase.h"
#include "Memory/Split/Generic/SplitMemoryLinks.h"

#include "ILinksExtensions.h"

#include "Decorators/LinksUniquenessResolver.h"
#include "Decorators/LinksCascadeUniquenessAndUsagesResolver.h"
#include "Decorators/LinksCascadeUsagesResolver.h"
#include "Decorators/NonNullContentsLinkDeletionResolver.h"
#include "Decorators/Decorators.h"

#endif
