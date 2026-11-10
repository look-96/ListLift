window.listLiftRecovery = {
    load: key => sessionStorage.getItem(key),
    save: (key, value) => sessionStorage.setItem(key, value),
    clear: key => sessionStorage.removeItem(key)
};
