Conan Center revision dbfaf1c016d9ed37b2d536d479cf5f0499bb361a includes cpuinfo/cci.20260612. Exporting its unmodified recipe with Conan 2.34.0 fails when risk diagnostics are fatal:

```sh
conan export recipes/cpuinfo/all --version cci.20260612 '-cc=core:warnings_as_errors=["*"]'
```

The date-based version does not have a semantic major/minor/patch identity. Conan reports the missing package_id_non_embed_mode and package_id_unknown_mode, then exits with an error. Reproduced while updating Data.Doublets#556; the source package itself builds successfully with the following recipe metadata:

```python
class CpuinfoConan(ConanFile):
    package_id_non_embed_mode = "full_mode"
    package_id_unknown_mode = "full_mode"
```

Please declare these relationships explicitly for date-versioned cpuinfo, so changes to the full upstream snapshot contribute to the consumer identity. Our temporary recipe transformation applies these two attributes; it preserves the current source SHA/checksum and keeps all diagnostics fatal. It can be deleted after this metadata reaches the upstream recipe.
