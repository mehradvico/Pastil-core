function arrayBufferToBase64(buffer) {
    return btoa(String.fromCharCode.apply(null, new Uint8Array(buffer)));
}

navigator.serviceWorker.register("/service-worker.js").then(async (reg) => {
    const sub = await reg.pushManager.subscribe({
        userVisibleOnly: true,
        applicationServerKey: "BKhX37_3FJv_uKm-HzqHGjmMPh3knZ1Dfi3EcvYp0KrkFd8mzGOeQ5Mn-7W4Rq8yVtS4ay7U3zXv66m9zE2p5wA"
    });

    await fetch("/api/subscribe/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            userId: 1,
            endpoint: sub.endpoint,
            p256dh: arrayBufferToBase64(sub.getKey("p256dh")),
            auth: arrayBufferToBase64(sub.getKey("auth"))
        })
    });
});
