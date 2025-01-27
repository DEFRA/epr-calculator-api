using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EPR.Calculator.API.Services
{
    public interface ICommandTimeoutService
    {
        void SetCommandTimeout(DatabaseFacade database, string key);
    }
}