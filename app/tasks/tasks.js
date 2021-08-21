import { settings } from "../settings/settings.js";
import { auth } from "../authentication/auth.js";
import { services } from "./tasks.services.js";
import { messenger } from "../messenger/messenger.js";

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
                { name: "tasks", title: "Tasks", subtitle: "List of your ongoing tasks", action: this.goToTasks },
                { name: "importantTasks", title: "Important Tasks", subtitle: "List of your important tasks", action: this.goToImportantTasks },
                { name: "completedTasks", title: "Completed Tasks", subtitle: "All your completed tasks", action: this.goToCompletedTasks },
                { name: "deletedTasks", title: "Deleted Tasks", subtitle: "List of your deleted tasks", action: this.goToDeletedTasks },
                { name: "list", title: "List", subtitle: "List of your ongoing tasks in this list", action: this.goToList }
            ],
            selectedRoute: {},
            selectedList: { id: null, name: "" }
        };
    },
    methods: {
        async changesListener() {
            const connection = new signalR.HubConnectionBuilder()
                .withUrl(settings.signalR, {
                    skipNegotiation: true,
                    transport: signalR.HttpTransportType.WebSockets
                })
                .configureLogging(signalR.LogLevel.Information)
                .build();

            connection.on("tasksChanged", async() => {
                await this.loadTasks();
            });

            connection.on("listsChanged", async() => {
                await this.loadLists();
            });

            await connection.start();
        },
        async loadLists() {
            this.lists = await this.callService(services.loadLists);
        },
        async removeList(id) {
            await this.callService(services.removeList, { args: [id], successMessage: "List successfully removed" });
        },
        async loadTasks() {
            this.tasks = await this.callService(services.loadTasks);
        },
        async addTask() {
            const task = {
                name: this.taskName,
                list: this.selectedList ? this.selectedList.id : null
            };

            await this.callService(services.addTask, { args: [task], successMessage: "Task added" });
            this.taskName = "";
        },
        async checkTask(taskId) {
            await this.callService(services.checkTask, { args: [taskId] });
        },
        async uncheckTask(taskId) {
            await this.callService(services.uncheckTask, { args: [taskId] });
        },
        async markTaskAsImportant(taskId) {
            await this.callService(services.markTaskAsImportant, { args: [taskId] });
        },
        async markTaskAsNotImportant(taskId) {
            await this.callService(services.markTaskAsNotImportant, { args: [taskId] });
        },
        async removeTask(taskId) {
            await this.callService(services.removeTask, { args: [taskId] });
        },

        async addList() {
            if (!this.listName)
                return;

            const list = {
                Name: this.listName
            };

            await this.callService(services.addList, { args: [list] });
            this.listName = "";
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
            this.filteredTasks = this.tasks.filter(t => !t.list && !t.deleted && !t.done)
        },
        goToImportantTasks() {
            this.selectedList = { id: null, name: "" };
            this.filteredTasks = this.tasks.filter(t => t.important);
        },
        goToCompletedTasks() {
            this.selectedList = { id: null, name: "" };
            this.filteredTasks = this.tasks.filter(t => t.done);
        },
        goToDeletedTasks() {
            this.selectedList = { id: null, name: "" };
            this.filteredTasks = this.tasks.filter(t => t.deleted);
        },
        goToList(list) {
            this.selectedRoute.title = list.name;
            this.selectedList = list;
            this.filteredTasks = this.tasks.filter(t => t.list === list.id);
        },
        logout() {
            auth.logout();
            location.reload();
        },
        async callService(service, config) {
            try {
                const args = config ? config.args : null;
                const successMessage = config ? config.successMessage : "";

                var result = await service.apply(this, args);

                if (successMessage)
                    messenger.ok(successMessage);

                return result;
            } catch (error) {
                messenger.error(error.errorMessage);
            }
        }
    },
    computed: {
        tasksCounter: function() {
            return this.tasks.filter(t => !t.list && !t.deleted && !t.done).length;
        },
        importantTasksCounter: function() {
            return this.tasks.filter(t => t.important).length;
        },
        completedTasksCounter: function() {
            return this.tasks.filter(t => t.done).length;
        },
        deletedTasksCounter: function() {
            return this.tasks.filter(t => t.deleted).length;
        },
        canAddNewTask: function() {
            return this.selectedRoute.name == "tasks" || this.selectedRoute.name == "importantTasks" || this.selectedRoute.name == "list";
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
        await this.setRoute("tasks");

        await this.changesListener();
    }
};

Vue.createApp(appData).mount("#app");