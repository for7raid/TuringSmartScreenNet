let lastPosition = null;
let lastDuration = null;

(function intercept() {
  const ms = navigator.mediaSession;
  if (!ms || !ms.setPositionState) return;

  const original = ms.setPositionState.bind(ms);

  ms.setPositionState = (state) => {
    if (state) {
      lastPosition = state.position ?? lastPosition;
      lastDuration = state.duration ?? lastDuration;
    }
    return original(state);
  };

  window.__MEDIA_POSITION__ = () => ({
    position: lastPosition,
    duration: lastDuration
  });
})();
