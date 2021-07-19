import { auth } from "./auth.js"
import { message } from "./messages.js";
import { services } from "./login.services.js";

const loginData = {
    data() {
        return {
            form: {}
        }
    },
    methods: {
        async submit(e) {
            if(!this.form.user || !this.form.password)
                return;

            e.preventDefault();
            
            try {
                const loggedUser = await services.login({
                    user: this.form.user,
                    password: this.form.password
                });
    
                auth.login(loggedUser);
                window.location.href = "app.html";                
            } catch (error) {
                message.error(error);
            }
        }
    }
}

Vue.createApp(loginData).mount("#login");