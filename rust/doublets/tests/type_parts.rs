use doublets::{parts, split, unit, Doublets};
use mem::GlobalMem;

type LinkType = usize;
type Mem = GlobalMem<parts::LinkPart<LinkType>>;
type UnitStore = unit::Store<LinkType, Mem>;

#[test]
fn unit_type_parts() {
    let mut store = UnitStore::new(GlobalMem::new()).unwrap();
    let _ = store.create();
}

type DataMem = GlobalMem<parts::DataPart<LinkType>>;
type IndexMem = GlobalMem<parts::IndexPart<LinkType>>;
type SplitStore = split::Store<LinkType, DataMem, IndexMem>;

#[test]
fn split_type_parts() {
    let mut store = SplitStore::new(GlobalMem::new(), GlobalMem::new()).unwrap();
    let _ = store.create();
}
