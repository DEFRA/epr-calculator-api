using api.Validators;
using Azure.Core;
using EPR.Calculator.API.Commands;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.CommandHandlers
{

    public class CreateDefaultParameterCommandHandler : ICreateDefaultParameterCommandHandler
    {
        private readonly ApplicationDBContext context;
        public CreateDefaultParameterCommandHandler(ApplicationDBContext context) 
        {
            this.context = context;
        }

        public void Handle(CreateDefaultParameterCommand command)
        {
            var customValidator = new CreateDefaultParameterDataValidator(this.context);
            var validationResult = customValidator.Validate(command.SchemeParameterTemplateValues);
            if (validationResult != null && validationResult.IsInvalid)
            {
                command.IsInvalid = true;
                command.ValidationErrors = validationResult.Errors;
            }

            using (var transaction = this.context.Database.BeginTransaction())
            {
                try
                {
                    var oldDefaultSettings = this.context.DefaultParameterSettings.Where(x => x.EffectiveTo == null).ToList();
                    oldDefaultSettings.ForEach(x => { x.EffectiveTo = DateTime.Now; });

                    var defaultParamSettingMaster = new DefaultParameterSettingMaster
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = "Testuser",
                        EffectiveFrom = DateTime.Now,
                        EffectiveTo = null,
                        ParameterYear = command.ParameterYear
                    };
                    this.context.DefaultParameterSettings.Add(defaultParamSettingMaster);

                    foreach (var templateValue in command.SchemeParameterTemplateValues)
                    {
                        this.context.DefaultParameterSettingDetail.Add(new DefaultParameterSettingDetail
                        {
                            ParameterValue = decimal.Parse(templateValue.ParameterValue.TrimEnd('%').Replace("£", string.Empty)),
                            ParameterUniqueReferenceId = templateValue.ParameterUniqueReferenceId,
                            DefaultParameterSettingMaster = defaultParamSettingMaster
                        });
                    }
                    this.context.SaveChanges();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
