var IoHandler =
{
  SyncFiles : function()
  {
    FS.syncfs(false, function(err) { /* handle callback...???????? */ });
  }
};

mergeInto(LibraryManager.library, IoHandler);
