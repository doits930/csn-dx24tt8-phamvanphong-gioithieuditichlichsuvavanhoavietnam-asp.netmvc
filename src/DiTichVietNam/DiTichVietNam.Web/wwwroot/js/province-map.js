$(function () {
    var plate = $('[data-province-map]');
    if (plate.length === 0) {
        return;
    }

    var readout = plate.find('[data-map-readout]');
    var readoutName = plate.find('[data-map-readout-name]');
    var readoutNote = plate.find('[data-map-readout-note]');
    var mapLinks = plate.find('[data-province-slug]');
    var directoryItems = $('.province-list li').has('[data-province-slug]');
    var directoryLinks = $('.province-list [data-province-slug]');
    var pointerIsCoarse = window.matchMedia('(hover: none)').matches;
    var revealedSlug = null;

    function plateRect() {
        return plate[0].getBoundingClientRect();
    }

    function placeReadout(x, y) {
        var width = readout.outerWidth();
        var height = readout.outerHeight();
        var limitX = Math.max(plate.width() - width, 0);
        var limitY = Math.max(plate.height() - height, 0);
        var left = Math.min(Math.max(x + 18, 0), limitX);
        var top = Math.min(Math.max(y - height - 18, 0), limitY);
        readout[0].style.setProperty('--readout-x', left + 'px');
        readout[0].style.setProperty('--readout-y', top + 'px');
    }

    function showReadout(link, x, y) {
        readoutName.text(link.attr('data-readout-name'));
        readoutNote.text(link.attr('data-readout-note'));
        readout.prop('hidden', false);
        placeReadout(x, y);
    }

    function anchorPoint(link) {
        var labelX = parseFloat(link.attr('data-label-x'));
        var labelY = parseFloat(link.attr('data-label-y'));
        if (!isNaN(labelX) && !isNaN(labelY)) {
            return { x: labelX / 100 * plate.width(), y: labelY / 100 * plate.height() };
        }
        var shape = link[0].getBoundingClientRect();
        var frame = plateRect();
        return { x: shape.left + shape.width / 2 - frame.left, y: shape.top - frame.top };
    }

    function showReadoutAtLabel(link) {
        var point = anchorPoint(link);
        showReadout(link, point.x, point.y);
    }

    function highlight(slug) {
        mapLinks.filter('[data-province-slug="' + slug + '"]').addClass('is-active');
        directoryLinks.filter('[data-province-slug="' + slug + '"]').closest('li').addClass('is-active');
    }

    function clearHighlight() {
        mapLinks.removeClass('is-active');
        directoryItems.removeClass('is-active');
    }

    function reset() {
        clearHighlight();
        readout.prop('hidden', true);
        revealedSlug = null;
    }

    plate.on('mouseenter', '[data-province-slug]', function (event) {
        var link = $(this);
        var rect = plateRect();
        clearHighlight();
        highlight(link.attr('data-province-slug'));
        showReadout(link, event.clientX - rect.left, event.clientY - rect.top);
    });

    plate.on('mousemove', '[data-province-slug]', function (event) {
        if (readout.prop('hidden')) {
            return;
        }
        var rect = plateRect();
        placeReadout(event.clientX - rect.left, event.clientY - rect.top);
    });

    plate.on('mouseleave', function () {
        if (revealedSlug === null) {
            reset();
        }
    });

    plate.on('focusin', '[data-province-slug]', function () {
        var link = $(this);
        clearHighlight();
        highlight(link.attr('data-province-slug'));
        showReadoutAtLabel(link);
    });

    plate.on('focusout', function () {
        reset();
    });

    directoryLinks.on('mouseenter focusin', function () {
        clearHighlight();
        highlight($(this).attr('data-province-slug'));
    });

    directoryLinks.on('mouseleave focusout', function () {
        clearHighlight();
    });

    if (pointerIsCoarse) {
        plate.on('click', '[data-province-slug]', function (event) {
            var link = $(this);
            var slug = link.attr('data-province-slug');
            if (revealedSlug === slug) {
                return;
            }
            event.preventDefault();
            var rect = plateRect();
            clearHighlight();
            highlight(slug);
            showReadout(link, event.clientX - rect.left, event.clientY - rect.top);
            revealedSlug = slug;
        });

        $(document).on('click', function (event) {
            if ($(event.target).closest('[data-province-map]').length === 0) {
                reset();
            }
        });
    }

    $(window).on('resize', function () {
        reset();
    });
});
