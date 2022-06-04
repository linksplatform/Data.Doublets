package main

/*
#cgo LDFLAGS: -ldoublets_ffi
#include "./lib/ffi.h"

uint64_t ignore(struct Link_u64 before, struct Link_u64 after);

*/
import "C"
import (
	"fmt"
	"os"
	"time"
)

/// //export ignore
/// func ignore(before C.Link_u64, after C.Link_u64) uint64 { return 0 }
///
func main() {
	db := C.UInt64Links_New(C.CString("db.links"))

	start := time.Now()

	for i := 1; i <= 1_000_000; i++ {
		//index := C.UInt64Links_Create(db, nil, 0, C.ignore)
		_ = C.UInt64Links_SmartCreate(db)
		//C.UInt64Links_SmartUpdate(db, index, index, index)
		//C.UInt64Links_Update(db, nil, 0, C.CUDCallback_u64(C.ignore))
	}

	fmt.Println(time.Since(start))
	C.UInt64Links_Drop(db)
	_ = os.Remove("db.links")
}
