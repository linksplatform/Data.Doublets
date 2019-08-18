namespace Platform.Data.Doublets.Decorators
{
    public class LinksCascadeUsagesResolver<TLink> : LinksDecoratorBase<TLink>
    {
        public LinksCascadeUsagesResolver(ILinks<TLink> links) : base(links) { }

        public override void Delete(TLink linkIndex)
        {
            if (!Links.AreValuesReset(linkIndex))
            {
                Links.ResetValues(linkIndex);
            }
            this.DeleteAllUsages(linkIndex);
            Links.Delete(linkIndex);
        }
    }
}
