using System;
using FluentAssertions;
using FsCheck;
using NSubstitute;
using Xunit;
using FsCheck.Xunit;
using Toggl.Foundation.Services;
using Toggl.Foundation.Tests.Generators;
using Toggl.Foundation.Analytics;

namespace Toggl.Foundation.Tests.Services
{
    public sealed class BackgroundServiceTests
    {
        public abstract class BackgroundServiceTest
        {
            protected readonly ITimeService TimeService;
            protected readonly IAnalyticsService AnalyticsService;

            public BackgroundServiceTest()
            {
                TimeService = Substitute.For<ITimeService>();
                AnalyticsService = Substitute.For<IAnalyticsService>();
            }
        }

        public sealed class TheConstructor
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsWhenTheArgumentIsNull(bool useTimeService, bool useAnalyticsService)
            {
                var timeService = useTimeService ? Substitute.For<ITimeService>() : null;
                var analyticsService = useAnalyticsService ? Substitute.For<IAnalyticsService>() : null;
                Action constructor = () => new BackgroundService(timeService, analyticsService);

                constructor.Should().Throw<ArgumentNullException>();
            }
        }

        public sealed class TheAppResumedFromBackgroundMethod : BackgroundServiceTest
        {
            private readonly DateTimeOffset now = new DateTimeOffset(2017, 12, 11, 0, 30, 59, TimeSpan.Zero);

            [Fact, LogIfTooSlow]
            public void DoesNotEmitAnythingWhenItHasNotEnterBackgroundFirst()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterForeground();

                emitted.Should().BeFalse();
            }

            [Fact, LogIfTooSlow]
            public void EmitsValueWhenEnteringForegroundAfterBeingInBackground()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                TimeService.CurrentDateTime.Returns(now);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterBackground();
                backgroundService.EnterForeground();

                emitted.Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotEmitAnythingWhenTheEnterForegroundIsCalledMultipleTimes()
            {
                bool emitted = false;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                TimeService.CurrentDateTime.Returns(now);
                backgroundService.EnterBackground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(1));
                backgroundService.EnterForeground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(2));
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(_ => emitted = true);

                backgroundService.EnterForeground();

                emitted.Should().BeFalse();
            }

            [Property]
            public void EmitsAValueWhenEnteringForegroundAfterBeingInBackgroundForMoreThanTheLimit(NonNegativeInt waitingTime)
            {
                TimeSpan? resumedAfter = null;
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                backgroundService
                    .AppResumedFromBackground
                    .Subscribe(timeInBackground => resumedAfter = timeInBackground);
                TimeService.CurrentDateTime.Returns(now);

                backgroundService.EnterBackground();
                TimeService.CurrentDateTime.Returns(now.AddMinutes(waitingTime.Get).AddSeconds(1));
                backgroundService.EnterForeground();

                resumedAfter.Should().NotBeNull();
                resumedAfter.Should().BeGreaterThan(TimeSpan.FromMinutes(waitingTime.Get));
            }

            [Fact]
            public void TracksEventWhenAppResumed()
            {
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                backgroundService.EnterBackground();
                backgroundService.EnterForeground();
                AnalyticsService.Received().AppDidEnterForeground.Track();
            }

            [Fact]
            public void TracksEventWhenAppGoesToBackground()
            {
                var backgroundService = new BackgroundService(TimeService, AnalyticsService);
                backgroundService.EnterBackground();
                AnalyticsService.Received().AppSentToBackground.Track();
            }
        }
    }
}
