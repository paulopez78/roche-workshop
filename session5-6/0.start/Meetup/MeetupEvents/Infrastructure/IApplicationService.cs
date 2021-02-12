using System.Threading.Tasks;
using MeetupEvents.Application;

namespace MeetupEvents.Infrastructure
{
    public interface IApplicationService
    {
        Task<CommandResult> Handle(object command);
    }
}