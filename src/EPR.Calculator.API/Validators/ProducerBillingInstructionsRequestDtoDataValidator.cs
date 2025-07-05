using System.Net;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public class ProducerBillingInstructionsRequestDtoDataValidator() : IProducerBillingInstructionsRequestDtoDataValidator
    {
        private static readonly HashSet<string> AllowedStatuses = new() { "Accepted", "Rejected" };

        public ValidationResultDto<ErrorDto> Validate(ProducerBillingInstructionsRequestDto request)
        {
            var validationResult = new ValidationResultDto<ErrorDto>();

            // PageNumber: can't be negative if provided
            if (request.PageNumber.HasValue && request.PageNumber < 0)
            {
                validationResult.IsInvalid = true;
                validationResult.StatusCode = HttpStatusCode.BadRequest;
                validationResult.Errors.Add(new ErrorDto
                {
                    Message = "PageNumber cannot be negative.",
                });
                return validationResult;
            }

            // PageSize: must be null or >= 1
            if (request.PageSize.HasValue && request.PageSize < 1)
            {
                validationResult.IsInvalid = true;
                validationResult.Errors.Add(new ErrorDto
                {
                    Message = "PageSize must be at least 1 if provided.",
                });
                return validationResult;
            }

            // SearchQuery validation
            if (request.SearchQuery != null)
            {
                var searchQuery = request.SearchQuery;

                // OrganisationId: null or > 0
                if (searchQuery.OrganisationId.HasValue && searchQuery.OrganisationId <= 0)
                {
                    validationResult.IsInvalid = true;
                    validationResult.Errors.Add(new ErrorDto
                    {
                        Message = "OrganisationId must be greater than 0 if provided.",
                    });
                    return validationResult;
                }

                // Status: can be null, must not contain duplicates, only "Accepted" or "Rejected"
                if (searchQuery.Status != null)
                {
                    var statusList = searchQuery.Status.ToList();

                    // Only allowed values
                    var invalidStatuses = statusList.Where(s => !AllowedStatuses.Contains(s)).Distinct().ToList();
                    if (invalidStatuses.Count > 0)
                    {
                        validationResult.IsInvalid = true;
                        validationResult.Errors.Add(new ErrorDto
                        {
                            Message = $"Status can only contain: {string.Join(", ", AllowedStatuses)}.",
                        });
                    }

                    // No duplicates
                    if (statusList.Count != statusList.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                    {
                        validationResult.IsInvalid = true;
                        validationResult.Errors.Add(new ErrorDto
                        {
                            Message = "Status cannot contain duplicate values.",
                        });
                    }
                }
            }

            return validationResult;
        }
    }
}