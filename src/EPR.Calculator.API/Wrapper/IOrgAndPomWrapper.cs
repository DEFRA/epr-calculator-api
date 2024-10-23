using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Wrapper
{
    public interface IOrgAndPomWrapper
    {
        bool AnyOrganisationData();

        bool AnyPomData();

        IEnumerable<OrganisationData> GetOrganisationData();

        IEnumerable<PomData> GetPomData();
    }
}
