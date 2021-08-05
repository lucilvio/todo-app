const cacheKey = "vue-todoapp-settings";

function loadCachedSettings() {
    return localStorage.getItem(cacheKey);
}

function saveSettingsToCache(settings) {
    localStorage.setItem(cacheKey, JSON.stringify(settings));
}

async function loadSettings() {
    const cachedSettings = loadCachedSettings();

    if (cachedSettings)
        return JSON.parse(cachedSettings);

    const settingsFile = "settings.json";

    try {

        const response = await fetch("../" + settingsFile);
        const data = await response.json();

        saveSettingsToCache(data);

        return data;
    } catch (error) {
        console.error("Error while trying to load settings on " + settingsFile + ". Details: " + error);
    }
}

export const settings = await loadSettings();