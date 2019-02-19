using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Span;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Diagnostics;
using Toggl.Foundation.Extensions;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.Extensions;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.Services;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Multivac.Extensions.Reactive;
using Toggl.PrimeRadiant.Settings;
using static Toggl.Foundation.Helper.Constants;
using static Toggl.Multivac.Extensions.CommonFunctions;
using IStopwatch = Toggl.Foundation.Diagnostics.IStopwatch;
using IStopwatchProvider = Toggl.Foundation.Diagnostics.IStopwatchProvider;

[assembly: MvxNavigation(typeof(StartTimeEntryViewModel), ApplicationUrls.StartTimeEntry)]
namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class StartTimeEntryViewModel : MvxViewModel<StartTimeEntryParameters>
    {
        //Fields
        private readonly ITimeService timeService;
        private readonly ITogglDataSource dataSource;
        private readonly IDialogService dialogService;
        private readonly IUserPreferences userPreferences;
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;
        private readonly IAnalyticsService analyticsService;
        private readonly ISchedulerProvider schedulerProvider;
        private readonly IIntentDonationService intentDonationService;
        private readonly IStopwatchProvider stopwatchProvider;

        private readonly CompositeDisposable disposeBag = new CompositeDisposable();
        private readonly ISubject<TextFieldInfo> uiSubject = new ReplaySubject<TextFieldInfo>();
        private readonly ISubject<TextFieldInfo> querySubject = new Subject<TextFieldInfo>();
        private readonly ISubject<AutocompleteSuggestionType> queryByTypeSubject = new Subject<AutocompleteSuggestionType>();
        private readonly ISubject<TimeSpan> displayedTime = new BehaviorSubject<TimeSpan>(TimeSpan.Zero);

        private bool isDirty => !string.IsNullOrEmpty(textFieldInfo.Description)
                                || textFieldInfo.Spans.Any(s => s is ProjectSpan || s is TagSpan)
                                || IsBillable
                                || startTime != parameter.StartTime
                                || duration != parameter.Duration;

        private bool hasAnyTags;
        private bool hasAnyProjects;
        private bool canCreateProjectsInWorkspace;
        private IThreadSafeWorkspace defaultWorkspace;
        private StartTimeEntryParameters parameter;
        private StartTimeEntryParameters initialParameters;

        private TextFieldInfo textFieldInfo = TextFieldInfo.Empty(0);
        private DateTimeOffset startTime;
        private TimeSpan? duration;

        private IStopwatch startTimeEntryStopwatch;
        private Dictionary<string, IStopwatch> suggestionsLoadingStopwatches = new Dictionary<string, IStopwatch>();
        private IStopwatch suggestionsRenderingStopwatch;

        private bool isRunning => !duration.HasValue;

        private string currentQuery;

        //Properties
        public IObservable<TextFieldInfo> TextFieldInfoObservable { get; }
        public bool IsBillable { get; private set; } = false;
        public bool IsSuggestingTags { get; private set; }
        public bool IsSuggestingProjects { get; private set; }

        public DurationFormat DisplayedTimeFormat { get; } = DurationFormat.Improved;

        public bool IsBillableAvailable { get; private set; } = false;

        public string PlaceholderText { get; private set; }

        public ITogglDataSource DataSource => dataSource;

        public IMvxAsyncCommand SetStartDateCommand { get; }

        public IMvxAsyncCommand ChangeTimeCommand { get; }

        public IMvxCommand ToggleBillableCommand { get; }

        public IMvxCommand DurationTapped { get; }

        public IMvxCommand ToggleTagSuggestionsCommand { get; }

        public IMvxCommand ToggleProjectSuggestionsCommand { get; }

        public IMvxCommand<ProjectSuggestion> ToggleTaskSuggestionsCommand { get; }

        public IOnboardingStorage OnboardingStorage { get; }




        private BehaviorSubject<HashSet<ProjectSuggestion>> expandedProjects =
            new BehaviorSubject<HashSet<ProjectSuggestion>>(new HashSet<ProjectSuggestion>());

        public UIAction Back { get; }
        public UIAction Done { get; }
        public InputAction<AutocompleteSuggestion> SelectSuggestion { get; }
        public InputAction<TimeSpan> SetRunningTime { get; }

        public IObservable<IEnumerable<CollectionSection<string, AutocompleteSuggestion>>>
            Suggestions { get; }
        public IObservable<string> DisplayedTime { get; }

        public StartTimeEntryViewModel(
            ITimeService timeService,
            ITogglDataSource dataSource,
            IDialogService dialogService,
            IUserPreferences userPreferences,
            IOnboardingStorage onboardingStorage,
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            IAnalyticsService analyticsService,
            IAutocompleteProvider autocompleteProvider,
            ISchedulerProvider schedulerProvider,
            IIntentDonationService intentDonationService,
            IStopwatchProvider stopwatchProvider,
            IRxActionFactory rxActionFactory
        )
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));
            Ensure.Argument.IsNotNull(dialogService, nameof(dialogService));
            Ensure.Argument.IsNotNull(userPreferences, nameof(userPreferences));
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(onboardingStorage, nameof(onboardingStorage));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(analyticsService, nameof(analyticsService));
            Ensure.Argument.IsNotNull(autocompleteProvider, nameof(autocompleteProvider));
            Ensure.Argument.IsNotNull(schedulerProvider, nameof(schedulerProvider));
            Ensure.Argument.IsNotNull(intentDonationService, nameof(intentDonationService));
            Ensure.Argument.IsNotNull(stopwatchProvider, nameof(stopwatchProvider));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.dataSource = dataSource;
            this.timeService = timeService;
            this.dialogService = dialogService;
            this.userPreferences = userPreferences;
            this.navigationService = navigationService;
            this.interactorFactory = interactorFactory;
            this.analyticsService = analyticsService;
            this.schedulerProvider = schedulerProvider;
            this.intentDonationService = intentDonationService;
            this.stopwatchProvider = stopwatchProvider;

            OnboardingStorage = onboardingStorage;

            TextFieldInfoObservable = uiSubject.AsDriver(schedulerProvider);
            DisplayedTime = displayedTime
                .Select(time => time.ToFormattedString(DurationFormat.Improved))
                .AsDriver(schedulerProvider);

            Back = rxActionFactory.FromAsync(Close);
            Done = rxActionFactory.FromObservable(done);
            SelectSuggestion = rxActionFactory.FromAsync<AutocompleteSuggestion>(selectSuggestion);
            SetRunningTime = rxActionFactory.FromAction<TimeSpan>(setRunningTime);

            DurationTapped = new MvxCommand(durationTapped);
            ChangeTimeCommand = new MvxAsyncCommand(changeTime);
            ToggleBillableCommand = new MvxCommand(toggleBillable);
            SetStartDateCommand = new MvxAsyncCommand(setStartDate);
            ToggleTagSuggestionsCommand = new MvxCommand(toggleTagSuggestions);
            ToggleProjectSuggestionsCommand = new MvxCommand(toggleProjectSuggestions);

            ToggleTaskSuggestionsCommand = new MvxCommand<ProjectSuggestion>(toggleTaskSuggestions);

            var queryByType = queryByTypeSubject
                .AsObservable()
                .SelectMany(type => autocompleteProvider.Query(new QueryInfo("", type)));

            var queryByText = querySubject
                .AsObservable()
                .StartWith(textFieldInfo)
                .Select(QueryInfo.ParseFieldInfo)
                .Do(onParsedQuery)
                .ObserveOn(schedulerProvider.BackgroundScheduler)
                .SelectMany(autocompleteProvider.Query);

            Suggestions = Observable.Merge(queryByText, queryByType)
                .Select(items => items.ToList()) // This is line is needed for now to read objects from realm
                .Select(filter)
                .Select(addStaticElements)
                .Select(group)
                .CombineLatest(expandedProjects, (groups, _) => groups)
                .Select(toCollections)
                .AsDriver(schedulerProvider);
        }

        public void Init()
        {
            var now = timeService.CurrentDateTime;
            var startTimeEntryParameters = userPreferences.IsManualModeEnabled
                ? StartTimeEntryParameters.ForManualMode(now)
                : StartTimeEntryParameters.ForTimerMode(now);
            Prepare(startTimeEntryParameters);
        }

        public override void Prepare(StartTimeEntryParameters parameter)
        {
            this.parameter = parameter;
            startTime = parameter.StartTime;
            duration = parameter.Duration;

            PlaceholderText = parameter.PlaceholderText;
            if (!string.IsNullOrEmpty(parameter.EntryDescription))
            {
                initialParameters = parameter;
            }

            timeService.CurrentDateTimeObservable
                .Where(_ => isRunning)
                .Subscribe(currentTime => displayedTime.OnNext(currentTime - startTime))
                .DisposedBy(disposeBag);
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            startTimeEntryStopwatch = stopwatchProvider.Get(MeasuredOperation.OpenStartView);
            stopwatchProvider.Remove(MeasuredOperation.OpenStartView);

            defaultWorkspace = await interactorFactory.GetDefaultWorkspace()
                .TrackException<InvalidOperationException, IThreadSafeWorkspace>("StartTimeEntryViewModel.Initialize")
                .Execute();

            canCreateProjectsInWorkspace =
                await interactorFactory.GetAllWorkspaces().Execute().Select(allWorkspaces =>
                    allWorkspaces.Any(ws => ws.IsEligibleForProjectCreation()));

            textFieldInfo = TextFieldInfo.Empty(parameter?.WorkspaceId ?? defaultWorkspace.Id);

            if (initialParameters != null)
            {
                var spans = new List<ISpan>();
                spans.Add(new TextSpan(initialParameters.EntryDescription));
                if (initialParameters.ProjectId != null) {
                    try
                    {
                        var project = await interactorFactory.GetProjectById((long)initialParameters.ProjectId).Execute();
                        spans.Add(new ProjectSpan(project));
                    }
                    catch
                    {
                        // Intentionally left blank
                    }
                }
                if (initialParameters.TagIds != null) {
                    try
                    {
                        var tags = initialParameters.TagIds.ToObservable()
                            .SelectMany<long, IThreadSafeTag>(tagId => interactorFactory.GetTagById(tagId).Execute())
                            .ToEnumerable();
                        spans.AddRange(tags.Select(tag => new TagSpan(tag)));
                    }
                    catch
                    {
                        // Intentionally left blank
                    }
                }

                textFieldInfo = textFieldInfo.ReplaceSpans(spans.ToImmutableList());
            }

            await setBillableValues(textFieldInfo.ProjectId);
            uiSubject.OnNext(textFieldInfo);

            hasAnyTags = (await dataSource.Tags.GetAll()).Any();
            hasAnyProjects = (await dataSource.Projects.GetAll()).Any();
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            startTimeEntryStopwatch?.Stop();
            startTimeEntryStopwatch = null;
        }

        public override void ViewDestroy(bool viewFinishing)
        {
            base.ViewDestroy(viewFinishing);
            disposeBag?.Dispose();
        }

        public void StopSuggestionsRenderingStopwatch()
        {
            suggestionsRenderingStopwatch?.Stop();
            suggestionsRenderingStopwatch = null;
        }

        public async Task OnTextFieldInfoFromView(IImmutableList<ISpan> spans)
        {
            queryWith(textFieldInfo.ReplaceSpans(spans));
            await setBillableValues(textFieldInfo.ProjectId);
        }

        public async Task<bool> Close()
        {
            if (isDirty)
            {
                var shouldDiscard = await dialogService.ConfirmDestructiveAction(ActionType.DiscardNewTimeEntry);
                if (!shouldDiscard)
                    return false;
            }

            await navigationService.Close(this);
            return true;
        }

        private void setRunningTime(TimeSpan runningTime)
        {
            if (isRunning)
            {
                startTime = timeService.CurrentDateTime - runningTime;
            }
            else
            {
                duration = runningTime;
            }

            displayedTime.OnNext(runningTime);
        }

        private async Task selectSuggestion(AutocompleteSuggestion suggestion)
        {
            switch (suggestion)
            {
                case QuerySymbolSuggestion querySymbolSuggestion:

                    if (querySymbolSuggestion.Symbol == QuerySymbols.ProjectsString)
                    {
                        analyticsService.StartViewTapped.Track(StartViewTapSource.PickEmptyStateProjectSuggestion);
                        analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.TableCellButton);
                    }
                    else if (querySymbolSuggestion.Symbol == QuerySymbols.TagsString)
                    {
                        analyticsService.StartViewTapped.Track(StartViewTapSource.PickEmptyStateTagSuggestion);
                        analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.TableCellButton);
                    }

                    queryAndUpdateUiWith(textFieldInfo.FromQuerySymbolSuggestion(querySymbolSuggestion));
                    break;

                case TimeEntrySuggestion timeEntrySuggestion:
                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTimeEntrySuggestion);
                    updateUiWith(textFieldInfo.FromTimeEntrySuggestion(timeEntrySuggestion));
                    await setBillableValues(timeEntrySuggestion.ProjectId);
                    break;

                case ProjectSuggestion projectSuggestion:
                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickProjectSuggestion);

                    if (textFieldInfo.WorkspaceId != projectSuggestion.WorkspaceId
                        && await workspaceChangeDenied())
                        return;

                    IsSuggestingProjects = false;
                    updateUiWith(textFieldInfo.FromProjectSuggestion(projectSuggestion));
                    await setBillableValues(projectSuggestion.ProjectId);
                    queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);

                    break;

                case TaskSuggestion taskSuggestion:
                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTaskSuggestion);

                    if (textFieldInfo.WorkspaceId != taskSuggestion.WorkspaceId
                        && await workspaceChangeDenied())
                        return;

                    IsSuggestingProjects = false;
                    updateUiWith(textFieldInfo.FromTaskSuggestion(taskSuggestion));
                    await setBillableValues(taskSuggestion.ProjectId);
                    queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);
                    break;

                case TagSuggestion tagSuggestion:
                    analyticsService.StartViewTapped.Track(StartViewTapSource.PickTagSuggestion);
                    updateUiWith(textFieldInfo.FromTagSuggestion(tagSuggestion));
                    break;

                case CreateEntitySuggestion createEntitySuggestion:
                    if (IsSuggestingProjects)
                    {
                        createProject();
                    }
                    else
                    {
                        createTag();
                    }
                    break;

                default:
                    return;
            }

            IObservable<bool> workspaceChangeDenied()
                => dialogService.Confirm(
                    Resources.DifferentWorkspaceAlertTitle,
                    Resources.DifferentWorkspaceAlertMessage,
                    Resources.Ok,
                    Resources.Cancel
                ).Select(Invert);
        }

        private async Task createProject()
        {
            var createProjectStopwatch = stopwatchProvider.CreateAndStore(MeasuredOperation.OpenCreateProjectViewFromStartTimeEntryView);
            createProjectStopwatch.Start();

            var projectId = await navigationService.Navigate<EditProjectViewModel, string, long?>(currentQuery);
            if (projectId == null) return;

            var project = await interactorFactory.GetProjectById(projectId.Value).Execute();
            var projectSuggestion = new ProjectSuggestion(project);

            updateUiWith(textFieldInfo.FromProjectSuggestion(projectSuggestion));
            IsSuggestingProjects = false;
            queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);
            hasAnyProjects = true;
        }

        private async Task createTag()
        {
            var createdTag = await interactorFactory.CreateTag(currentQuery, textFieldInfo.WorkspaceId).Execute();
            var tagSuggestion = new TagSuggestion(createdTag);
            await SelectSuggestion.Execute(tagSuggestion);
            hasAnyTags = true;
            toggleTagSuggestions();
        }

        private void durationTapped()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.Duration);
        }

        private void toggleTagSuggestions()
        {
            if (IsSuggestingTags)
            {
                updateUiWith(textFieldInfo.RemoveTagQueryIfNeeded());
                IsSuggestingTags = false;
                return;
            }

            analyticsService.StartViewTapped.Track(StartViewTapSource.Tags);
            analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.ButtonOverKeyboard);
            OnboardingStorage.ProjectOrTagWasAdded();

            queryAndUpdateUiWith(textFieldInfo.AddQuerySymbol(QuerySymbols.TagsString));
        }

        private void toggleProjectSuggestions()
        {
            if (IsSuggestingProjects)
            {
                IsSuggestingProjects = false;
                updateUiWith(textFieldInfo.RemoveProjectQueryIfNeeded());
                queryByTypeSubject.OnNext(AutocompleteSuggestionType.None);
                return;
            }

            analyticsService.StartViewTapped.Track(StartViewTapSource.Project);
            analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.ButtonOverKeyboard);
            OnboardingStorage.ProjectOrTagWasAdded();

            if (textFieldInfo.HasProject)
            {
                IsSuggestingProjects = true;
                queryByTypeSubject.OnNext(AutocompleteSuggestionType.Projects);
                return;
            }

            queryAndUpdateUiWith(
                textFieldInfo.AddQuerySymbol(QuerySymbols.ProjectsString)
            );
        }

        private void toggleTaskSuggestions(ProjectSuggestion projectSuggestion)
        {
            var currentExpandedProjects = expandedProjects.Value;
            if (currentExpandedProjects.Contains(projectSuggestion))
            {
                currentExpandedProjects.Remove(projectSuggestion);
            }
            else
            {
                currentExpandedProjects.Add(projectSuggestion);
            }
            expandedProjects.OnNext(currentExpandedProjects);
        }

        private void toggleBillable()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.Billable);
            IsBillable = !IsBillable;
        }

        private async Task changeTime()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.StartTime);

            var currentDuration = DurationParameter.WithStartAndDuration(startTime, duration);

            var selectedDuration = await navigationService
                .Navigate<EditDurationViewModel, EditDurationParameters, DurationParameter>(new EditDurationParameters(currentDuration, isStartingNewEntry: true))
                .ConfigureAwait(false);

            startTime = selectedDuration.Start;
            duration = selectedDuration.Duration ?? duration;
        }

        private async Task setStartDate()
        {
            analyticsService.StartViewTapped.Track(StartViewTapSource.StartDate);

            var parameters = isRunning
                ? DateTimePickerParameters.ForStartDateOfRunningTimeEntry(startTime, timeService.CurrentDateTime)
                : DateTimePickerParameters.ForStartDateOfStoppedTimeEntry(startTime);

            var duration = this.duration;

            startTime = await navigationService
                .Navigate<SelectDateTimeViewModel, DateTimePickerParameters, DateTimeOffset>(parameters)
                .ConfigureAwait(false);

            if (isRunning == false)
            {
                this.duration = duration;
            }
        }

        private IObservable<Unit> done()
        {
            var timeEntry = textFieldInfo.AsTimeEntryPrototype(startTime, duration ?? TimeSpan.Zero, IsBillable);
            return interactorFactory.CreateTimeEntry(timeEntry).Execute()
                .Do(_ => navigationService.Close(this))
                .SelectUnit();
        }

        private void onParsedQuery(QueryInfo parsedQuery)
        {
            var newQuery = parsedQuery.Text?.Trim() ?? "";
            if (currentQuery != newQuery)
            {
                currentQuery = newQuery;
                suggestionsLoadingStopwatches[currentQuery] = stopwatchProvider.Create(MeasuredOperation.StartTimeEntrySuggestionsLoadingTime);
                suggestionsLoadingStopwatches[currentQuery].Start();
            }
            bool suggestsTags = parsedQuery.SuggestionType == AutocompleteSuggestionType.Tags;
            bool suggestsProjects = parsedQuery.SuggestionType == AutocompleteSuggestionType.Projects;

            if (!IsSuggestingTags && suggestsTags)
            {
                analyticsService.StartEntrySelectTag.Track(ProjectTagSuggestionSource.TextField);
            }

            if (!IsSuggestingProjects && suggestsProjects)
            {
                analyticsService.StartEntrySelectProject.Track(ProjectTagSuggestionSource.TextField);
            }

            IsSuggestingTags = suggestsTags;
            IsSuggestingProjects = suggestsProjects;
        }

        private IEnumerable<AutocompleteSuggestion> filter(IEnumerable<AutocompleteSuggestion> suggestions)
        {
            suggestionsRenderingStopwatch = stopwatchProvider.Create(MeasuredOperation.StartTimeEntrySuggestionsRenderingTime);
            suggestionsRenderingStopwatch.Start();

            if (textFieldInfo.HasProject && !IsSuggestingProjects && !IsSuggestingTags)
            {
                var projectId = textFieldInfo.Spans.OfType<ProjectSpan>().Single().ProjectId;

                return suggestions.OfType<TimeEntrySuggestion>()
                    .Where(suggestion => suggestion.ProjectId == projectId);
            }

            return suggestions;
        }

        private IEnumerable<AutocompleteSuggestion> addStaticElements(
            IEnumerable<AutocompleteSuggestion> suggestions)
        {
            IList<AutocompleteSuggestion> items = suggestions.ToList();

            if (IsSuggestingProjects)
            {
                if (shouldAddProjectCreationSuggestion())
                {
                    items = (IList<AutocompleteSuggestion>)items
                        .Prepend(
                            new CreateEntitySuggestion(Resources.CreateProject, textFieldInfo.Description)
                        );
                }

                if (!hasAnyProjects)
                {
                    items = (IList<AutocompleteSuggestion>)items.Append(NoEntityInfoMessage.CreateProject());
                }
            }
            else if (IsSuggestingTags)
            {
                if (shouldAddTagCreationSuggestion())
                {
                    items = (IList<AutocompleteSuggestion>)items
                        .Prepend(
                            new CreateEntitySuggestion(Resources.CreateTag, textFieldInfo.Description)
                        );
                }

                if (!hasAnyTags)
                {
                    items = (IList<AutocompleteSuggestion>)items.Append(NoEntityInfoMessage.CreateTag());
                }
            }

            return items;

            bool shouldAddProjectCreationSuggestion()
                => canCreateProjectsInWorkspace && !textFieldInfo.HasProject &&
                   currentQuery.LengthInBytes() <= MaxProjectNameLengthInBytes &&
                   !string.IsNullOrEmpty(currentQuery) &&
                   suggestions.None(item =>
                       item is ProjectSuggestion projectSuggestion &&
                       projectSuggestion.ProjectName.IsSameCaseInsensitiveTrimedTextAs(currentQuery));

            bool shouldAddTagCreationSuggestion()
                => !string.IsNullOrEmpty(currentQuery) && currentQuery.IsAllowedTagByteSize() &&
                   suggestions.None(item =>
                       item is TagSuggestion tagSuggestion &&
                       tagSuggestion.Name.IsSameCaseInsensitiveTrimedTextAs(currentQuery));
        }

        private IEnumerable<IGrouping<long, AutocompleteSuggestion>> group(IEnumerable<AutocompleteSuggestion> suggestions)
        {
            var firstSuggestion = suggestions.FirstOrDefault();
            if (firstSuggestion is ProjectSuggestion)
            {
                return suggestions
                    .Cast<ProjectSuggestion>()
                    .OrderBy(ps => ps.ProjectName)
                    .Where(suggestion => !string.IsNullOrEmpty(suggestion.WorkspaceName))
                    .GroupBy(suggestion => suggestion.WorkspaceId)
                    .OrderByDescending(group => group.First().WorkspaceId == (defaultWorkspace?.Id ?? 0))
                    .ThenBy(group => group.First().WorkspaceName);
            }

            if (IsSuggestingTags)
                suggestions = suggestions.Where(suggestion => suggestion.WorkspaceId == textFieldInfo.WorkspaceId);

            return suggestions
                .GroupBy(suggestion => suggestion.WorkspaceId);
        }

        private IEnumerable<CollectionSection<string, AutocompleteSuggestion>> toCollections(IEnumerable<IGrouping<long, AutocompleteSuggestion>> suggestions)
        {
            var sections = suggestions.Select(group =>
                new CollectionSection<string, AutocompleteSuggestion>(
                    group.First() is ProjectSuggestion projectSuggestion ? projectSuggestion.WorkspaceName : "",
                    group
                        .Distinct(AutocompleteSuggestionComparer.Instance)
                        .SelectMany(includeTasksIfExpanded)
                )
            );

            if (suggestionsLoadingStopwatches.ContainsKey(currentQuery))
            {
                suggestionsLoadingStopwatches[currentQuery]?.Stop();
                suggestionsLoadingStopwatches = new Dictionary<string, IStopwatch>();
            }

            return sections;
        }

        private IEnumerable<AutocompleteSuggestion> includeTasksIfExpanded(AutocompleteSuggestion suggestion)
        {
            yield return suggestion;

            if (suggestion is ProjectSuggestion projectSuggestion && expandedProjects.Value.Contains(projectSuggestion))
            {
                var orderedTasks = projectSuggestion.Tasks
                    .OrderBy(t => t.Name);

                foreach (var taskSuggestion in orderedTasks)
                    yield return taskSuggestion;
            }
        }

        private async Task setBillableValues(long? currentProjectId)
        {
            var hasProject = currentProjectId.HasValue && currentProjectId.Value != ProjectSuggestion.NoProjectId;
            if (hasProject)
            {
                var projectId = currentProjectId.Value;
                IsBillableAvailable =
                    await interactorFactory.IsBillableAvailableForProject(projectId).Execute();

                IsBillable = IsBillableAvailable && await interactorFactory.ProjectDefaultsToBillable(projectId).Execute();
            }
            else
            {
                IsBillable = false;
                IsBillableAvailable = await interactorFactory.IsBillableAvailableForWorkspace(textFieldInfo.WorkspaceId).Execute();
            }
        }

        private void queryWith(TextFieldInfo newTextFieldinfo)
        {
            textFieldInfo = newTextFieldinfo;
            querySubject.OnNext(textFieldInfo);
        }

        private void updateUiWith(TextFieldInfo newTextFieldinfo)
        {
            textFieldInfo = newTextFieldinfo;
            uiSubject.OnNext(textFieldInfo);
        }

        private void queryAndUpdateUiWith(TextFieldInfo newTextFieldinfo)
        {
            textFieldInfo = newTextFieldinfo;
            uiSubject.OnNext(textFieldInfo);
            querySubject.OnNext(textFieldInfo);
        }
    }
}
