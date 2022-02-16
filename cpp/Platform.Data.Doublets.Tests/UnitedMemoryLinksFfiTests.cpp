//using namespace Platform::Data::Doublets::Memory::United::Generic::Ffi::externC;

namespace Platform::Data::Doublets::Tests
{
    TEST(UnitedMemoryLinksFfiTests, ConstructorTest)
    {
        auto storage = Platform::Data::Doublets::Memory::United::Generic::Ffi::ByteUnitedMemoryLinks_New("db.links");
        Platform::Data::Doublets::Memory::United::Generic::Ffi::ByteUnitedMemoryLinks_Drop(storage);
    };
}
