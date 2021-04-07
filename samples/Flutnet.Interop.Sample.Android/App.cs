using System;
using Android.App;
using Android.Content;

namespace Flutnet.Interop.Sample
{
    /// <summary>
    /// Singleton class for Application wide objects. 
    /// </summary>
    public static class App
    {
        static ActivityLifecycleContextListener _lifecycleListener;

        public static Context AppContext => Application.Context;

        public static Activity CurrentActivity => _lifecycleListener?.Activity;

        public static bool Initialized { get; private set; }

        public static void Init(Application application)
        {
            if (Initialized)
                return;

            _lifecycleListener = new ActivityLifecycleContextListener();
            application.RegisterActivityLifecycleCallbacks(_lifecycleListener);
            Initialized = true;
        }

        public static void Init(Activity activity)
        {
            if (Initialized)
                return;

            _lifecycleListener = new ActivityLifecycleContextListener { Activity = activity };
            activity.Application.RegisterActivityLifecycleCallbacks(_lifecycleListener);
            Initialized = true;
        }
    }

    class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        readonly WeakReference<Activity> _currentActivity = new WeakReference<Activity>(null);

        internal Context Context => Activity ?? Application.Context;

        internal Activity Activity
        {
            get => _currentActivity.TryGetTarget(out var a) ? a : null;
            set => _currentActivity.SetTarget(value);
        }

        void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Android.OS.Bundle savedInstanceState)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity)
        {
            Activity = activity;
        }

        void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Android.OS.Bundle outState)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity)
        {
        }

        void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity)
        {
        }
    }
}