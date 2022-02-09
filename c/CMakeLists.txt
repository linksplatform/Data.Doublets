cmake_minimum_required(VERSION 3.13)

set(LINKS_PLATFORM_TESTS OFF CACHE BOOL "Whether to compile tests")
set(LINKS_PLATFORM_EXTRA_FLAGS "" CACHE STRING "Extra compiler flags")

set(CONAN_DISABLE_CHECK_COMPILER TRUE)
set(LINKS_PLATFORM_TESTS TRUE)

project(Platform.Data.Doublets C)

list(APPEND LINKS_PLATFORM_EXTRA_FLAGS ${CONAN_USER_PLATFORM.HASHING_suggested_flags})

add_library(${PROJECT_NAME}.Library INTERFACE)
target_include_directories(${PROJECT_NAME}.Library INTERFACE ${PROJECT_NAME})
message(PROJECT NAME: ${PROJECT_NAME})
target_compile_options(${PROJECT_NAME}.Library INTERFACE ${LINKS_PLATFORM_EXTRA_FLAGS})

if(${LINKS_PLATFORM_TESTS})
    add_executable(${PROJECT_NAME}.Tests ${PROJECT_NAME}.Tests/FfiTest.c)
    target_link_libraries(${PROJECT_NAME}.Tests PRIVATE ${PROJECT_NAME}.Library)
endif()
