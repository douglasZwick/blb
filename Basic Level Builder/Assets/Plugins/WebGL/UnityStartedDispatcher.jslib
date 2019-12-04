var UnityStartedDispatcher =
{
  DispatchUnityStartedEvent : function()
  {
    window.dispatchEvent(new Event("unitystarted"));
  }
}

mergeInto(LibraryManager.library, UnityStartedDispatcher);
