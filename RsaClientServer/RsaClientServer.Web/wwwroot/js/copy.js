window.copyToClipboard = function (text) {
    return navigator.clipboard.writeText(text || '');
};
