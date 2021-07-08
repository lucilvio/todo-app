// const api = "https://vue-todo-app-api.azurewebsites.net/tasks";
const api = "https://localhost:5001/tasks";

const appData = {
    data() {
        return {
            tasks: [],
            taskName: ""
        };
    },
    methods: {
        async loadTasks() {
            const response = await fetch(api);
            const json = await response.json();
            this.tasks = json;
        },
        async addTask() {
            let task = {
                Name: this.taskName
            };

            const response = await fetch(api, { method: "post", body: JSON.stringify(task) });

            if (response.ok) {
                await this.loadTasks();
                toastr.success("Registered!")

                this.taskName = "";
            } else {
                if(response.status >= 500)
                    return;

                const json = await response.json();
                toastr.error(json.message, "Ooops!");
                return;
            }
        },
        async checkTask(taskId) {
            const response = await fetch(api + "/" + taskId + "/done", { method: "put" });
            await this.loadTasks();
        },
        async uncheckTask(taskId) {
            const response = await fetch(api + "/" + taskId + "/undo", { method: "put" });
            await this.loadTasks();
        },
        async removeTask(taskId) {
            this.tasks = this.tasks.filter(t => t.id !== taskId);
            const response = await fetch(api + "/" + taskId, { method: "delete" });

            await this.loadTasks();
            toastr.success("Deleted!");
        }
    },
    mounted() {
        this.loadTasks();
    }
};

Vue.createApp(appData).mount("#app");