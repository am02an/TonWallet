mergeInto(LibraryManager.library, {
    GetTelegramUsername: function () {
        if (typeof Telegram !== "undefined" &&
            Telegram.WebApp &&
            Telegram.WebApp.initDataUnsafe &&
            Telegram.WebApp.initDataUnsafe.user) {
            
            var username = "";
            if (Telegram.WebApp.initDataUnsafe.user.username) {
                username = Telegram.WebApp.initDataUnsafe.user.username;
            }
            return allocateUTF8(username);
        }
        return allocateUTF8("");
    }
});
