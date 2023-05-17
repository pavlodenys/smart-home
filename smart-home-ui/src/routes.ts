import { wrap } from "svelte-spa-router/wrap";
import Dashboard from "./components/dashboard/Dashboard.svelte";
import Login from "./components/Login.svelte";
import Home from "./components/Home.svelte";
import NotFound from "./components/NotFound.svelte";
import ForgotPassword from "./components/forgot-password/ForgotPassword.svelte";
import Register from "./components/register/Register.svelte";

export const routes = {
    "/": Home,
    "/dashboard": wrap({
        asyncComponent: () =>
            import("./components/dashboard/Dashboard.svelte"),
        //conditions: [(detail) => routeLoading(detail)],
    }),
    "/sensor": wrap({
        asyncComponent: () =>
            import("./components/sensor-datail/SensorDetail.svelte"),
    }),
    "/sensor/:id": wrap({
        asyncComponent: () =>
            import("./components/sensor-datail/SensorDetail.svelte"),
    }),
    "/register": Register,
    "/forgot-password": ForgotPassword,
    "/login": Login,
    "*": NotFound,
};