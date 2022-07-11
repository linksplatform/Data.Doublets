use doublets::{
    data::{
        Flow::{Break, Continue},
        ToQuery,
    },
    mem::GlobalMem,
    num::LinkType,
    split, Doublets, Error as LinksError, Link, Links,
};
use num_traits::zero;
use std::{error::Error, time::Instant};

fn write_seq<T: LinkType>(store: &mut impl Doublets<T>, seq: &[T]) -> Result<T, LinksError<T>> {
    let mut aliases = vec![store.create()?];

    for id in seq {
        let link = store.create()?;
        aliases.push(store.update(link, link, *id)?)
    }

    for (i, cur) in aliases.iter().enumerate() {
        if let Some(next) = aliases.get(i + 1) {
            store.create_link(*cur, *next)?;
        }
    }
    Ok(*aliases.first().unwrap_or(&zero()))
}

fn custom_single<T: LinkType>(store: &impl Doublets<T>, query: impl ToQuery<T>) -> Option<Link<T>> {
    // todo:
    //  store.each_iter(query).filter(Link::is_partial);

    let mut single = None;
    store.each_by(query, |link| {
        if single.is_none() && link.index != link.source {
            single = Some(link);
            Continue
        } else if link.index != link.source {
            single = None;
            Break
        } else {
            Continue
        }
    });
    single
}

fn read_seq<T: LinkType>(store: &impl Doublets<T>, root: T) -> Result<Vec<T>, LinksError<T>> {
    let any = store.constants().any;
    let mut seq = vec![];
    let mut cur = root;
    while let Some(link) = custom_single(store, [any, cur, any]) {
        cur = link.target;
        let alias = store.try_get_link(link.target)?;
        seq.push(alias.target);
    }
    Ok(seq)
}

const TEXT: &str = r#"
    Bei Nacht im Dorf der Wächter rief:
    Elfe!
    Ein ganz kleines Elfchen im Walde schlief,
    Wohl um die Elfe;
    Und meint, es rief ihm aus dem Thal
    Bei seinem Namen die Nachtigall,
    Oder Silpelit[8] hätt' ihn gerufen.
    Reibt sich der Elf' die Augen aus,
    Begibt sich vor sein Schneckenhaus
    Und ist als wie ein trunken Mann,
    Sein Schläflein war nicht voll gethan,
    Und humpelt also tippe tapp
    Durch's Haselholz in's Thal hinab,
    Schlupft an der Weinbergmauer hin,
    Daran viel Feuerwürmchen glühn:
    "Was sind das helle Fensterlein?
    Da drin wird eine Hochzeit seyn;
    Die Kleinen sitzen beim Mahle,
    Und treiben's in dem Saale.
    Da guck' ich wohl ein wenig 'nein!"
    — Pfui, stößt den Kopf an harten Stein!
    Elfe, gelt, du hast genug?
    Gukuk! Gukuk!
"#;

fn str_as_vec(str: &str) -> Vec<usize> {
    str.chars().map(|c| c as usize).collect()
}

const N: usize = 10000;

#[test]
fn seq() -> Result<(), Box<dyn Error>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    let seq = str_as_vec(TEXT);
    let instant = Instant::now();

    let sequences: Vec<_> = (0..N)
        .map(|_| write_seq(&mut store, seq.as_slice()).unwrap())
        .collect();

    for seq in sequences {
        read_seq(&store, seq).unwrap();
    }

    println!("{:?}", instant.elapsed());

    Ok(())
}

#[test]
fn bug() -> Result<(), Box<dyn Error>> {
    let mut store = split::Store::<usize, _, _>::new(GlobalMem::new(), GlobalMem::new())?;

    let a = store.create_point()?;
    let b = store.create_point()?;
    let _c = store.create_link(a, b)?;

    let _d = store.create_link(1, 10)?;
    let _d = store.create_link(10, 1)?;

    store.delete(a)?;

    let any = store.constants().any;

    for link in store.each_iter([any, any, 1]) {
        println!("{:?}", link);
    }

    Ok(())
}
