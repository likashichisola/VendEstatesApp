(function () {
    'use strict';

    var body = document.body;
    var sidebarToggle = document.getElementById('sidebarToggle');
    var sidebarCollapseToggle = document.getElementById('sidebarCollapseToggle');

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            body.classList.toggle('sidebar-open');
        });
    }

    if (sidebarCollapseToggle) {
        sidebarCollapseToggle.addEventListener('click', function () {
            body.classList.toggle('sidebar-collapsed');
            try {
                localStorage.setItem('ves-sidebar-collapsed', body.classList.contains('sidebar-collapsed'));
            } catch (e) { /* ignore storage errors */ }
        });
    }

    try {
        if (localStorage.getItem('ves-sidebar-collapsed') === 'true') {
            body.classList.add('sidebar-collapsed');
        }
    } catch (e) { /* ignore storage errors */ }

    // Auto-dismiss alerts after a few seconds.
    document.querySelectorAll('.alert-dismissible').forEach(function (alertEl) {
        setTimeout(function () {
            var alert = bootstrap.Alert.getOrCreateInstance(alertEl);
            if (alert) {
                alert.close();
            }
        }, 6000);
    });
})();
