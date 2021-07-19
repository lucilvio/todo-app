const key = "user";
const now = Math.floor(Date.now() / 1000);

function verifyToken() {
    if (window.location.anonymousAccess)
        return;

    const userToken = JSON.parse(localStorage.getItem(key));

    if (!userToken || !userToken.expiresIn || userToken.expiresIn < now) {
        window.location.href = "login.html";
    }
}

verifyToken();

export const auth = {
    login: function(user) {
        localStorage.setItem(key, JSON.stringify(user));
    },
    getUser: function() {
        return JSON.parse(localStorage.getItem(key));
    },
    logout: function() {
        localStorage.removeItem(key);
    }
}