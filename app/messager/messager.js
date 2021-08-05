function ok(message) {
    toastr.success(message, "Yey!");
}

function error(message) {
    toastr.error(message, "OOooops!");
}

export const messager = {
    ok: ok,
    error: error
}