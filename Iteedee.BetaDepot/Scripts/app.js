var APP = window.app || {};
window.app = APP;
APP.init = function () {
};
APP.helpers = {
    prepareRelativeUrl: function (url) {
        return $('head base').attr('href') + url;
    },
    addAntiForgeryToken: function(data) {
        data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
        return data;
    }
}
$(function () { APP.init(); });