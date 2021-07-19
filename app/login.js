import { settings } from "./settings.js";
import { auth } from "./auth.js";

const loginData = {
    data() {
        return {
            user: "",
            password: ""
        }
    },
    methods: {
        async login() {
            const request = {
                user: this.user,
                password: this.password
            };

            try {
                const reponse = await fetch(settings.api + "/token", {
                    method: "post",
                    body: JSON.stringify(request),
                    headers: { 
                        "Authorization": "Bearer " + this.user.token,
                        "Content-Type": "application/json",
                        "iss": settings.app,
                        "aud": settings.host
                    }
                });
    
                if(!reponse.ok)
                {
                    if(reponse.status === 404)
                        return;
                    
                    var data = await reponse.json();
                    toastr.error(data.message, "Oooops!");
                    return;
                }

                var data = await reponse.json();
    
                auth.login(data);
                window.location.href = "app.html";                
            } catch (error) {
                toastr.error("Error while trying to login. Details: " + error);
            }
        }
    }
}

Vue.createApp(loginData).mount("#login");