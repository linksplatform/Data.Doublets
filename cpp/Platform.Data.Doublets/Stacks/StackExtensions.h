namespace Platform::Data::Doublets::Stacks
{
    class StackExtensions
    {
        public: template <typename TLink> static TLink CreateStack(ILinks<TLink> &links, TLink stackMarker)
        {
            auto stackPoint = links.CreatePoint();
            auto stack = links.Update(stackPoint, stackMarker, stackPoint);
            return stack;
        }
    };
}
