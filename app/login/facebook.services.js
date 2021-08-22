import { settings } from "../settings/settings.js";

async function login(code) {
    return await fetch(settings.api + "/token/facebook", {
        method: "POST",
        credentials: "omit",
        body: JSON.stringify(code),
        headers: {
            "Content-Type": "application/json",
            "iss": settings.app,
            "aud": settings.host
        }
    });
}

export const services = {
    login: login
}