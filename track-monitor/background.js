chrome.runtime.onMessage.addListener((msg) => {
    if (msg.type !== 'MEDIA_SNAPSHOT') return;

    fetch('http://127.0.0.1:10455/', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            event: msg.state,
            title: msg.title || null,
            artist: msg.artist || null,
            album: msg.album || null,
            playbackState: msg.playbackState || null,
            host: msg.host || null,
            timestamp: Date.now()
        })
    }).catch(() => { });
});
