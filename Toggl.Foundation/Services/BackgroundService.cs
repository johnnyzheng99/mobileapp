using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Foundation.Analytics;
using Toggl.Multivac;

namespace Toggl.Foundation.Services
{
    public sealed class BackgroundService : IBackgroundService
    {
        private readonly ITimeService timeService;
        private readonly IAnalyticsService analyticsService;

        private DateTimeOffset? lastEnteredBackground { get; set; }
        private ISubject<TimeSpan> appBecameActiveSubject { get; }

        public IObservable<TimeSpan> AppResumedFromBackground { get; }

        public BackgroundService(ITimeService timeService, IAnalyticsService analyticsService)
        {
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));

            this.timeService = timeService;
            this.analyticsService = analyticsService;

            appBecameActiveSubject = new Subject<TimeSpan>();
            lastEnteredBackground = null;

            AppResumedFromBackground = appBecameActiveSubject.AsObservable();
        }

        public void EnterBackground()
        {
            analyticsService.AppSentToBackground.Track();
            lastEnteredBackground = timeService.CurrentDateTime;
        }

        public void EnterForeground()
        {
            if (lastEnteredBackground.HasValue == false)
                return;

            var timeInBackground = timeService.CurrentDateTime - lastEnteredBackground.Value;
            lastEnteredBackground = null;
            appBecameActiveSubject.OnNext(timeInBackground);
            analyticsService.AppDidEnterForeground.Track();
        }
    }
}
