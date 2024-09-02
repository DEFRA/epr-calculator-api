using EPR.Calculator.API.Commands;

namespace EPR.Calculator.API.CommandHandlers
{
    public interface ICreateDefaultParameterCommandHandler
    {
        void Handle(CreateDefaultParameterCommand command);
    }
}
