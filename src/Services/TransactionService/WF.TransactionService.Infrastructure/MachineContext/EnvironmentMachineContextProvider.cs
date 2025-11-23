using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using WF.TransactionService.Application.Abstractions;

namespace WF.TransactionService.Infrastructure.MachineContext;

public class EnvironmentMachineContextProvider(
    IHostEnvironment env,
    IConfiguration config) : IMachineContextProvider
{
    public int GetMachineId()
    {
        if (env.IsDevelopment())
        {
            return 0;
        }

        var val = config["MACHINE_ID"];
        if (int.TryParse(val, out int machineId))
        {
            return machineId;
        }

        return 0;
    }
}

