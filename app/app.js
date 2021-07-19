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
            await services.removeList(id);
            this.setRoute("tasks");
        },
        async loadTasks() {
            const loadedTasks = await services.loadTasks();

            if(loadedTasks)
                this.tasks = loadedTasks;
        },
        async addTask() {
            const task = {
                name: this.taskName,
                list: this.selectedList ? this.selectedList.id : null
            };
            
            await services.addTask(task);
        },
        async checkTask(taskId) {
            await services.checkTask(taskId);
        },
        async uncheckTask(taskId) {
            await services.uncheckTask(taskId);
        },
        async markTaskAsImportant(taskId) {
            await services.markTaskAsImportant(taskId);
        },
        async markTaskAsNotImportant(taskId) {
            await services.markTaskAsNotImportant(taskId);
        },
        async removeTask(taskId) {
            await services.removeTask(taskId);
        },

        async addList() {
            if (!this.listName)
                return;

            const list = {
                Name: this.listName
            };

            await services.addList(list);
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