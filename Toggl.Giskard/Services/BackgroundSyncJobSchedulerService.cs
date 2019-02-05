using System;
using Android.App;
using Android.App.Job;
using MvvmCross;
using MvvmCross.Platforms.Android.Core;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Interactors;

namespace Toggl.Giskard.Services
{
    [Service(Exported = true, 
             Permission = "android.permission.BIND_JOB_SERVICE",
             Name = "com.toggl.giskard.BackgroundSyncJobSchedulerService")]
    public class BackgroundSyncJobSchedulerService : JobService
    {
        public const int JobId = 1;

        private IDisposable disposable;

        public override bool OnStartJob(JobParameters @params)
        {
            // Background sync is temporary disabled due to a crash that is hard to reproduce
            // Calling JobFinished and eturning early here stops the background job from running
            JobFinished(@params, false);
            return true;

            MvxAndroidSetupSingleton
                .EnsureSingletonAvailable(ApplicationContext)
                .EnsureInitialized();

            if (!Mvx.TryResolve<IInteractorFactory>(out var interactorFactory))
                return false;

            disposable = interactorFactory
                .RunBackgroundSync()
                .Execute()
                .Subscribe(_ => JobFinished(@params, false));

            return true;
        }

        public override bool OnStopJob(JobParameters @params)
        {
            Mvx.TryResolve<IAnalyticsService>(out var analyticsService);
            analyticsService?.BackgroundSyncMustStopExcecution.Track();
            return true;
        }
    }
}
