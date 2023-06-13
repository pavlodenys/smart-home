import { wrap } from "svelte-spa-router/wrap";
import Dashboard from "./components/dashboard/Dashboard.svelte";
import Login from "./components/Login.svelte";
import Home from "./components/Home.svelte";
import NotFound from "./components/NotFound.svelte";
import ForgotPassword from "./components/forgot-password/ForgotPassword.svelte";
import Register from "./components/register/Register.svelte";
import Reports from "./components/reports/Reports.svelte";
import Help from "./components/help/Help.svelte";
import { wrapAuth } from './lib/AuthRoute';

export const routes = {
    "/": Home,
    "/dashboard": wrapAuth(() =>
        import("./components/dashboard/Dashboard.svelte")),
    "/sensor": wrapAuth(() =>
            import("./components/sensor-datail/SensorDetail.svelte"),
    ),
    "/sensor/:id": wrapAuth(() =>
            import("./components/sensor-datail/SensorDetail.svelte"),
    ),
    "/reports": wrapAuth(() =>
        import("./components/reports/Reports.svelte"),
    ),
    "/help": Help,
    "/register": Register,
    "/forgot-password": ForgotPassword,
    "/login": Login,
    "*": NotFound,
};
