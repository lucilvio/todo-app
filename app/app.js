import { settings }  from "./settings.js";
import { auth } from "./auth.js";
import { services } from "./app.services.js";

const appData = {
    data() {
        return {
            user: {},
            tasks: [],
            filteredTasks: [],
            lists: [],
            taskName: "",
            listName: "",
            routes: [
                { icon: "far fa-clipboard-list-check", title: "Tasks", name: "tasks", action: this.goToTasks },
                { icon: "fas fa-bookmark", title: "Important Tasks", name: "importantTasks", action: this.goToImportantTasks },
                { icon: "fa fa-list", title: "List", name: "list", action: this.goToList }
            ],
            selectedRoute: {},
            selectedList: { id: null, name: "" }
        };
    },
    methods: {
        async changesListener() {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(settings.signalR + "/change-listener")
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
        async loadLists() {
            this.lists = await services.loadLists();
        },
        async removeList(id) {
            try {
                await fetch(settings.api + "/lists/" + id, { 
                    method: "delete",
                    headers: { "Authorization": "Bearer " + this.user.token }
                });
                toastr.success("List Deleted!");

                this.setRoute("tasks");
            } catch (error) {
                toastr.error("Error while trying to delete list", "Ooops!");
                console.error(error, "Ooops!");
            }
        },
        async loadTasks() {
            this.tasks = await services.loadTasks();
        },
        async addTask() {
            const task = {
                name: this.taskName,
                list: this.selectedList ? this.selectedList.id : null
            };
            
            await services.addTask(task);
        },
        async checkTask(taskId) {
            await fetch(settings.api + "/tasks/" + taskId + "/done", { 
                method: "put",
                headers: { "Authorization": "Bearer " + this.user.token } 
            });
        },
        async uncheckTask(taskId) {
            await fetch(settings.api + "/tasks/" + taskId + "/undo", { 
                method: "put",
                headers: { "Authorization": "Bearer " + this.user.token } 
            });
        },
        async markTaskAsImportant(taskId) {
            await fetch(settings.api + "/tasks/" + taskId + "/important", { 
                method: "put",
                headers: { "Authorization": "Bearer " + this.user.token } 
            });
        },
        async markTaskAsNotImportant(taskId) {
            await fetch(settings.api + "/tasks/" + taskId + "/not-important", { 
                method: "put",
                headers: { "Authorization": "Bearer " + this.user.token } 
            });
        },
        async removeTask(taskId) {
            try {
                await fetch(settings.api + "/tasks/" + taskId, { 
                    method: "delete",
                    headers: { "Authorization": "Bearer " + this.user.token } 
                });
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

            const response = await fetch(settings.api + "/lists", { 
                method: "post", 
                body: JSON.stringify(list), 
                headers: { "Content-Type": "application/json",
                    "Authorization": "Bearer " + this.user.token }
            });                

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
            this.filteredTasks = this.tasks.filter(t => t.important);
        },
        goToList(list) {
            this.selectedRoute.title = list.name;
            this.selectedList = list;
            this.filteredTasks = this.tasks.filter(t => t.list === list.id);
        },
        logout() {
            auth.logout();
            location.reload();
        }
    },
    computed: {
        tasksCounter: function () {
            return this.tasks.filter(t => !t.list).length;
        },
        importantTasksCounter: function () {
            return this.tasks.filter(t => t.important).length;
        }
    },
    watch: {
        tasks(newValue, oldValue) {
            if (this.selectedRoute.action)
                this.selectedRoute.action(this.selectedList);
        }
    },
    async mounted() {
        this.user = auth.getUser();

        await this.loadLists();
        await this.loadTasks();
        await this.changesListener();

        await this.setRoute("tasks");
    }
};

Vue.createApp(appData).mount("#app");