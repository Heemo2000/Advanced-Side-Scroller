mergeInto(LibraryManager.library, {
  IsMobileBrowser: function () {
    const ua = navigator.userAgent || navigator.vendor || window.opera;

    if (/android/i.test(ua)) return 1;
    if (/iPad|iPhone|iPod/.test(ua) && !window.MSStream) return 1;

    return 0;
  }
});
