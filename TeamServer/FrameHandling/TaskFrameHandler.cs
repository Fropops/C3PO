using System.Threading.Tasks;
using Common.APIModels;
using Shared;

namespace TeamServer.FrameHandling;

public class TaskFrameHandler : FrameHandler
{
    public override NetFrameType FrameType { get => NetFrameType.TaskResult; }
    public override async Task ProcessFrame(NetFrame frame, string relay)
    {
        var taskOutput = await this.ExtractFrameData<AgentTaskResult>(frame);
        this.Server.TaskResultService.AddTaskResult(taskOutput);
        this.Server.ChangeTrackingService.TrackChange(ChangingElement.Result, taskOutput.Id);
    }
}