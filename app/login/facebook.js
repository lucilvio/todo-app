import { auth } from "../authentication/auth.js"
import { loader } from "../loader/loader.js";

import { services } from "./facebook.services.js";

try {
    loader.block();

    const queryString = window.location.search;
    const code = new URLSearchParams(queryString).get("code");

    if (!code) {
        window.location.href = "/login/login.html";
    }

    const params = await services.login({
        code: code
    });

    auth.login(params);
    window.location.href = "/tasks/tasks.html";
} catch (error) {
    console.log(error);
    // window.location.href = "/login/login.html";
}