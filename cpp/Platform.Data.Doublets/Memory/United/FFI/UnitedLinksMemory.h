#include <cstdint>
#include <functional>

namespace Platform::Data::Doublets::Memory::United::FFI
{
	template <typename Sig>
	thread_local std::function<Sig> GLOBAL_LAMBDA_FUNCTION = nullptr;

	template <typename ReturnType, typename ... Args>
	ReturnType call_global_lambda(Args... param) {
		ReturnType result_of_call = GLOBAL_LAMBDA_FUNCTION<ReturnType(Args...)>(param...);
		GLOBAL_LAMBDA_FUNCTION<ReturnType(Args...)> = nullptr;
		return result_of_call;
	}

	template <typename TLinkAddress>  class UnitedMemoryLinks<TLinkAddress> : ILinks<UnitedMemoryLinks, TLinkAddress, LinksConstants<TLinkAddress>> {

	private:

		static const UncheckedConverter<std::uint8_t, TLinkAddress> from_u8 = UncheckedConverter<std::uint8_t, TLinkAddress>::Default;
		static const UncheckedConverter<std::uint16_t, TLinkAddress> from_u16 = UncheckedConverter<std::uint16_t, TLinkAddress>::Default;
		static const UncheckedConverter<std::uint32_t, TLinkAddress> from_u32 = UncheckedConverter<std::uint32_t, TLinkAddress>::Default;
		static const UncheckedConverter<std::uint64_t, TLinkAddress> from_u64 = UncheckedConverter<std::uint64_t, TLinkAddress>::Default;
		static const UncheckedConverter<TLinkAddress, std::uint64_t> from_t = UncheckedConverter<TLinkAddress, std::uint64_t>::Default;

		const void* _ptr;

	public:

		UnitedMemoryLinks(const char* path) {

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				_ptr = ByteUnitedMemoryLinks_New(path);
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				_ptr = UInt16UnitedMemoryLinks_New(path);
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				_ptr = UInt32UnitedMemoryLinks_New(path);
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				_ptr = UInt64UnitedMemoryLinks_New(path);
			}
			else {
				throw new NotImplementedException();
			}
				
			Constants = new LinksConstants<TLinkAddress>(true);

		}

		TLinkAddress Count(IList<TLinkAddress>& restriction) {

			std::uintptr_t restrictionLength = (restriction != null) ? restriction.Count : 0;

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				std::uint8_t* restrictionArray = (std::uint8_t*) alloca(restrictionLength);
				IList<std::uint8_t> byteRestrictionArray = (IList<std::uint8_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}
				
				return from_u8.Convert(ByteUnitedMemoryLinks_Count(_ptr, restrictionArray, restrictionLength));
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				std::uint16_t* restrictionArray = (std::uint16_t*)alloca(restrictionLength*2);
				IList<std::uint16_t> uint16RestrictionArray = (IList<std::uint16_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = uint16RestrictionArray[i];
				}

				return from_u16.Convert(UInt16UnitedMemoryLinks_Count(_ptr, restrictionArray, restrictionLength));
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				std::uint32_t* restrictionArray = (std::uint32_t*)alloca(restrictionLength*4);
				IList<std::uint32_t> uint32RestrictionArray = (IList<std::uint32_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = uint32RestrictionArray[i];
				}

				return from_u32.Convert(UInt32UnitedMemoryLinks_Count(_ptr, restrictionArray, restrictionLength));
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				std::uint64_t* restrictionArray = (std::uint64_t*)alloca(restrictionLength*8);
				IList<std::uint64_t> byteRestrictionArray = (IList<std::uint64_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = uint64RestrictionArray[i];
				}

				return from_u64.Convert(UInt64UnitedMemoryLinks_Count(_ptr, restrictionArray, restrictionLength));
			}
			else {
				throw new NotImplementedException();
			}

		}

		TLinkAddress Each(std::function<TLinkAddress(IList<TLinkAddress>)> handler, IList<TLinkAddress>& restriction) {

			std::uintptr_t restrictionLength = (restriction != null) ? restriction.Count : 0;

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint8_t(Link<std::uint8_t>)> = [&](Link<std::uint8_t> link) -> std::uint8_t {
					return (std::uint8_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u8.Convert(link.Index), from_u8.Convert(link.Source), from_u8.Convert(link.Target))) : Constants::Continue);
				};

				EachCallback<std::uint8_t> Callback = call_global_lambda<std::uint8_t, Link<std::uint8_t>>;

				std::uint8_t* restrictionArray = (std::uint8_t*)alloca(restrictionLength);
				IList<std::uint8_t> byteRestrictionArray = (IList<std::uint8_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}
					
				return from_u8.Convert(ByteUnitedMemoryLinks_Each(_ptr, restrictionArray, restrictionLength, Callback));
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint16_t(Link<std::uint16_t>)> = [&](Link<std::uint16_t> link) -> std::uint16_t {
					return (std::uint16_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u16.Convert(link.Index), from_u16.Convert(link.Source), from_u16.Convert(link.Target))) : Constants::Continue);
				};

				EachCallback<std::uint16_t> Callback = call_global_lambda<std::uint16_t, Link<std::uint16_t>>;

				std::uint16_t* restrictionArray = (std::uint16_t*)alloca(restrictionLength * 2);
				IList<std::uint16_t> byteRestrictionArray = (IList<std::uint16_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u16.Convert(UInt16UnitedMemoryLinks_Each(_ptr, restrictionArray, restrictionLength, Callback));
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint32_t(Link<std::uint32_t>)> = [&](Link<std::uint32_t> link) -> std::uint32_t {
					return (std::uint32_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u32.Convert(link.Index), from_u32.Convert(link.Source), from_u32.Convert(link.Target))) : Constants::Continue);
				};

				EachCallback<std::uint32_t> Callback = call_global_lambda<std::uint32_t, Link<std::uint32_t>>;

				std::uint32_t* restrictionArray = (std::uint32_t*)alloca(restrictionLength * 4);
				IList<std::uint32_t> byteRestrictionArray = (IList<std::uint32_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u32.Convert(UInt32UnitedMemoryLinks_Each(_ptr, restrictionArray, restrictionLength, Callback));
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint64_t(Link<std::uint64_t>)> = [&](Link<std::uint64_t> link) -> std::uint64_t {
					return (std::uint64_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u64.Convert(link.Index), from_u64.Convert(link.Source), from_u64.Convert(link.Target))) : Constants::Continue);
				};

				EachCallback<std::uint64_t> Callback = call_global_lambda<std::uint64_t, Link<std::uint64_t>>;

				std::uint64_t* restrictionArray = (std::uint64_t*)alloca(restrictionLength * 8);
				IList<std::uint64_t> byteRestrictionArray = (IList<std::uint64_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u64.Convert(UInt64UnitedMemoryLinks_Each(_ptr, restrictionArray, restrictionLength, Callback));
			}
			else {
				throw new NotImplementedException();
			}

		}

		TLinkAddress Create(std::function<TLinkAddress(IList<TLinkAddress>, IList<TLinkAddress>)> handler, IList<TLinkAddress>& substitution) {

			std::uintptr_t restrictionLength = (restriction != null) ? restriction.Count : 0;

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint8_t(Link<std::uint8_t>, Link<std::uint8_t>)> = [&](Link<std::uint8_t> before, Link<std::uint8_t> after) -> std::uint8_t {
					return (std::uint8_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLinkAddress>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint8_t> CUDCallback = call_global_lambda<std::uint8_t, Link<std::uint8_t>, Link<std::uint8_t>>;

				std::uint8_t * restrictionArray = (std::uint8_t*)alloca(restrictionLength);
				IList<std::uint8_t> byteRestrictionArray = (IList<std::uint8_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u8.Convert(ByteUnitedMemoryLinks_Create(_ptr, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint16_t(Link<std::uint16_t>, Link<std::uint16_t>)> = [&](Link<std::uint16_t> before, Link<std::uint16_t> after) -> std::uint16_t {
					return (std::uint16_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLinkAddress>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint16_t> CUDCallback = call_global_lambda<std::uint16_t, Link<std::uint16_t>, Link<std::uint16_t>>;

				std::uint16_t * restrictionArray = (std::uint16_t*)alloca(restrictionLength * 2);
				IList<std::uint16_t> byteRestrictionArray = (IList<std::uint16_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u16.Convert(UInt16UnitedMemoryLinks_Create(_ptr, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint32_t(Link<std::uint32_t>, Link<std::uint32_t>)> = [&](Link<std::uint32_t> before, Link<std::uint32_t> after) -> std::uint32_t {
					return (std::uint32_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLinkAddress>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint32_t> CUDCallback = call_global_lambda<std::uint32_t, Link<std::uint32_t>, Link<std::uint32_t>>;


				std::uint32_t * restrictionArray = (std::uint32_t*)alloca(restrictionLength * 4);
				IList<std::uint32_t> byteRestrictionArray = (IList<std::uint32_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u32.Convert(UInt32UnitedMemoryLinks_Create(_ptr, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint64_t(Link<std::uint64_t>, Link<std::uint64_t>)> = [&](Link<std::uint64_t> before, Link<std::uint64_t> after) -> std::uint64_t {
					return (std::uint64_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLinkAddress>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint64_t> CUDCallback = call_global_lambda<std::uint64_t, Link<std::uint64_t>, Link<std::uint64_t>>;

				std::uint64_t * restrictionArray = (std::uint64_t*)alloca(restrictionLength * 8);
				IList<std::uint64_t> byteRestrictionArray = (IList<std::uint64_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u64.Convert(UInt64UnitedMemoryLinks_Create(_ptr, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else {
				throw new NotImplementedException();
			}
		}

		TLinkAddress Update(std::function<TLinkAddress(IList<TLinkAddress>, IList<TLinkAddress>)> handler, IList<TLinkAddress>& restriction, IList<TLinkAddress>& substitution) {
			std::uintptr_t restrictionLength = (restriction!=NULL) ? restriction.Count : 0;
			std::uintptr_t substitutionLength = (substitution!=NULL) ? substitution.Count : 0;

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint8_t(Link<std::uint8_t>, Link<std::uint8_t>)> = [&](Link<std::uint8_t> before, Link<std::uint8_t> after) -> std::uint8_t {
					return (std::uint8_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLinkAddress>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint8_t> CUDCallback = call_global_lambda<std::uint8_t, Link<std::uint8_t>, Link<std::uint8_t>>;
				
				std::uint8_t* restrictionArray = (std::uint8_t*)alloca(restrictionLength);
				IList<std::uint8_t> byteRestrictionArray = (IList<std::uint8_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				std::uint8_t* substitutionArray = (std::uint8_t*)alloca(substitutionLength);
				IList<std::uint8_t> byteSubstitutionArray = (IList<std::uint8_t>)substitution;

				for (std::uintptr_t i = 0; i < substitutionLength; i++)
				{
					substitutionArray[i] = byteSubstitutionArray[i];
				}

				return from_u8.Convert(ByteUnitedMemoryLinks_Update(_ptr, restrictionArray, restrictionLength, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint16_t(Link<std::uint16_t>, Link<std::uint16_t>)> = [&](Link<std::uint16_t> before, Link<std::uint16_t> after) -> std::uint16_t {
					return (std::uint16_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLinkAddress>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint16_t> CUDCallback = call_global_lambda<std::uint16_t, Link<std::uint16_t>, Link<std::uint16_t>>;

				std::uint16_t* restrictionArray = (std::uint16_t*)alloca(restrictionLength*2);
				IList<std::uint16_t> byteRestrictionArray = (IList<std::uint16_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				std::uint16_t* substitutionArray = (std::uint16_t*)alloca(substitutionLength*2);
				IList<std::uint16_t> byteSubstitutionArray = (IList<std::uint16_t>)substitution;

				for (std::uintptr_t i = 0; i < substitutionLength; i++)
				{
					substitutionArray[i] = byteSubstitutionArray[i];
				}

				return from_u16.Convert(UInt16UnitedMemoryLinks_Update(_ptr, restrictionArray, restrictionLength, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
				
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint32_t(Link<std::uint32_t>, Link<std::uint32_t>)> = [&](Link<std::uint32_t> before, Link<std::uint32_t> after) -> std::uint32_t {
					return (std::uint32_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLinkAddress>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint32_t> CUDCallback = call_global_lambda<std::uint32_t, Link<std::uint32_t>, Link<std::uint32_t>>;

				std::uint32_t* restrictionArray = (std::uint32_t*)alloca(restrictionLength * 4);
				IList<std::uint32_t> byteRestrictionArray = (IList<std::uint32_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				std::uint32_t* substitutionArray = (std::uint32_t*)alloca(substitutionLength * 4);
				IList<std::uint32_t> byteSubstitutionArray = (IList<std::uint32_t>)substitution;

				for (std::uintptr_t i = 0; i < substitutionLength; i++)
				{
					substitutionArray[i] = byteSubstitutionArray[i];
				}

				return from_u32.Convert(UInt32UnitedMemoryLinks_Update(_ptr, restrictionArray, restrictionLength, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint64_t(Link<std::uint64_t>, Link<std::uint64_t>)> = [&](Link<std::uint64_t> before, Link<std::uint64_t> after) -> std::uint64_t {
					return (std::uint64_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLinkAddress>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint64_t> CUDCallback = call_global_lambda<std::uint64_t, Link<std::uint64_t>, Link<std::uint64_t>>;

				std::uint32_t* restrictionArray = (std::uint32_t*)alloca(restrictionLength * 8);
				IList<std::uint64_t> byteRestrictionArray = (IList<std::uint64_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				std::uint64_t* substitutionArray = (std::uint64_t*)alloca(substitutionLength * 8);
				IList<std::uint64_t> byteSubstitutionArray = (IList<std::uint64_t>)substitution;

				for (std::uintptr_t i = 0; i < substitutionLength; i++)
				{
					substitutionArray[i] = byteSubstitutionArray[i];
				}

				return from_u64.Convert(UInt32UnitedMemoryLinks_Update(_ptr, restrictionArray, restrictionLength, substitutionArray, ((substitution != null) ? substitution.Count : 0), CUDCallback));
			}
			else {
				throw new NotImplementedException();
			}
		}

		TLinkAddress Delete(std::function<TLinkAddress(IList<TLinkAddress>, IList<TLinkAddress>)> handler, IList<TLinkAddress>& restriction) {
		
			std::uintptr_t restrictionLength = (restriction != null) ? restriction.Count : 0;

			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint8_t(Link<std::uint8_t>, Link<std::uint8_t>)> = [&](Link<std::uint8_t> before, Link<std::uint8_t> after) -> std::uint8_t {
					return (std::uint8_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u8.Convert(before.Index), from_u8.Convert(before.Source), from_u8.Convert(before.Target)), new Link<TLinkAddress>(from_u8.Convert(after.Index), from_u8.Convert(after.Source), from_u8.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint8_t> CUDCallback = call_global_lambda<std::uint8_t, Link<std::uint8_t>, Link<std::uint8_t>>;

				std::uint8_t* restrictionArray = (std::uint8_t*)alloca(restrictionLength);
				IList<std::uint8_t> byteRestrictionArray = (IList<std::uint8_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u8.Convert(ByteUnitedMemoryLinks_Delete(_ptr, restrictionArray, restrictionLength, CUDCallback));
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint16_t(Link<std::uint16_t>, Link<std::uint16_t>)> = [&](Link<std::uint16_t> before, Link<std::uint16_t> after) -> std::uint16_t {
					return (std::uint16_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u16.Convert(before.Index), from_u16.Convert(before.Source), from_u16.Convert(before.Target)), new Link<TLinkAddress>(from_u16.Convert(after.Index), from_u16.Convert(after.Source), from_u16.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint16_t> CUDCallback = call_global_lambda<std::uint16_t, Link<std::uint16_t>, Link<std::uint16_t>>;

				std::uint16_t* restrictionArray = (std::uint16_t*)alloca(restrictionLength * 2);
				IList<std::uint16_t> byteRestrictionArray = (IList<std::uint16_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u16.Convert(UInt16UnitedMemoryLinks_Delete(_ptr, restrictionArray, restrictionLength, CUDCallback));
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint32_t(Link<std::uint32_t>, Link<std::uint32_t>)> = [&](Link<std::uint32_t> before, Link<std::uint32_t> after) -> std::uint32_t {
					return (std::uint32_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u32.Convert(before.Index), from_u32.Convert(before.Source), from_u32.Convert(before.Target)), new Link<TLinkAddress>(from_u32.Convert(after.Index), from_u32.Convert(after.Source), from_u32.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint32_t> CUDCallback = call_global_lambda<std::uint32_t, Link<std::uint32_t>, Link<std::uint32_t>>;


				std::uint32_t* restrictionArray = (std::uint32_t*)alloca(restrictionLength * 4);
				IList<std::uint32_t> byteRestrictionArray = (IList<std::uint32_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u32.Convert(UInt32UnitedMemoryLinks_Delete(_ptr, restrictionArray, restrictionLength, CUDCallback));
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				GLOBAL_LAMBDA_FUNCTION<std::uint64_t(Link<std::uint64_t>, Link<std::uint64_t>)> = [&](Link<std::uint64_t> before, Link<std::uint64_t> after) -> std::uint64_t {
					return (std::uint64_t)from_t.Convert(handler != null ? handler(new Link<TLinkAddress>(from_u64.Convert(before.Index), from_u64.Convert(before.Source), from_u64.Convert(before.Target)), new Link<TLinkAddress>(from_u64.Convert(after.Index), from_u64.Convert(after.Source), from_u64.Convert(after.Target))) : Constants.Continue);
				};

				CUDCallback<std::uint64_t> CUDCallback = call_global_lambda<std::uint64_t, Link<std::uint64_t>, Link<std::uint64_t>>;

				std::uint64_t* restrictionArray = (std::uint64_t*)alloca(restrictionLength * 8);
				IList<std::uint64_t> byteRestrictionArray = (IList<std::uint64_t>)restriction;

				for (std::uintptr_t i = 0; i < restrictionLength; i++)
				{
					restrictionArray[i] = byteRestrictionArray[i];
				}

				return from_u64.Convert(UInt64UnitedMemoryLinks_Delete(_ptr, restrictionArray, restrictionLength, CUDCallback));
			}
			else {
				throw new NotImplementedException();
			}
		
		}

		~UnitedMemoryLinks() {
			if constexpr (std::same_as<std::uint8_t, TLinkAddress>)
			{
				ByteUnitedMemoryLinks_Drop(_ptr);
			}
			else if constexpr (std::same_as<std::uint16_t, TLinkAddress>)
			{
				UInt16UnitedMemoryLinks_Drop(_ptr);
			}
			else if constexpr (std::same_as<std::uint32_t, TLinkAddress>)
			{
				UInt32UnitedMemoryLinks_Drop(_ptr);
			}
			else if constexpr (std::same_as<std::uint64_t, TLinkAddress>)
			{
				UInt64UnitedMemoryLinks_Drop(_ptr);
			}
			else {
				throw new NotImplementedException();
			}
		}

	};
}
