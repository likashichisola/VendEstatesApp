// Vend Estates Management System - Service Worker
// Provides an app shell cache, offline fallback, and a push notification structure.

const CACHE_VERSION = 'v1';
const STATIC_CACHE = `vend-estates-static-${CACHE_VERSION}`;
const RUNTIME_CACHE = `vend-estates-runtime-${CACHE_VERSION}`;
const OFFLINE_URL = '/offline.html';

// Core "app shell" assets that make the UI usable while offline.
const APP_SHELL_ASSETS = [
    OFFLINE_URL,
    '/manifest.json',
    '/css/site.css',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/icons/icon-192x192.png',
    '/icons/icon-512x512.png',
    '/favicon.png'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(STATIC_CACHE)
            .then((cache) => cache.addAll(APP_SHELL_ASSETS))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(
                keys
                    .filter((key) => key !== STATIC_CACHE && key !== RUNTIME_CACHE)
                    .map((key) => caches.delete(key))
            ))
            .then(() => self.clients.claim())
    );
});

// Network-first for navigations (HTML pages), so authenticated/data-driven pages stay fresh,
// falling back to a cached copy or the offline page when the network is unavailable.
// Cache-first (with background refresh) for static assets under wwwroot.
self.addEventListener('fetch', (event) => {
    const { request } = event;

    if (request.method !== 'GET') {
        return;
    }

    const url = new URL(request.url);
    if (url.origin !== self.location.origin) {
        return;
    }

    if (request.mode === 'navigate') {
        event.respondWith(networkFirstNavigation(request));
        return;
    }

    if (isStaticAsset(url.pathname)) {
        event.respondWith(cacheFirst(request));
    }
});

function isStaticAsset(pathname) {
    return pathname.startsWith('/css/') ||
        pathname.startsWith('/js/') ||
        pathname.startsWith('/lib/') ||
        pathname.startsWith('/icons/') ||
        pathname === '/favicon.png' ||
        pathname === '/manifest.json';
}

async function networkFirstNavigation(request) {
    try {
        const networkResponse = await fetch(request);
        const cache = await caches.open(RUNTIME_CACHE);
        cache.put(request, networkResponse.clone());
        return networkResponse;
    } catch {
        const cachedResponse = await caches.match(request);
        return cachedResponse || caches.match(OFFLINE_URL);
    }
}

async function cacheFirst(request) {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
        // Refresh the cache in the background so assets stay current.
        fetch(request)
            .then((response) => caches.open(STATIC_CACHE).then((cache) => cache.put(request, response)))
            .catch(() => { /* offline - keep serving cached asset */ });
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(request);
        const cache = await caches.open(STATIC_CACHE);
        cache.put(request, networkResponse.clone());
        return networkResponse;
    } catch {
        return cachedResponse;
    }
}

// ===================== Push Notification Support Structure =====================
// Requires VAPID keys and a server-side subscription endpoint to be fully wired up.
// The client-side subscription flow lives in wwwroot/js/pwa.js (subscribeToPushNotifications).

self.addEventListener('push', (event) => {
    if (!event.data) {
        return;
    }

    let payload;
    try {
        payload = event.data.json();
    } catch {
        payload = { title: 'Vend Estates', body: event.data.text() };
    }

    const title = payload.title || 'Vend Estates Management System';
    const options = {
        body: payload.body || 'You have a new notification.',
        icon: '/icons/icon-192x192.png',
        badge: '/icons/icon-96x96.png',
        data: { url: payload.url || '/Notification/Index' },
        tag: payload.tag || 'vend-estates-notification'
    };

    event.waitUntil(self.registration.showNotification(title, options));
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    const targetUrl = (event.notification.data && event.notification.data.url) || '/Notification/Index';

    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientsList) => {
            for (const client of clientsList) {
                if (client.url.includes(targetUrl) && 'focus' in client) {
                    return client.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow(targetUrl);
            }
        })
    );
});
