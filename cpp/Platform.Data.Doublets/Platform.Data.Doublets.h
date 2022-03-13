#ifndef PLATFORM_DATA_DOUBLETS
#define PLATFORM_DATA_DOUBLETS

#include <cstdarg>
#include <cstdint>
#include <cstdlib>
#include <ostream>
#include <new>
#include <map>
#include <ranges>

#include <Platform.Collections.Methods.h>

#include <Platform.Collections.h>
#include <Platform.Threading.h>
#include <Platform.Memory.h>
#include <Platform.Data.h>

#include "Doublet.h"
#include "Link.h"
//#include "ILinks.h"

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
#include "Memory/United/Ffi/UnitedMemoryLinksBase.h"
#include "Memory/United/Ffi/UnitedMemoryLinks.h"

#include "ILinksExtensions.h"

#include "Decorators/LinksDecoratorBase.h"
#include "Decorators/LinksUniquenessResolver.h"
#include "Decorators/LinksCascadeUniquenessAndUsagesResolver.h"
#include "Decorators/LinksCascadeUsagesResolver.h"
#include "Decorators/NonNullContentsLinkDeletionResolver.h"
#include "Decorators/Decorators.h"
//#include "Decorators/LinksCascadeUniquenessAndUsagesResolverBase.h"

#endif
