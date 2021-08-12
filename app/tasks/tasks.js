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
                .withUrl(settings.signalR)
                .configureLogging(signalR.LogLevel.Trace)
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
            return this.tasks.filter(t => !t.list).length;
        },
        importantTasksCounter: function() {
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
        await this.setRoute("tasks");

        await this.changesListener();
    }
};

Vue.createApp(appData).mount("#app");