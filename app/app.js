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
        async changesListener() {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl("https://localhost:5001/tasks")
                .configureLogging(signalR.LogLevel.Information)
                .build();

            await connection.start();

            connection.on("tasksChanged", () => {
                this.loadTasks();
            });
        },
        async loadSettings() {
            try {
                const response = await fetch("./settings.json");
                console.log(response);
                const jsonSettings = await response.json();
                this.settings = jsonSettings;                
            } catch (error) {
                console.error(error);
            }
        },
        async loadLists() {
            try {
                console.log(this.settings.api);
                const response = await fetch(this.settings.api + "/lists");
                const json = await response.json();
                this.lists = json;
            } catch (error) {
                console.error(error);
            }
        },
        async loadTasks() {
            const response = await fetch(this.settings.api + "/lists/" + this.selectedList.id + "/tasks");
            const json = await response.json();
            this.tasks = json;
        },
        async addTask() {
            let task = {
                name: this.taskName
            };

            const response = await fetch(this.settings.api + "/lists/" + this.selectedList.id + "/tasks", { method: "post", body: JSON.stringify(task), headers: { "Content-Type" : "application/json" }});

            if (response.ok) {
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
            const response = await fetch(this.settings.api + "/lists/" + this.selectedList.id + "/tasks/" + taskId + "/done", { method: "put" });
        },
        async uncheckTask(taskId) {
            const response = await fetch(this.settings.api + "/lists/" + this.selectedList.id + "/tasks/" + taskId + "/undo", { method: "put" });
        },
        async removeTask(taskId) {            
            const response = await fetch(this.settings.api + "/lists/" + this.selectedList.id + "/tasks/" + taskId, { method: "delete" });            

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

            const response = await fetch(this.settings.api + "/lists", { method: "post", body: JSON.stringify(list), headers: { "Content-Type" : "application/json" }});

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
        await this.loadSettings();
        await this.loadLists();
        this.selectedList = this.lists[0];
        
        await this.loadTasks();

        await this.changesListener();
    }
};

Vue.createApp(appData).mount("#app");