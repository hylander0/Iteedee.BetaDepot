var APP = window.app || {};
window.app = APP;
APP.init = function () {
    //Setup Validation
    $("input,select,textarea").not("[type=submit]").jqBootstrapValidation();
};
$(function () { APP.init(); });