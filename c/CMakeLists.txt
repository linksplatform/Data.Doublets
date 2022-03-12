cmake_minimum_required(VERSION 3.13)

set(LINKS_PLATFORM_TESTS OFF CACHE BOOL "Whether to compile tests")
set(LINKS_PLATFORM_BENCHMARKS OFF CACHE BOOL "Whether to compile benchmarks")
set(LINKS_PLATFORM_EXTRA_FLAGS "" CACHE STRING "Extra compiler flags")
set(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -pthread -ldl")
set(CONAN_DISABLE_CHECK_COMPILER TRUE)
set(LINKS_PLATFORM_TESTS TRUE)
set(LINKS_PLATFORM_BENCHMARKS TRUE)

project(Platform.Data.Doublets)

set(CONAN_DISABLE_CHECK_COMPILER TRUE)
include(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup(TARGETS)
list(APPEND LINKS_PLATFORM_EXTRA_FLAGS ${CONAN_USER_PLATFORM.HASHING_suggested_flags})


add_library(${PROJECT_NAME}.Library INTERFACE)
target_include_directories(${PROJECT_NAME}.Library INTERFACE src)

target_link_directories(${PROJECT_NAME}.Library INTERFACE ${CMAKE_CURRENT_LIST_DIR})
target_link_libraries(${PROJECT_NAME}.Library INTERFACE doublets_ffi)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE ${CMAKE_DL_LIBS})

target_compile_options(${PROJECT_NAME}.Library INTERFACE ${LINKS_PLATFORM_EXTRA_FLAGS})

if(${LINKS_PLATFORM_TESTS})
    add_executable(${PROJECT_NAME}.Tests tests/FfiTests.cpp)
    set_target_properties(${PROJECT_NAME}.Tests PROPERTIES LINKER_LANGUAGE CXX)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE CONAN_PKG::gtest)
    set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -pthread -ldl")

    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE ${PROJECT_NAME}.Library)
endif()
#
#if(${LINKS_PLATFORM_BENCHMARKS})
#    add_executable(${PROJECT_NAME}.Benchmarks ${PROJECT_NAME}.Benchmarks/AllBenchmarks.cpp)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE CONAN_PKG::gtest)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE CONAN_PKG::benchmark)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE ${PROJECT_NAME}.Library)
#endif()
