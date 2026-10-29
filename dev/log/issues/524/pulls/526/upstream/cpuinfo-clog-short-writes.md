The five POSIX logging paths discard `write(2)` results. This produces `-Wunused-result` warnings with fortified GCC builds and can silently truncate diagnostics after a short write or EINTR. The same unchecked calls remain in current `main` (`deps/clog/src/clog.c`, lines 117, 195, 273, 351, 429, checked 2026-10-29).

Reproduction from a cpuinfo checkout (Linux, GCC):

```sh
cc -std=c11 -O2 -D_FORTIFY_SOURCE=2 -Werror=unused-result -Ideps/clog/include -c deps/clog/src/clog.c
```

This fails at all five unchecked writes. An automated finite runtime reproduction, which replaces `write` with a sink returning two bytes per call and EINTR on its second call, is preserved in https://github.com/linksplatform/Data.Doublets/blob/issue-524-b1512fd1c7a8/experiments/clog_partial_write.c . Compile it with `-DCLOG_SOURCE_PATH='"/absolute/cpuinfo/deps/clog/src/clog.c"'` and `-I/absolute/cpuinfo/deps/clog/include`. It exits 1 with the current implementation: only the first two bytes of `Note (repro): hello\n` reach the sink.

Suggested fix: share a POSIX helper across all five paths. Consume each successful short write, retry EINTR, and stop on zero or a permanent error so logging does not loop on a broken sink:

```c
#include <errno.h>
static void clog_write_all(int fd, const char* bytes, size_t size) {
    while (size != 0) {
        ssize_t count = write(fd, bytes, size);
        if (count < 0 && errno == EINTR) continue;
        if (count <= 0) break;
        bytes += count;
        size -= (size_t)count;
    }
}
```

A bounded Conan source workaround applying this helper is in https://github.com/linksplatform/Data.Doublets/blob/issue-524-b1512fd1c7a8/scripts/patch_conan_recipes.py . The same runtime probe exits 0 with the patch, and the fortified `-Werror=unused-result` compilation succeeds. The helper must remain guarded for POSIX platforms and preserve the Windows/Android/Hexagon paths. Temporarily disabling the affected logging level avoids the write path but loses diagnostics; disabling the warning alone does not repair truncation.

Found while auditing https://github.com/linksplatform/Data.Doublets/issues/524 .
