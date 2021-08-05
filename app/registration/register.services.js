import { settings } from "../settings/settings.js";

async function register(user) {
    const reponse = await fetch(settings.api + "/users", {
        method: "post",
        body: JSON.stringify(user),
        headers: {
            "Content-Type": "application/json"
        }
    });

    if (!reponse.ok) {
        if (reponse.status === 404)
            throw "Could not find user register service";

        var data = await reponse.json();
        throw data.message;
    }
}

export const services = {
    register: register
}