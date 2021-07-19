import { settings }  from "./settings.js";
import { auth } from "./auth.js";

const user = auth.getUser();

async function loadLists() {
    try {
        const response = await fetch(settings.api + "/lists", {
            method: "get",                    
            headers: { "Authorization": "Bearer " + user.token }
        });

        if(!response.ok)
            return;

        return await response.json();
    } catch (error) {
        console.error(error, "Ooops!");
    }
}

async function removeList(id) {
    try {
        await fetch(settings.api + "/lists/" + id, { 
            method: "delete",
            headers: { "Authorization": "Bearer " + user.token }
        });

        toastr.success("List Deleted!");        
    } catch (error) {
        toastr.error("Error while trying to delete list", "Ooops!");
        console.error(error, "Ooops!");
    }
}

async function loadTasks() {
    let response;

    try {
        response = await fetch(settings.api + "/tasks", {
            method: "get",                    
            headers: { "Authorization": "Bearer " + user.token }
        });

        if(!response.ok)
            return;

        return await response.json();
    } catch (error) {
        toastr.error("Error on tasks loading");
        console.error(error, "Ooops!");
    }
}

async function addTask(task) {
    try {
        const response = await fetch(settings.api + "/tasks", { 
            method: "post", 
            body: JSON.stringify(task), 
            headers: { "Content-Type": "application/json", 
                "Authorization": "Bearer " + user.token } 
        });

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
}

async function checkTask(taskId) {
    await fetch(settings.api + "/tasks/" + taskId + "/done", { 
        method: "put",
        headers: { "Authorization": "Bearer " + user.token } 
    });
}

async function uncheckTask(taskId) {
    await fetch(settings.api + "/tasks/" + taskId + "/undo", { 
        method: "put",
        headers: { "Authorization": "Bearer " + user.token } 
    });
}

async function markTaskAsImportant(taskId) {
    await fetch(settings.api + "/tasks/" + taskId + "/important", { 
        method: "put",
        headers: { "Authorization": "Bearer " + user.token } 
    });
}

async function markTaskAsNotImportant(taskId) {
    await fetch(settings.api + "/tasks/" + taskId + "/not-important", { 
        method: "put",
        headers: { "Authorization": "Bearer " + user.token } 
    });
}

async function removeTask(taskId) {
    try {
        await fetch(settings.api + "/tasks/" + taskId, { 
            method: "delete",
            headers: { "Authorization": "Bearer " + user.token } 
        });
        toastr.success("Task Deleted!");
    } catch (error) {
        toastr.error("Error while trying to delete task", "Oooops!");
        console.error(error);
    }
}

async function addList(list) {
    const response = await fetch(settings.api + "/lists", { 
        method: "post", 
        body: JSON.stringify(list), 
        headers: { "Content-Type": "application/json",
            "Authorization": "Bearer " + user.token }
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
}

export const services = {
    addList: addList,
    loadLists: loadLists,
    removeList: removeList,
    addTask: addTask,
    loadTasks: loadTasks,
    removeTask: removeTask,
    checkTask: checkTask,
    uncheckTask: uncheckTask,
    markTaskAsImportant: markTaskAsImportant,
    markTaskAsNotImportant: markTaskAsNotImportant
}