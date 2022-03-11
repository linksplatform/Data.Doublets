cmake_minimum_required(VERSION 3.13)

set(LINKS_PLATFORM_TESTS OFF CACHE BOOL "Whether to compile tests")
set(LINKS_PLATFORM_BENCHMARKS OFF CACHE BOOL "Whether to compile benchmarks")
set(LINKS_PLATFORM_EXTRA_FLAGS "" CACHE STRING "Extra compiler flags")

set(CONAN_DISABLE_CHECK_COMPILER TRUE)
set(LINKS_PLATFORM_TESTS TRUE)
set(LINKS_PLATFORM_BENCHMARKS TRUE)

project(Platform.Data.Doublets C)

add_library(${PROJECT_NAME}.Library INTERFACE)
target_include_directories(${PROJECT_NAME}.Library INTERFACE ${PROJECT_NAME})

target_link_directories(${PROJECT_NAME}.Library INTERFACE ${CMAKE_CURRENT_LIST_DIR})
target_link_libraries(${PROJECT_NAME}.Library INTERFACE doublets_ffi)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE ${CMAKE_DL_LIBS})

target_compile_options(${PROJECT_NAME}.Library INTERFACE ${LINKS_PLATFORM_EXTRA_FLAGS})

if(${LINKS_PLATFORM_TESTS})
    add_executable(${PROJECT_NAME}.Tests ${PROJECT_NAME}.Tests/FfiTest.c)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE ${PROJECT_NAME}.Library)
endif()
#
#if(${LINKS_PLATFORM_BENCHMARKS})
#    add_executable(${PROJECT_NAME}.Benchmarks ${PROJECT_NAME}.Benchmarks/AllBenchmarks.cpp)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE CONAN_PKG::gtest)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE CONAN_PKG::benchmark)
#    target_link_libraries(${PROJECT_NAME}.Benchmarks PRIVATE ${PROJECT_NAME}.Library)
#endif()
