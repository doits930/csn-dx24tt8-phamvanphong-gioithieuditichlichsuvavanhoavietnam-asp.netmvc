(function ($) {
    'use strict';

    var $bar = $('[data-section-bar]');
    if ($bar.length === 0) {
        return;
    }

    var $links = $bar.find('[data-section-link]');
    var sections = [];

    $links.each(function () {
        var anchor = $(this).attr('data-section-link');
        var element = document.getElementById(anchor);
        if (element) {
            sections.push({ anchor: anchor, element: element });
        }
    });

    if (sections.length === 0) {
        return;
    }

    var barNode = $bar[0];
    var pending = false;
    var pinned = null;
    var pinnedAt = 0;

    function activeAnchor() {
        var edge = barNode.getBoundingClientRect().bottom + 32;

        if (pinned) {
            var pinnedNode = document.getElementById(pinned);
            var pinnedTop = pinnedNode ? pinnedNode.getBoundingClientRect().top : null;
            var settling = Date.now() - pinnedAt < 400;
            if (pinnedTop !== null && (settling || (pinnedTop <= edge + 8 && pinnedTop >= edge - 160))) {
                return pinned;
            }

            pinned = null;
        }

        var docHeight = document.documentElement.scrollHeight;
        if (window.pageYOffset + window.innerHeight >= docHeight - 2) {
            return sections[sections.length - 1].anchor;
        }

        var current = sections[0].anchor;
        var bestTop = null;

        for (var i = 0; i < sections.length; i++) {
            var top = sections[i].element.getBoundingClientRect().top;
            if (top <= edge && (bestTop === null || top > bestTop)) {
                bestTop = top;
                current = sections[i].anchor;
            }
        }

        return current;
    }

    function highlight() {
        pending = false;
        var active = activeAnchor();

        $links.each(function () {
            var $link = $(this);
            var isActive = $link.attr('data-section-link') === active;
            $link.toggleClass('is-active', isActive);

            if (isActive) {
                $link.attr('aria-current', 'true');
            } else {
                $link.removeAttr('aria-current');
            }
        });

        var node = $links.filter('.is-active')[0];
        var scroller = $bar.find('.relic-section-links')[0];
        if (node && scroller && scroller.scrollWidth > scroller.clientWidth) {
            scroller.scrollTo({ left: Math.max(0, node.offsetLeft - 16), behavior: 'auto' });
        }
    }

    function schedule() {
        if (pending) {
            return;
        }

        pending = true;
        window.requestAnimationFrame(highlight);
    }

    function pin(anchor) {
        pinned = anchor;
        pinnedAt = Date.now();
        window.setTimeout(schedule, 0);
        window.setTimeout(schedule, 420);
    }

    $links.on('click', function () {
        pin($(this).attr('data-section-link'));
    });

    $bar.find('.relic-section-visit').on('click', function () {
        pin('tham-quan');
    });

    $(window).on('scroll resize', schedule);
    highlight();
}(jQuery));
