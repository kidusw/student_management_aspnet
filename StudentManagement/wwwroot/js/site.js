// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    var STORAGE_KEY = "sidebar-collapsed";
    var appShell = document.getElementById("app-shell");
    var toggleBtn = document.getElementById("sidebar-collapse-toggle");

    if (!appShell || !toggleBtn) {
        return;
    }

    function setLabel(isCollapsed) {
        var label = isCollapsed ? "Expand sidebar" : "Collapse sidebar";
        toggleBtn.setAttribute("aria-label", label);
        toggleBtn.setAttribute("title", label);
    }

    setLabel(appShell.classList.contains("sidebar-collapsed"));

    toggleBtn.addEventListener("click", function () {
        var isCollapsed = appShell.classList.toggle("sidebar-collapsed");
        try {
            localStorage.setItem(STORAGE_KEY, isCollapsed);
        } catch (e) { }
        setLabel(isCollapsed);
    });
})();
