namespace NuDoq
{
    using Autofac;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using NuDoq.Diagnostics;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class WorkerRole : RoleEntryPoint
    {
        static readonly ITracer tracer = Tracer.Get<WorkerRole>();
        IContainer container;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public override void Run()
        {
            tracer.Info("Run");

            Task task = RunAsync(tokenSource.Token);
            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                tracer.Error(ex, "Unhandled exception in worker role.");
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        public override void OnStop()
        {
            tokenSource.Cancel();
            tokenSource.Token.WaitHandle.WaitOne();
            base.OnStop();
        }

        private async Task RunAsync(CancellationToken token)
        {
            using (var scope = container.BeginLifetimeScope())
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        // await AsyncWorkFromSomethingResolvedFromScope
                        tracer.Info("Doing some processing work here.");
                    }
                    catch (Exception ex)
                    {
                        tracer.Error(ex, "Exception in worker role Run loop.");
                    }

                    await Task.Delay(1000);
                }
            }
        }
    }
}
