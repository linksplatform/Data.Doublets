#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>
#include "ILinksTestExtensions.h"

using TLinkAddress = std::uint64_t;
constexpr Platform::Data::LinksConstants<TLinkAddress> constants {true};
#include "GenericLinksTests.cpp"
//#include "UnitedMemoryLinksFfiTests.cpp"
