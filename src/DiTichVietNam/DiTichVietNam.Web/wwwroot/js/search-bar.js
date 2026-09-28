$(function () {
    var inlineBreakpoint = 576;
    var $filters = $('.search-bar-filters');

    if ($filters.length === 0) {
        return;
    }

    var openOnNarrowScreen = $filters.prop('open');
    var wasWide = false;

    function isWide() {
        return window.innerWidth >= inlineBreakpoint;
    }

    function syncFilters() {
        var wide = isWide();
        if (wide) {
            if (!wasWide) {
                openOnNarrowScreen = $filters.prop('open');
            }
            $filters.prop('open', true);
        } else if (wasWide) {
            $filters.prop('open', openOnNarrowScreen);
        }
        wasWide = wide;
    }

    syncFilters();
    $(window).on('resize', syncFilters);
});
