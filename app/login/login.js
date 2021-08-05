import { auth } from "../authentication/auth.js"
import { messager } from "../messager/messager.js";
import { loader } from "../loader/loader.js";

import { services } from "./login.services.js";

const loginData = {
    data() {
        return {
            form: {}
        }
    },
    methods: {
        async submit(e) {
            loader.block();

            if (!this.form.user || !this.form.password)
                return;

            e.preventDefault();

            try {
                const loggedUser = await services.login({
                    user: this.form.user,
                    password: this.form.password
                });

                auth.login(loggedUser);
                window.location.href = "/tasks/tasks.html";
            } catch (error) {
                loader.unblock();
                messager.error(error);
            }
        }
    }
}

Vue.createApp(loginData).mount("#login");