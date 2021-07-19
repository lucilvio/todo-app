import { message } from "./messages.js";
import { services } from "./register.services.js";

const loginData = {
    data() {
        return {
            form: {}
        }
    },
    methods: {
        async submit(e) {            
            e.preventDefault();

            try {
                await services.register({
                    name: this.form.name,
                    email: this.form.email,
                    password: this.form.password
                });
                
                message.ok("User registered!", "Yey");            
                window.location.href = "/login.html";
            } catch (error) {
                message.error(error, "Oooops!");
            }
        }
    }
}

Vue.createApp(loginData).mount("#register");