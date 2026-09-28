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

    var EDGE_TOLERANCE = 2;
    var TIE_TOLERANCE = 8;
    var SETTLE_INTERVAL = 60;
    var SETTLE_TICKS = 3;
    var PIN_LIMIT = 1600;

    var pending = false;
    var pinned = null;
    var currentAnchor = sections[0].anchor;
    var settleTimer = null;
    var limitTimer = null;
    var lastOffset = null;
    var stableTicks = 0;

    function pixels(value) {
        var parsed = parseFloat(value);
        return isNaN(parsed) ? 0 : parsed;
    }

    function anchorOffset(element) {
        return pixels(window.getComputedStyle(document.documentElement).scrollPaddingTop)
            + pixels(window.getComputedStyle(element).scrollMarginTop);
    }

    function activeAnchor() {
        if (pinned) {
            return pinned;
        }

        var docHeight = document.documentElement.scrollHeight;
        if (window.pageYOffset + window.innerHeight >= docHeight - 2) {
            return sections[sections.length - 1].anchor;
        }

        var best = sections[0].anchor;
        var bestTop = null;
        var currentTop = null;

        for (var i = 0; i < sections.length; i++) {
            var top = sections[i].element.getBoundingClientRect().top - anchorOffset(sections[i].element);

            if (sections[i].anchor === currentAnchor) {
                currentTop = top;
            }

            if (top <= EDGE_TOLERANCE && (bestTop === null || top > bestTop)) {
                bestTop = top;
                best = sections[i].anchor;
            }
        }

        if (bestTop !== null && currentTop !== null
            && currentTop <= EDGE_TOLERANCE && bestTop - currentTop <= TIE_TOLERANCE) {
            return currentAnchor;
        }

        return best;
    }

    function highlight() {
        pending = false;
        var active = activeAnchor();
        currentAnchor = active;

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

    function stopSettleTimer() {
        if (settleTimer !== null) {
            window.clearInterval(settleTimer);
            settleTimer = null;
        }
    }

    function release() {
        stopSettleTimer();

        if (limitTimer !== null) {
            window.clearTimeout(limitTimer);
            limitTimer = null;
        }

        document.removeEventListener('scrollend', release);
        pinned = null;
        schedule();
    }

    function releaseWhenSettled() {
        stopSettleTimer();
        lastOffset = null;
        stableTicks = 0;

        settleTimer = window.setInterval(function () {
            var offset = window.pageYOffset;
            if (lastOffset !== null && Math.abs(offset - lastOffset) < 1) {
                stableTicks++;
            } else {
                stableTicks = 0;
            }

            lastOffset = offset;

            if (stableTicks >= SETTLE_TICKS) {
                stopSettleTimer();
                release();
            }
        }, SETTLE_INTERVAL);
    }

    function pin(anchor) {
        release();
        pinned = anchor;
        schedule();
        limitTimer = window.setTimeout(release, PIN_LIMIT);

        if ('onscrollend' in window) {
            document.addEventListener('scrollend', release);
            return;
        }

        releaseWhenSettled();
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
