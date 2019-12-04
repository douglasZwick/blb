var UrlDataRetreiver =
{
  GetUrl : function()
  {
    var url = window.location.href;
    var bufferSize = lengthBytesUTF8(url) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(url, buffer, bufferSize);

    return buffer;
  }
}

mergeInto(LibraryManager.library, UrlDataRetreiver);
