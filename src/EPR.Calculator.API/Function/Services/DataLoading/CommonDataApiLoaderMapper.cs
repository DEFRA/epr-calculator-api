using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services.CommonDataApi;

namespace EPR.Calculator.Service.Function.Services.DataLoading
{
    internal static class CommonDataApiLoaderMapper
    {
        /// <summary>
        ///     Creates a mapper function to convert PomResponse API objects to PomData database entities.
        /// </summary>
        /// <param name="loadTime">The timestamp to apply to all mapped entities.</param>
        /// <returns>A mapper function that throws FormatException if SubmitterId is invalid.</returns>
        internal static Func<PomResponse, PomData> MapPom(DateTimeOffset loadTime)
        {
            return r => new PomData
            {
                SubmissionPeriod = r.SubmissionPeriod,
                SubmissionPeriodDesc = r.SubmissionPeriodDescription,
                OrganisationId = r.OrganisationId,
                SubsidiaryId = r.SubsidiaryId,
                PackagingType = r.PackagingType,
                PackagingMaterial = r.PackagingMaterial,
                PackagingMaterialSubtype = r.PackagingMaterialSubtype,
                PackagingMaterialWeight = r.PackagingMaterialWeight,
                PackagingClass = r.PackagingClass,
                PackagingActivity = r.PackagingActivity,
                RamRagRating = r.RamRagRating,
                SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
                    ? guid
                    : throw new FormatException(
                        $"Invalid {nameof(PomResponse)}.{nameof(PomResponse.SubmitterId)}: {r.SubmitterId}"),
                LoadTimeStamp = loadTime.UtcDateTime
            };
        }

        /// <summary>
        ///     Creates a mapper function to convert OrganisationResponse API objects to OrganisationData database entities.
        /// </summary>
        /// <param name="loadTime">The timestamp to apply to all mapped entities.</param>
        /// <returns>A mapper function that throws FormatException if required fields are null or invalid.</returns>
        internal static Func<OrganisationResponse, OrganisationData> MapOrganisation(DateTimeOffset loadTime)
        {
            return r => new OrganisationData
            {
                OrganisationId = r.OrganisationId ?? throw new FormatException(
                    $"Invalid {nameof(OrganisationResponse)}.{nameof(OrganisationResponse.OrganisationId)}: {r.OrganisationId}"),
                SubsidiaryId = r.SubsidiaryId,
                OrganisationName = r.OrganisationName ?? throw new FormatException(
                    $"Invalid {nameof(OrganisationResponse)}.{nameof(OrganisationResponse.OrganisationName)}: {r.OrganisationName}"),
                TradingName = r.TradingName,
                StatusCode = r.StatusCode,
                ErrorCode = r.ErrorCode,
                JoinerDate = r.JoinerDate,
                LeaverDate = r.LeaverDate,
                ObligationStatus = r.ObligationStatus ?? throw new FormatException(
                    $"Invalid {nameof(OrganisationResponse)}.{nameof(OrganisationResponse.ObligationStatus)}: {r.ObligationStatus}"),
                DaysObligated = r.NumDaysObligated,
                SubmitterId = Guid.TryParse(r.SubmitterId, out var guid)
                    ? guid
                    : throw new FormatException(
                        $"Invalid {nameof(OrganisationResponse)}.{nameof(OrganisationResponse.SubmitterId)}: {r.SubmitterId}"),
                HasH1 = r.HasH1,
                HasH2 = r.HasH2,
                LoadTimestamp = loadTime.UtcDateTime
            };
        }
    }
}