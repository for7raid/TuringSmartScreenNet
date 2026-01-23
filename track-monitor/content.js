let lastTrackId = null;
let lastState = null;

/**
 * Определяем состояние воспроизведения
 */
function getPlaybackState() {
    const ms = navigator.mediaSession;

    if (!ms || !ms.metadata) {
        return { state: 'stoped', trackId: null, meta: null };
    }

    const meta = ms.metadata;
    const trackId = `${meta.title || ''}|${meta.artist || ''}|${meta.album || ''}`;

    let playbackState = ms.playbackState || 'none';

    if (playbackState === 'none') {
        const vkButton = document.querySelector('[data-testid=audio-player-controls-state-button]>span')?.innerText;
        playbackState = vkButton === 'Воспроизвести' ? 'paused' :
            vkButton == 'Приостановить' ? 'playing' :
                'none';
    }

    return { playbackState, trackId, meta };
}

/**
 * Основной polling
 */
setInterval(() => {
    const { playbackState, trackId, meta } = getPlaybackState();

    // // stop
    // if (playbackState === 'none' && lastState !== 'none') {
    //     chrome.runtime.sendMessage({
    //         type: 'MEDIA_SNAPSHOT',
    //         playbackState: 'none',
    //         title: meta.title,
    //         artist: meta.artist,
    //         album: meta.album
    //     });
    //     lastState = 'stoped';
    //     lastTrackId = null;
    //     return;
    // }

    if (!meta) return;

    // смена трека
    if (trackId !== lastTrackId) {
        chrome.runtime.sendMessage({
            type: 'MEDIA_SNAPSHOT',
            playbackState: 'playing',
            title: meta.title,
            artist: meta.artist,
            album: meta.album,
            host: window.location.host,
        });
        lastTrackId = trackId;
        lastState = 'playing';
        return;
    }

    // play (первичное)
    if (lastState !== playbackState) {
        chrome.runtime.sendMessage({
            type: 'MEDIA_SNAPSHOT',
            playbackState,
            title: meta.title,
            artist: meta.artist,
            album: meta.album,
            host: window.location.host,
        });
        lastState = playbackState;
    }

}, 1000);
