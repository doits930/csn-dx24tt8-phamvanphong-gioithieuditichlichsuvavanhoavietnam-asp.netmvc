$(function () {
    $('form[data-search-form]').on('submit', function (e) {
        var input = $(this).find('input[name="tuKhoa"]');
        var keyword = $.trim(input.val());
        input.val(keyword);
        if (keyword.length === 0) {
            e.preventDefault();
            input.trigger('focus');
        }
    });
});
