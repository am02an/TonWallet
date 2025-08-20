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
            
            var buffer = allocate(intArrayFromString(username), 'i8', ALLOC_STACK);
            return buffer;
        }
        var buffer = allocate(intArrayFromString("Unknown"), 'i8', ALLOC_STACK);
        return buffer;
    },

    GetTelegramUserImage: function () {
        if (typeof Telegram !== "undefined" &&
            Telegram.WebApp &&
            Telegram.WebApp.initDataUnsafe &&
            Telegram.WebApp.initDataUnsafe.user &&
            Telegram.WebApp.initDataUnsafe.user.photo_url) {
            
            var imageUrl = Telegram.WebApp.initDataUnsafe.user.photo_url;
            var buffer = allocate(intArrayFromString(imageUrl), 'i8', ALLOC_STACK);
            return buffer;
        }
        var buffer = allocate(intArrayFromString(""), 'i8', ALLOC_STACK);
        return buffer;
    }
});
