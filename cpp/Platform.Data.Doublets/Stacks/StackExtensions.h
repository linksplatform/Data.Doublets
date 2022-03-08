namespace Platform::Data::Doublets::Stacks
{
    class StackExtensions
    {
        public: template <typename TLink> static TLink CreateStack(ILinks<TLink> &storage, TLink stackMarker)
        {
            auto stackPoint = storage.CreatePoint();
            auto stack = storage.Update(stackPoint, stackMarker, stackPoint);
            return stack;
        }
    };
}
