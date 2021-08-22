import { settings } from "../settings/settings.js";

async function login(code) {
    const reponse = await fetch(settings.api + "/token/facebook", {
        method: "POST",
        credentials: "omit",
        body: JSON.stringify(code),
        headers: {
            "Content-Type": "application/json",
            "iss": settings.app,
            "aud": settings.host
        }
    });

    if (!reponse.ok) {
        if (reponse.status === 404)
            throw "Login service not found";

        var data = await reponse.json();
        throw data.message;
    }

    return await reponse.json();
}

export const services = {
    login: login
}