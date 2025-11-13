const CACHE_NAME = 'kafe-qr-menu-v2'; // ✅ Bumped version to force update
const urlsToCache = [
    '/',
    '/Home/Index',
    '/offline.html',
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
    self.skipWaiting(); // Activate immediately
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

// ✅ NEW: Network-first strategy for dynamic content (menu pages)
function networkFirstStrategy(request) {
    return fetch(request)
        .then(response => {
            // Clone the response before caching
            const responseToCache = response.clone();

            // Update cache with fresh content
            caches.open(CACHE_NAME).then(cache => {
                cache.put(request, responseToCache);
                console.log('[Service Worker] Updated cache for:', request.url);
            });

            return response;
        })
        .catch(error => {
            console.log('[Service Worker] Network failed, trying cache:', request.url);
            // Fallback to cache if network fails (offline mode)
            return caches.match(request)
                .then(cachedResponse => {
                    if (cachedResponse) {
                        console.log('[Service Worker] Serving from cache (offline):', request.url);
                        return cachedResponse;
                    }
                    // If not in cache, show offline page
                    return caches.match('/offline.html');
                });
        });
}

// ✅ NEW: Cache-first strategy for static assets
function cacheFirstStrategy(request) {
    return caches.match(request)
        .then(cachedResponse => {
            if (cachedResponse) {
                console.log('[Service Worker] Serving static asset from cache:', request.url);
                return cachedResponse;
            }

            // If not in cache, fetch from network and cache it
            return fetch(request).then(response => {
                const responseToCache = response.clone();
                caches.open(CACHE_NAME).then(cache => {
                    cache.put(request, responseToCache);
                });
                return response;
            });
        });
}

// ✅ UPDATED: Fetch event with smart routing
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Identify static assets (CSS, JS, images, libraries)
    const isStaticAsset =
        url.pathname.startsWith('/css/') ||
        url.pathname.startsWith('/js/') ||
        url.pathname.startsWith('/lib/') ||
        url.pathname.startsWith('/images/') ||
        url.pathname.endsWith('.css') ||
        url.pathname.endsWith('.js') ||
        url.pathname.endsWith('.png') ||
        url.pathname.endsWith('.jpg') ||
        url.pathname.endsWith('.jpeg') ||
        url.pathname.endsWith('.gif') ||
        url.pathname.endsWith('.webp') ||
        url.hostname !== location.hostname; // CDN resources

    // Identify menu/dynamic pages
    const isMenuPage =
        url.pathname === '/' ||
        url.pathname === '/Home/Index' ||
        url.pathname.startsWith('/Home/Menu/');

    // ✅ Network-first for menu pages (always fresh when online)
    if (isMenuPage) {
        event.respondWith(networkFirstStrategy(event.request));
        return;
    }

    // ✅ Cache-first for static assets (fast loading)
    if (isStaticAsset) {
        event.respondWith(cacheFirstStrategy(event.request));
        return;
    }

    // Don't intercept other routes (let MVC handle them)
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