using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoShoutData.Models.Publish {
    public class BroadcastField {
        public string IdToken { get; set; }

        public int FieldId { get; set; }
        public int BroadcastActionId { get; set; }
        public int? NormalizedFieldId { get; set; }
        public int TemplateActionId { get; set; }
        public int PublishActionFieldDefinitionId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ThumbnailUrl { get; set; }
        public string WebsiteAdapterIcon { get; set; }

        public string DisplayValue { get; set; }

        //Field def
        public Guid UniqueId { get; set; }
        public string Description { get; set; }
        public string UIType { get; set; }
        public bool IsRequired { get; set; }
        public int Length { get; set; }
        public bool OfferCharacterCounter { get; set; }
        public bool CanShorten { get; set; }
        public string ValidationExpression { get; set; }
        public string ValidationErrorMessage { get; set; }
        public bool HasRangeValues { get; set; }
    }
}
