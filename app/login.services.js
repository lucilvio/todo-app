import { settings } from "./settings.js";

async function login(user) {
    const reponse = await fetch(settings.api + "/token", {
        method: "post",
        body: JSON.stringify(user),
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