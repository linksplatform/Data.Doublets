cmake_minimum_required(VERSION 3.13)

set(LINKS_PLATFORM_TESTS OFF CACHE BOOL "Whether to compile tests")
set(LINKS_PLATFORM_BENCHMARKS OFF CACHE BOOL "Whether to compile benchmarks")
set(LINKS_PLATFORM_EXTRA_FLAGS "" CACHE STRING "Extra compiler flags")

set(CONAN_DISABLE_CHECK_COMPILER TRUE)
set(LINKS_PLATFORM_TESTS TRUE)
set(LINKS_PLATFORM_BENCHMARKS TRUE)

project(Platform.Data.Doublets CXX)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

find_package(GTest)
find_package(benchmark)
find_package(Platform.Interfaces)
find_package(Platform.Collections.Methods)
find_package(Platform.Collections)
find_package(Platform.Numbers)
find_package(Platform.Memory)
find_package(Platform.Exceptions)
find_package(Platform.Data)
find_package(Platform.Setters)
find_package(Platform.Ranges)
find_package(mio)

add_library(${PROJECT_NAME}.Library INTERFACE)
target_include_directories(${PROJECT_NAME}.Library INTERFACE ${PROJECT_NAME})
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Interfaces::Platform.Interfaces)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Collections.Methods::Platform.Collections.Methods)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Collections::Platform.Collections)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Numbers::Platform.Numbers)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Memory::Platform.Memory)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Exceptions::Platform.Exceptions)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Data::Platform.Data)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Setters::Platform.Setters)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE Platform.Ranges::Platform.Ranges)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE mio::mio)

target_link_directories(${PROJECT_NAME}.Library INTERFACE ${CMAKE_CURRENT_LIST_DIR})
#target_link_libraries(${PROJECT_NAME}.Library INTERFACE doublets_ffi)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE ${CMAKE_DL_LIBS})

target_compile_options(${PROJECT_NAME}.Library INTERFACE ${LINKS_PLATFORM_EXTRA_FLAGS})

if(${LINKS_PLATFORM_TESTS})
    add_executable(${PROJECT_NAME}.Tests ${PROJECT_NAME}.Tests/AllTests.cpp)
    set_target_properties(${PROJECT_NAME}.Tests PROPERTIES CXX_STANDARD 20)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE GTest::gtest)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE GTest::gtest_main)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE ${PROJECT_NAME}.Library)
endif()

if(${LINKS_PLATFORM_BENCHMARKS})
    add_executable(${PROJECT_NAME}.Benchmarks ${PROJECT_NAME}.Benchmarks/AllBenchmarks.cpp)
    set_target_properties(${PROJECT_NAME}.Benchmarks PROPERTIES CXX_STANDARD 20)
    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE benchmark::benchmark)
    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE ${PROJECT_NAME}.Library)
endif()
