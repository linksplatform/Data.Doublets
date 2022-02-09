using namespace Platform::Data::Doublets::Memory::United::Generic::Ffi::externC;

namespace Platform::Data::Doublets::Tests
{
    TEST(UnitedMemoryLinksFfiTests, ConstructorTest)
    {
        auto storage = ByteUnitedMemoryLinksFfi_New("db.links");
        ByteUnitedMemoryLinksFfi_Drop(storage);
    };
}
