const CACHE_NAME = 'kafe-qr-menu-v1';
const urlsToCache = [
    '/',
    '/Home/Index',
    '/css/site.css',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css',
    '/lib/bootstrap/dist/js/bootstrap.bundle.min.js',
    '/lib/jquery/dist/jquery.min.js',
    'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css'
];

// Install event - cache resources
self.addEventListener('install', event => {
    console.log('[Service Worker] Installing...');
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('[Service Worker] Caching app shell');
                return cache.addAll(urlsToCache.map(url => new Request(url, { credentials: 'same-origin' })))
                    .catch(error => {
                        console.log('[Service Worker] Cache addAll error:', error);
                    });
            })
    );
    self.skipWaiting();
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('[Service Worker] Activating...');
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('[Service Worker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
    return self.clients.claim();
});

// Fetch event - ONLY cache Home/Index and static assets
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Only intercept for Home/Index or static assets (css, js, images)
    const shouldCache =
        url.pathname === '/' ||
        url.pathname === '/Home/Index' ||
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname.startsWith('/images/') ||
        url.hostname !== location.hostname; // CDN resources

    if (!shouldCache) {
        // Let it go through normal MVC routing
        return;
    }

    // Cache strategy only for allowed URLs
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                if (response) {
                    console.log('[Service Worker] Serving from cache:', event.request.url);
                    return response;
                }

                const fetchRequest = event.request.clone();

                return fetch(fetchRequest).then(response => {
                    if (!response || response.status !== 200 || response.type !== 'basic') {
                        return response;
                    }

                    const responseToCache = response.clone();

                    caches.open(CACHE_NAME)
                        .then(cache => {
                            cache.put(event.request, responseToCache);
                        });

                    return response;
                }).catch(error => {
                    console.log('[Service Worker] Fetch failed:', error);
                    return caches.match('/offline.html');
                });
            })
    );
});

// Background Sync
self.addEventListener('sync', event => {
    console.log('[Service Worker] Background sync:', event.tag);
    if (event.tag === 'sync-menu') {
        event.waitUntil(syncMenu());
    }
});

async function syncMenu() {
    try {
        const response = await fetch('/Home/Index');
        const cache = await caches.open(CACHE_NAME);
        await cache.put('/Home/Index', response);
        console.log('[Service Worker] Menu synced');
    } catch (error) {
        console.log('[Service Worker] Sync failed:', error);
    }
}

// Push Notifications
self.addEventListener('push', event => {
    console.log('[Service Worker] Push received:', event);

    const options = {
        body: event.data ? event.data.text() : 'Yeni menü güncellemesi!',
        icon: '/images/icons/icon-192x192.png',
        badge: '/images/icons/icon-72x72.png',
        vibrate: [200, 100, 200],
        data: {
            dateOfArrival: Date.now(),
            primaryKey: 1
        }
    };

    event.waitUntil(
        self.registration.showNotification('Kafe QR Menü', options)
    );
});

// Notification click
self.addEventListener('notificationclick', event => {
    console.log('[Service Worker] Notification click received.');
    event.notification.close();
    event.waitUntil(
        clients.openWindow('/')
    );
});