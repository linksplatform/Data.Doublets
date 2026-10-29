#pragma once

#include <filesystem>
#include <random>
#include <stdexcept>
#include <string>
#include <system_error>
#include <utility>

namespace Platform::Data::Doublets::Tools
{
    // Atomic directory creation prevents a second process from replacing a
    // predictable file between name generation and opening the mapped file.
    class TemporaryDirectory
    {
        std::filesystem::path _path;

      public:
        TemporaryDirectory()
        {
            std::random_device random;
            for (int attempt = 0; attempt < 100; ++attempt)
            {
                auto candidate = std::filesystem::temp_directory_path() /
                                 ("doublets-" + std::to_string(random()) + "-" + std::to_string(random()));
                if (std::filesystem::create_directory(candidate))
                {
                    _path = std::move(candidate);
                    return;
                }
            }
            throw std::runtime_error("Cannot create a unique temporary directory");
        }

        TemporaryDirectory(const TemporaryDirectory&) = delete;
        TemporaryDirectory& operator=(const TemporaryDirectory&) = delete;

        ~TemporaryDirectory()
        {
            std::error_code error;
            std::filesystem::remove_all(_path, error);
        }

        std::string File(const std::string& name) const
        {
            return (_path / name).string();
        }
    };
} // namespace Platform::Data::Doublets::Tools
