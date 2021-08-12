import { settings } from "../settings/settings.js";
import { auth } from "../authentication/auth.js";

const user = auth.getUser();

async function loadLists() {
    return await fetch(settings.api + "/lists", {
        method: "get",
        headers: { "Authorization": "Bearer " + user.token }
    });
}

async function removeList(id) {
    await fetch(settings.api + "/lists/" + id, {
        method: "delete",
        headers: { "Authorization": "Bearer " + user.token }
    });
}

async function loadTasks() {
    return await fetch(settings.api + "/tasks", {
        method: "get",
        headers: { "Authorization": "Bearer " + user.token }
    });
}

async function addTask(task) {
    await fetch(settings.api + "/tasks", {
        method: "post",
        body: JSON.stringify(task),
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + user.token
        }
    });
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
    await fetch(settings.api + "/tasks/" + taskId, {
        method: "delete",
        headers: { "Authorization": "Bearer " + user.token }
    });
}

async function addList(list) {
    await fetch(settings.api + "/lists", {
        method: "post",
        body: JSON.stringify(list),
        headers: {
            "Content-Type": "application/json",
            "Authorization": "Bearer " + user.token
        }
    });
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