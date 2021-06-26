const appData = {
    data() {
        return {
            tasks: [],
            taskName: ""
        };
    },
    methods: {
        addTask() {
            if(!this.taskName.trim())
            {
                toastr.error("It's not possible to do a task without name", "Oops!");
                return;
            }
            if(this.tasks.find(t => t.name === this.taskName))
            {
                toastr.error("Seems you already have a task with this name", "Oops!");
                return;
            }

            this.tasks.push({ id: uuidv4(), name: this.taskName, done: false, date: new Date() });
            this.taskName = "";
        },
        removeTask(taskId) {
            this.tasks = this.tasks.filter(t => t.id !== taskId);
        }   
    }
};

Vue.createApp(appData).mount("#app");