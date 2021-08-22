import { loader } from "../loader/loader.js";

(function() {
    const originFetch = window.fetch;

    window.fetch = function() {
        if (!arguments[0].endsWith("/change-listener"))
            loader.block();

        return originFetch.apply(this, arguments)
            .then((response) => {
                loader.unblock();

                if (!response.ok) {
                    if (response.status === 404)
                        throw { errorMessage: "Service not found!" };

                    if (response.status === 500)
                        throw { errorMessage: "Internal Server Error!" };
                }

                const contentType = response.headers.get('content-type');

                if (!contentType || !contentType.includes('application/json'))
                    return Promise.resolve();

                return response.json();
            }).then((jsonData) => {
                if (!jsonData)
                    return;

                if (jsonData.errorMessage)
                    throw jsonData.errorMessage;

                return jsonData;
            }).catch((error) => {
                loader.unblock();
                throw error;
            });
    }
})();