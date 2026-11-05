/* Finite reproduction: short writes and one EINTR must preserve the message. */
#define write issue524_write
#include CLOG_SOURCE_PATH
#undef write
#include <errno.h>

static char captured[128];
static size_t captured_size;
static unsigned calls;

ssize_t issue524_write(int fd, const void* bytes, size_t size)
{
    (void)fd;
    if (++calls == 2) { errno = EINTR; return -1; }
    size_t count = size < 2 ? size : 2;
    if (captured_size + count >= sizeof(captured)) return -1;
    memcpy(captured + captured_size, bytes, count);
    captured_size += count;
    return (ssize_t)count;
}

static void log_message(const char* format, ...)
{
    va_list args;
    va_start(args, format);
    clog_vlog_info("repro", format, args);
    va_end(args);
}

int main(void)
{
    log_message("hello");
    return strcmp(captured, "Note (repro): hello\n") == 0 ? 0 : 1;
}
