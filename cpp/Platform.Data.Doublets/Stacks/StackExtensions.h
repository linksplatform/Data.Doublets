namespace Platform::Data::Doublets::Stacks
{
    class StackExtensions
    {
        public: template <typename TLinkAddress> static TLinkAddress CreateStack(ILinks<TLinkAddress> &storage, TLinkAddress stackMarker)
        {
            auto stackPoint = storage.CreatePoint();
            auto stack = storage.Update(stackPoint, stackMarker, stackPoint);
            return stack;
        }
    };
}
