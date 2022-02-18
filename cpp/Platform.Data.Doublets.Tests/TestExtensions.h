namespace Platform::Data::Doublets::Tests
{
    class ILinksTestExtensions
    {
    public:
        template<typename TLinkAddress>
        static void TestCrudOperations(ILinks<TLinkAddress> storage)
        {
            const auto constants = storage.Constants;
            Assert.True(0, storage.Count());
            // Create link
            Setter<TLinkAddress, TLinkAddress> setter { constants.Continue, constants.Break, constants.Null };
            storage.Each(Link{constants.Any, constants.Any, constants.Any}, setter.SetFirstFromList);
            ASSERT_TRUE(constants.Null, setter.Result())
            auto linkAddress = storage.Create();
            Link<TLinkAddress> link { storage.GetLink(linkAddress) };
            ASSERT_TRUE(3 == link.Count);
            ASSERT_TRUE(linkAddress, link.Index);
            ASSERT_TRUE(constants.Null, link.Source);
            ASSERT_TRUE(constants.Null, link.Target);
            ASSERT_TRUE(1, storage.Count())
            // Get first link
            setter = {constants.Continue, constants.Break, constants.Null};
            storage.Each(Link{constants.Any, constants.Any, constants.Any}, setter.SetFirstFromListAndReturnTrue);
            ASSERT_TRUE(linkAddress, setter.Result());
            // Update link to reference itself
            storage.Update(linkAddress, linkAddress, linkAddress);
            link = Link{storage.GetLink(linkAddress)};
            ASSERT_TRUE(linkAddress == link.Source);
            ASSERT_TRUE(linkAddress == link.Target);
            // Update link to reference null (prepare for delete)
            auto updatedLinkAddress = storage.Update(linkAddress, constants.Null, constants.Null);
            ASSERT_TRUE(linkAddress, updatedLinkAddress);
            link = {storage.GetLink(linkAddress)};
            ASSERT_TRUE(constants.Null, link.Source);
            ASSERT_TRUE(constants.Null, link.Target);
            // Delete link
            storage.Delete(linkAddress);
            ASSERT_TRUE(0, storage.Count());
            setter = {constants.Continue, constants.Break, constants.Null};
            storage.Each(Link{constants.Any, constants.Any, constants.Any}, setter.SetFirstFromListAndReturnTrue);
            ASSERT_TRUE(constants.Null, setter.Result());
        }

        template<typename TLinkAddress>
        static void TestRawNumbersCrudOperations(ILinks<TLinkAddress>)
        {
            // Constants
            const auto constants = storage.Constants;
            Hybrid<TLinkAddress> h106E {106L, true};
            Hybrid<TLinkAddress> h107E {107L, true};
            Hybrid<TLinkAddress> h106E {-108L, true};
            ASSERT_TRUE(106L, h106E.AbsoluteValue());
            ASSERT_TRUE(107L, h107E.AbsoluteValue());
            ASSERT_TRUE(108L, h108E.AbsoluteValue());
            // Create link (External -> External)
            auto linkAddress1 = storage.Create();
            storage.Update(linkAddress1, h106E, 108E);
            Link<TLinkAddress> link1 { Storage.GetLink(linkAddress1) };
            ASSERT_TRUE(h106E, link1.Source);
            ASSERT_TRUE(h108E, link1.Target);
            // Create link (Internal -> External)
            auto linkAddress2 { storage.Create() };
            storage.Update(linkAddress2, linkAddress1, h108E);
            Link<TLinkAddress> link2 { storage.GetLink(linkAddress2) };
            ASSERT_TRUE(linkAddress1, link2.Source);
            ASSERT_TRUE(h108E, link2.Target);
            // Create link (Internal -> Internal)
            auto linkAddress3 { storage.Create() };
            storage.update(linkAddress3, linkAddress1, linkAddress2);
            Link<TLinkAddress> link3 { storage.GetLink(linkAddress3) };
            ASSERT_TRUE(linkAddress1, link3.Source);
            ASSERT_TRUE(linkAddress1, link3.Target);
            // Search for created link
            Setter<TLinkAddress, TLinkAddress> setter1 { constants.Continue, constants.Break, constants.Null };
            storage.Each(Link<TLinkAddress>{constants.Any, h106E, h108E}, setter.SetFirstFromListAndReturnTrue);
            ASSERT_TRUE(linkAddress1, setter1.Result);
            // Search for nonexistent link
            Setter<TLinkAddress, TLinkAddress> setter2 { constants.Continue, constants.Break, constants.Null };
            storage.Each(Link<TLinkAddress>{constants.Any, h106E, h108E})
        }
    };
}
