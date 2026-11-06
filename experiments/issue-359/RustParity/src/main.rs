use doublets::{
    Doublets, Error, Links,
    data::{Flow, LinksConstants},
    mem::Global,
    split, unit,
};

fn check(
    mut store: impl Doublets<u32>,
    name: &str,
    audit_two_part: bool,
) -> Result<usize, Error<u32>> {
    let any = Links::constants(&store).any;
    let a = store.create_point()?;
    let b = store.create_point()?;
    let asymmetric = store.create_link(a, b)?;
    store.create_link(b, a)?;
    let external = *Links::constants(&store)
        .external_range
        .as_ref()
        .expect("external references enabled")
        .start()
        + 20;
    store.create_link(external, a)?;
    store.create_link(b, external)?;
    store.create_link(external, external)?;
    store.create_link(0, a)?;
    store.create_link(b, 0)?;
    store.create()?;
    let deleted = store.create_point()?;
    let last = store.create_point()?;
    store.update(deleted, 0, 0)?;
    store.delete(deleted)?;
    let values = [any, 0, a, b, external, 50];
    let mut checks = 0;
    let mut differences = 0;
    for stage in 0..2 {
        if stage == 1 {
            store.update(asymmetric, b, asymmetric)?;
            assert_eq!(deleted, store.create_link(last, a)?);
        }
        let mut rows = Vec::new();
        store.each(|link| {
            rows.push(link);
            Flow::Continue
        });
        let indices = rows
            .iter()
            .map(|row| row.index)
            .chain([0, deleted, external, 50]);
        for index in indices {
            let row = rows.iter().find(|row| row.index == index);
            for source in values {
                for target in values {
                    let query = [index, source, target];
                    let matches = row.is_some_and(|row| {
                        (source == any || row.source == source)
                            && (target == any || row.target == target)
                    });
                    assert_eq!(
                        store.count_by(query),
                        u32::from(matches),
                        "{name}: {query:?}"
                    );
                    for flow in [Flow::Continue, Flow::Break] {
                        let mut visited = Vec::new();
                        let result = store.each_by(query, |link| {
                            visited.push(link);
                            flow
                        });
                        assert_eq!(
                            visited,
                            row.filter(|_| matches)
                                .cloned()
                                .into_iter()
                                .collect::<Vec<_>>(),
                            "{name}: {query:?}"
                        );
                        assert_eq!(result, if matches { flow } else { Flow::Continue });
                    }
                    checks += 1;
                }
                if audit_two_part {
                    let matches = row.is_some_and(|row| {
                        source == any || row.source == source || row.target == source
                    });
                    let actual = store.count_by([index, source]);
                    let mut calls = 0;
                    store.each_by([index, source], |_| {
                        calls += 1;
                        Flow::Continue
                    });
                    assert_eq!(calls, usize::from(matches));
                    if actual != u32::from(matches) {
                        differences += 1;
                        if differences <= 3 {
                            println!(
                                "{name}: [{index}, {source}], row={row:?}: Count={actual}, Each calls={calls}"
                            );
                        }
                    }
                }
            }
        }
    }
    println!("{name}: {checks} positional queries pass Count, Each and callback-flow checks");
    if audit_two_part {
        println!("{name}: {differences} two-part Count/Each differences");
    }
    Ok(differences)
}

fn main() -> Result<(), Error<u32>> {
    let audit_two_part = std::env::args().any(|argument| argument == "--audit-two-part");
    let united = check(
        unit::Store::<u32, _>::with_constants(Global::new(), LinksConstants::external())?,
        "united",
        audit_two_part,
    )?;
    let split = check(
        split::Store::<u32, _, _>::with_constants(
            Global::new(),
            Global::new(),
            LinksConstants::external(),
        )?,
        "split",
        audit_two_part,
    )?;
    if united + split != 0 {
        std::process::exit(1);
    }
    Ok(())
}
