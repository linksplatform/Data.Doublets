cmake_minimum_required(VERSION 3.13)

set(LINKS_PLATFORM_TESTS OFF CACHE BOOL "Whether to compile tests")
set(LINKS_PLATFORM_EXTRA_FLAGS "" CACHE STRING "Extra compiler flags")

project(Platform.Data.Doublets CXX)
include(${CMAKE_BINARY_DIR}/conanbuildinfo.cmake)
conan_basic_setup(TARGETS)

list(APPEND LINKS_PLATFORM_EXTRA_FLAGS ${CONAN_USER_PLATFORM.HASHING_suggested_flags})

add_library(${PROJECT_NAME}.Library INTERFACE)
target_include_directories(${PROJECT_NAME}.Library INTERFACE ${PROJECT_NAME})
target_link_libraries(${PROJECT_NAME}.Library INTERFACE CONAN_PKG::platform.collections)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE CONAN_PKG::platform.threading)
target_link_libraries(${PROJECT_NAME}.Library INTERFACE CONAN_PKG::platform.memory)
target_compile_options(${PROJECT_NAME}.Library INTERFACE ${LINKS_PLATFORM_EXTRA_FLAGS})

if(${LINKS_PLATFORM_TESTS})
    add_executable(${PROJECT_NAME}.Tests ${PROJECT_NAME}.Tests/AllTests.cpp)
    set_target_properties(${PROJECT_NAME}.Tests PROPERTIES CXX_STANDARD 20)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE CONAN_PKG::gtest)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE ${PROJECT_NAME}.Library)
endif()
