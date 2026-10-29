// Reproduction for CodeQL cpp/unused-{local,static}-variable.
// Compile separately; the annotations and registration side effect are intentional.
#include <array>

int registrations = 0;

int* Register()
{
    ++registrations;
    return &registrations;
}

[[maybe_unused]] static int* registration = Register();

void Iterate()
{
    for ([[maybe_unused]] auto value : std::array{1, 2})
    {
    }
}

int main()
{
    Iterate();
    return registrations == 1 ? 0 : 1;
}
