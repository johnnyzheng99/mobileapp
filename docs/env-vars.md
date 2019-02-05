# Environment Variables :microscope:

While all Toggl apps are open source, not all things can be made public. This includes, but is not limited to, analytics services' keys and testing credentials. When in doubt, read the company policy before making something public.

Below is a list of all the environment variables you need to configure before building and the purposes they serve:

- iOS analytics' services :bar_chart:
`TOGGL_AD_UNIT_ID_FOR_BANNER_TEST`
`TOGGL_AD_UNIT_ID_FOR_INTERSTITIAL_TEST`
`TOGGL_CLIENT_ID`
`TOGGL_REVERSED_CLIENT_ID`
`TOGGL_API_KEY`
`TOGGL_GCM_SENDER_ID`
`TOGGL_PROJECT_ID`
`TOGGL_STORAGE_BUCKET`
`TOGGL_GOOGLE_APP_ID`
`TOGGL_ADJUST_APP_TOKEN`

- Android Google login :busts_in_silhouette:
`TOGGL_DROID_GOOGLE_SERVICES_API_KEY`
`TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID`
`TOGGL_DROID_GOOGLE_SERVICES_MOBILE_SDK_APP_ID`
`TOGGL_DROID_GOOGLE_SERVICES_PROJECT_NUMBER`
`TOGGL_DROID_GOOGLE_SERVICES_PROJECT_ID`

- App Center crash tracking :boom:
`TOGGL_APP_CENTER_ID_IOS`
`TOGGL_APP_CENTER_ID_DROID`

# new 'puter who dis? :computer:

To configure this on a macOS:

```
$ touch ~/.bash_profile
$ open ~/.bash_profile
```

Then add the following lines to `.bash_profile`:
```
export TOGGL_AD_UNIT_ID_FOR_BANNER_TEST=""
export TOGGL_AD_UNIT_ID_FOR_INTERSTITIAL_TEST=""
export TOGGL_CLIENT_ID=""
export TOGGL_REVERSED_CLIENT_ID=""
export TOGGL_API_KEY=""
export TOGGL_GCM_SENDER_ID=""
export TOGGL_PROJECT_ID=""
export TOGGL_STORAGE_BUCKET=""
export TOGGL_GOOGLE_APP_ID=""
export TOGGL_DATABASE_URL=""
export TOGGL_APP_CENTER_ID_IOS=""
export TOGGL_APP_CENTER_ID_DROID=""
export TOGGL_DROID_GOOGLE_SERVICES_API_KEY=""
export TOGGL_DROID_GOOGLE_SERVICES_CLIENT_ID=""
export TOGGL_DROID_GOOGLE_SERVICES_MOBILE_SDK_APP_ID=""
export TOGGL_DROID_GOOGLE_SERVICES_PROJECT_NUMBER=""
export TOGGL_DROID_GOOGLE_SERVICES_PROJECT_ID=""
export TOGGL_ADJUST_APP_TOKEN=""
```

Finish it off with:

```
$ source ~/.bash_profile
```

NOTE: Replace `bash_profile` with `zshrc` if you're using `zsh`
