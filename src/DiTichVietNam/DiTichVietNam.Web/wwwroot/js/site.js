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

    var menu = document.getElementById('main-menu');
    var toggler = document.querySelector('.navbar-toggler[data-bs-target="#main-menu"]');
    if (menu && toggler && window.bootstrap) {
        document.addEventListener('keydown', function (e) {
            if (e.key !== 'Escape' || !menu.classList.contains('show')) {
                return;
            }
            bootstrap.Collapse.getOrCreateInstance(menu).hide();
            toggler.focus();
        });
    }
});
