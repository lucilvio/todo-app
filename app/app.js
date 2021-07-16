const appData = {
    data() {
        return {
            tasks: [],
            filteredTasks: [],
            lists: [],
            taskName: "",
            listName: "",
            routes: [
                { icon: "far fa-clipboard-list-check", title: "Tasks", name: "tasks", action: this.goToTasks },
                { icon: "far fa-bookmark", title: "Important Tasks", name: "importantTasks", action: this.goToImportantTasks },
                { icon: "fa fa-list", title: "List", name: "list", action: this.goToList }
            ],
            selectedRoute: {},
            selectedList: { id: null, name: "" }
        };
    },
    methods: {
        async changesListener() {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(this.settings.signalR + "/change-listener")
                .configureLogging(signalR.LogLevel.Information)
                .build();

            await connection.start();

            connection.on("tasksChanged", () => {
                this.loadTasks();
            });

            connection.on("listsChanged", () => {
                this.loadLists();
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
                const response = await fetch(this.settings.api + "/lists");
                const data = await response.json();

                this.lists = data;
            } catch (error) {
                console.error(error);
            }
        },
        async removeList(id) {
            try {
                await fetch(this.settings.api + "/lists/" + id, { method: "delete" });
                toastr.success("List Deleted!");

                this.setRoute("tasks");
            } catch (error) {
                toastr.error("Error while trying to delete list", "Ooops!");
                console.error(error);
            }
        },
        async loadTasks() {
            let response;

            try {
                response = await fetch(this.settings.api + "/tasks");

                const data = await response.json();
                this.tasks = data;
            } catch (error) {
                toastr.error("Error on tasks loading");
                console.error(error);
            }
        },
        async addTask() {
            let task = {
                name: this.taskName,
                list: this.selectedList ? this.selectedList.id : null
            };
            
            try {
                const response = await fetch(this.settings.api + "/tasks", { method: "post", body: JSON.stringify(task), headers: { "Content-Type": "application/json" } });

                if (response.ok) {
                    toastr.success("Task Registered!")

                    this.taskName = "";
                } else {
                    if (response.status >= 500)
                        throw new Exception();

                    const json = await response.json();
                    toastr.error(json.message, "Ooops!");
                    return;
                }
            } catch (error) {
                toastr.error("Error on task register", "Ooops!");
                console.error(error);
            }
        },
        async checkTask(taskId) {
            await fetch(this.settings.api + "/tasks/" + taskId + "/done", { method: "put" });
        },
        async uncheckTask(taskId) {
            await fetch(this.settings.api + "/tasks/" + taskId + "/undo", { method: "put" });
        },
        async removeTask(taskId) {
            try {
                await fetch(this.settings.api + "/tasks/" + taskId, { method: "delete" });
                toastr.success("Task Deleted!");
            } catch (error) {
                toastr.error("Error while trying to delete task", "Oooops!");
                console.error(error);
            }
        },

        async addList() {
            if (!this.listName)
                return;

            const list = {
                Name: this.listName
            };

            const response = await fetch(this.settings.api + "/lists", { method: "post", body: JSON.stringify(list), headers: { "Content-Type": "application/json" } });

            if (response.ok) {
                toastr.success("Registered!")

                this.listName = "";
            } else {
                if (response.status >= 500)
                    return;

                const json = await response.json();
                toastr.error(json.message, "Ooops!");
                return;
            }
        },
        listTasksCounter(id) {
            return this.tasks.filter(t => t.list === id).length;
        },
        setRoute(route, params) {
            const foundRoute = this.routes.find(r => r.name == route);

            if (!foundRoute) {
                console.error("Route " + route + " not found!");
                return;
            }

            this.selectedRoute = foundRoute;
            this.selectedRoute.action(params);
        },
        goToTasks() {
            this.selectedList = { id: null, name: "" };
            this.filteredTasks = this.tasks.filter(t => !t.list)
        },
        goToImportantTasks() {
            this.selectedList = { id: null, name: "" };
            this.filteredTasks = this.tasks.filter(t => !t.list && t.important);
        },
        goToList(list) {
            this.selectedRoute.title = list.name;
            this.selectedList = list;
            this.filteredTasks = this.tasks.filter(t => t.list === list.id);
        }
    },
    computed: {
        tasksCounter: function () {
            return this.tasks.filter(t => !t.list).length;
        },
        importantTasksCounter: function () {
            return this.tasks.filter(t => !t.list && t.important).length;
        }
    },
    watch: {
        tasks(newValue, oldValue) {
            if (this.selectedRoute.action)
                this.selectedRoute.action(this.selectedList);
        }
    },
    async mounted() {
        await this.loadSettings();
        await this.loadLists();
        await this.loadTasks();
        await this.changesListener();

        await this.setRoute("tasks");
    }
};

Vue.createApp(appData).mount("#app");