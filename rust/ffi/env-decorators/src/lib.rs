use proc_macro::TokenStream;
use quote::quote;
use syn::{
    parse::{Parse, ParseStream},
    Expr, LitStr, Token, Type,
};

struct EnvInput {
    env: String,
    expr: Expr,
}

impl Parse for EnvInput {
    fn parse(input: ParseStream) -> syn::Result<Self> {
        let env = input.parse::<LitStr>()?.value();
        input.parse::<Token![,]>()?;
        let expr = input.parse::<Expr>()?;
        Ok(EnvInput { env, expr })
    }
}

#[proc_macro]
pub fn env_value(item: TokenStream) -> TokenStream {
    let EnvInput { env, expr } = syn::parse_macro_input!(item as EnvInput);
    let mut expr = quote! { #expr };
    std::env::var(env)
        .unwrap_or_default()
        .split_whitespace()
        .rev()
        .for_each(|name| {
            let ty_name: proc_macro2::TokenStream = name.parse().unwrap();
            expr = quote! {
                #ty_name :: new(#expr)
            }
        });
    TokenStream::from(expr)
}

struct EnvType {
    env: String,
    pat: String,
    default: Type,
}

impl Parse for EnvType {
    fn parse(input: ParseStream) -> syn::Result<Self> {
        let env = input.parse::<LitStr>()?.value();
        input.parse::<Token![,]>()?;
        let pat = input.parse::<LitStr>()?.value();
        input.parse::<Token![,]>()?;
        let ty = input.parse::<Type>()?;
        Ok(EnvType {
            env,
            pat,
            default: ty,
        })
    }
}

#[proc_macro]
pub fn env_type(item: TokenStream) -> TokenStream {
    let EnvType { env, pat, default } = syn::parse_macro_input!(item as EnvType);
    let mut ty = quote! { #default };
    std::env::var(env)
        .unwrap_or_default()
        .split_whitespace()
        .rev()
        .for_each(|name| {
            let new_pat = pat.replace('*', &ty.to_string());
            println!("{}", new_pat);
            let pat_ty: proc_macro2::TokenStream = new_pat.parse().unwrap();
            let ty_name: proc_macro2::TokenStream = name.parse().unwrap();
            ty = quote! {
                #ty_name #pat_ty
            }
        });
    TokenStream::from(ty)
}
