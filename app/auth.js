const key = "user";

function verifyToken() {
    if (window.location.pathname === "/login.html")
        return;

    const user = localStorage.getItem(key);

    if (!user) {
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