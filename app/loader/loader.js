const loaderElement = ".loader";

function block() {
    const loader = document.querySelector(loaderElement);

    if (!loader)
        return;

    loader.style.display = "block";
}

function unblock() {
    const loader = document.querySelector(loaderElement);

    if (!loader)
        return;

    loader.style.display = "none";
}

export const loader = {
    block: block,
    unblock: unblock
}