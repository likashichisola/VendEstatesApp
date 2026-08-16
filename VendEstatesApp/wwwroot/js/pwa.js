// Vend Estates Management System - PWA registration and install prompt handling.
(function () {
    'use strict';

    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function () {
            navigator.serviceWorker.register('/service-worker.js')
                .then(function (registration) {
                    console.log('[PWA] Service worker registered with scope:', registration.scope);
                })
                .catch(function (error) {
                    console.error('[PWA] Service worker registration failed:', error);
                });
        });
    }

    // ===================== Install prompt (Add to Home Screen) =====================
    var deferredInstallPrompt = null;
    var installButton = document.getElementById('pwaInstallButton');

    if (installButton) {
        installButton.classList.add('d-none');
    }

    window.addEventListener('beforeinstallprompt', function (event) {
        event.preventDefault();
        deferredInstallPrompt = event;

        if (installButton) {
            installButton.classList.remove('d-none');
        }
    });

    if (installButton) {
        installButton.addEventListener('click', function () {
            if (!deferredInstallPrompt) {
                return;
            }

            deferredInstallPrompt.prompt();
            deferredInstallPrompt.userChoice.finally(function () {
                deferredInstallPrompt = null;
                installButton.classList.add('d-none');
            });
        });
    }

    window.addEventListener('appinstalled', function () {
        if (installButton) {
            installButton.classList.add('d-none');
        }
        deferredInstallPrompt = null;
        console.log('[PWA] Application installed.');
    });

    // ===================== Push notification subscription structure =====================
    // Call VendEstatesPwa.subscribeToPushNotifications(vapidPublicKey) once a server-side
    // subscription endpoint (e.g. NotificationController.Subscribe) is available.
    window.VendEstatesPwa = {
        isPushSupported: function () {
            return 'serviceWorker' in navigator && 'PushManager' in window;
        },

        subscribeToPushNotifications: async function (vapidPublicKey) {
            if (!this.isPushSupported()) {
                console.warn('[PWA] Push notifications are not supported in this browser.');
                return null;
            }

            var permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                console.warn('[PWA] Notification permission was not granted.');
                return null;
            }

            var registration = await navigator.serviceWorker.ready;
            var subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(vapidPublicKey)
            });

            // TODO: POST `subscription` to a server-side endpoint (e.g. /Notification/Subscribe)
            // so push messages can be sent to this device.
            return subscription;
        }
    };

    function urlBase64ToUint8Array(base64String) {
        var padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        var base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        var rawData = window.atob(base64);
        var outputArray = new Uint8Array(rawData.length);

        for (var i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
})();
