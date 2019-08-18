using System.Collections.Generic;

namespace Platform.Data.Doublets.Decorators
{
    public class LinksNullToSelfReferenceResolver<TLink> : LinksDecoratorBase<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

        public LinksNullToSelfReferenceResolver(ILinks<TLink> links) : base(links) { }

        public override TLink Create()
        {
            var link = Links.Create();
            return Links.Update(link, link, link);
        }

        public override TLink Update(IList<TLink> restrictions)
        {
            var constants = Constants;
            var nullConstant = constants.Null;
            var index = restrictions[constants.IndexPart];
            var source = restrictions[constants.SourcePart];
            var target = restrictions[constants.TargetPart];
            source = _equalityComparer.Equals(source, nullConstant) ? index : source;
            target = _equalityComparer.Equals(target, nullConstant) ? index : target;
            return Links.Update(new Link<TLink>(index, source, target));
        }
    }
}
