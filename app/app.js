const api = "https://localhost:5001";
// const api = "https://vue-todo-app-api.azurewebsites.net";

const appData = {
    data() {
        return {
            tasks: [],
            lists: [],
            taskName: "",
            listName: "",
            selectedList: { name: "" }
        };
    },
    methods: {
        async loadLists() {
            const response = await fetch(api + "/lists");
            const json = await response.json();
            this.lists = json;
        },
        async loadTasks() {
            const response = await fetch(api + "/lists/" + this.selectedList.id + "/tasks");
            const json = await response.json();
            this.tasks = json;
        },
        async addTask() {
            let task = {
                name: this.taskName
            };

            const response = await fetch(api + "/lists/" + this.selectedList.id + "/tasks", { method: "post", body: JSON.stringify(task), headers: { "Content-Type" : "application/json" }});

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
            const response = await fetch(api + "/lists/" + this.selectedList.id + "/tasks/" + taskId + "/done", { method: "put" });
            await this.loadTasks();
        },
        async uncheckTask(taskId) {
            const response = await fetch(api + "/lists/" + this.selectedList.id + "/tasks/" + taskId + "/undo", { method: "put" });
            await this.loadTasks();
        },
        async removeTask(taskId) {            
            const response = await fetch(api + "/lists/" + this.selectedList.id + "/tasks/" + taskId, { method: "delete" });            
            await this.loadTasks();

            toastr.success("Deleted!");
        },
        async changeList(list){
            this.selectedList = list;
            await this.loadTasks();
        },
        async addList() {
            if(!this.listName)
                return;

            const list = {
                Name: this.listName
            };

            const response = await fetch(api + "/lists", { method: "post", body: JSON.stringify(list), headers: { "Content-Type" : "application/json" }});

            if (response.ok) {                
                toastr.success("Registered!")
                
                this.listName = "";
                await this.loadLists();
            } else {
                if(response.status >= 500)
                    return;

                const json = await response.json();
                toastr.error(json.message, "Ooops!");
                return;
            } 
        }
    },
    async mounted() {
        await this.loadLists();
        this.selectedList = this.lists[0];

        await this.loadTasks();
    }
};

Vue.createApp(appData).mount("#app");