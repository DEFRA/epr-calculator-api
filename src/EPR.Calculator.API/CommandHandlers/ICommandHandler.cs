using EPR.Calculator.API.Commands;

namespace EPR.Calculator.API.CommandHandlers
{
    public interface ICommandHandler<TCommand>
    {
        void Handle(TCommand command);
    }
}
