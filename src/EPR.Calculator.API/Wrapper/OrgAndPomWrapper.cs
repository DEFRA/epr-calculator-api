using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Wrapper
{
    public class OrgAndPomWrapper : IOrgAndPomWrapper
    {
        private readonly ApplicationDBContext context;
        public OrgAndPomWrapper(ApplicationDBContext context) { this.context = context; }

        public bool AnyOrganisationData()
        {
            return this.context.OrganisationData.Any();
        }

        public bool AnyPomData()
        {
            return this.context.PomData.Any();
        }

        public IEnumerable<OrganisationData> GetOrganisationData()
        {
            return this.context.OrganisationData.ToList();
        }

        public IEnumerable<PomData> GetPomData()
        {
            return this.context.PomData.ToList();
        }
    }
}
