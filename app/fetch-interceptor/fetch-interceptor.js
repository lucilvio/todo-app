import { loader } from "../loader/loader.js";

(function() {
    const originFetch = window.fetch;

    window.fetch = function() {
        loader.block();

        return originFetch.apply(this, arguments)
            .then((res) => {
                loader.unblock();
                return res;
            });
    }
})();