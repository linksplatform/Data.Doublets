CC=gcc
CFLAGS=-Wall -Wextra -std=c99
LIBS=-ldl

# Path to the native library (adjust as needed)
LIB_PATH=../../csharp/Platform.Data.Doublets.NativeLibrary/bin/Release/net8/linux-x64/publish

test: test.c
	$(CC) $(CFLAGS) -o test test.c $(LIBS)

run: test
	LD_LIBRARY_PATH=$(LIB_PATH) ./test

clean:
	rm -f test test-native.links

copy-lib:
	cp $(LIB_PATH)/Platform.Data.Doublets.NativeLibrary.so .

.PHONY: test run clean copy-lib