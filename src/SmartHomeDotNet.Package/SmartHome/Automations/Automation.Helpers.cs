using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartHomeDotNet.Logging;
using SmartHomeDotNet.SmartHome.Devices;
using SmartHomeDotNet.SmartHome.Scenes;
using SmartHomeDotNet.Utils;

namespace SmartHomeDotNet.SmartHome.Automations
{
	partial class Automation
	{
		//protected IDisposable When<TDevice>(HomeDevice<TDevice> device, Predicate<TDevice> predicate, Scene sceneToStart)
		//	=> device.When(predicate).Do(_ => sceneToStart.Start()).Subscribe(this);

		//protected IDisposable When<TDevice>(HomeDevice<TDevice> device, Predicate<TDevice> predicate, Func<TDevice, Task> execute, ConcurrentExecutionMode mode = ConcurrentExecutionMode.AbortPrevious)
		//{
		//	return device.When(predicate).DoAsync(SafeExecute, mode, Scheduler).Subscribe(this);

		//	async Task SafeExecute(CancellationToken ct, TDevice d)
		//	{
		//		try
		//		{
		//			using (new AsyncContext(ct, Scheduler))
		//			{
		//				await execute(d);
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			this.Log().Error("Execution failed", e);
		//		}
		//	}
		//} 
	}
}