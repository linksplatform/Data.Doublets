Conan 2.34.0 rejects the current ms-gsl/5.0.1 recipe with `-cc=core:warnings_as_errors=["*"]`:

```
conan install --requires=ms-gsl/5.0.1 --build=missing -cc='core:warnings_as_errors=["*"]'
ERROR: deprecated: Usage of deprecated Conan 1.X features that will be removed in Conan 2.X
```

Without warnings-as-errors it identifies `'cpp_info.names' used in: ms-gsl/5.0.1`. The current recipe at dbfaf1c016d9ed37b2d536d479cf5f0499bb361a retains four `self.cpp_info[.components[...]].names[...]` assignments for cmake_find_package and cmake_find_package_multi, even though modern CMake properties are already set.

Please remove the legacy `names` assignments and retain `cmake_file_name` and `cmake_target_name` properties. Add an install smoke test with Conan 2 warnings-as-errors. The downstream workaround in https://github.com/linksplatform/Data.Doublets/pull/557 exports this exact recipe after deleting only those four legacy metadata assignments; the library remains at 5.0.1 and strict warning validation stays enabled. No upstream source changes are necessary.

Searched existing issues for ms-gsl and cpp_info.names/deprecated before filing; the legacy metadata addition in PR #8224 concerns old generators and does not fix this Conan 2 failure.
