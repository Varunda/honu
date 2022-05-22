import Vue from "vue";

import { Fragment } from "vue-fragment";

export const HonuMenu = Vue.extend({
    template: `
        <nav class="navbar navbar-expand p-0">
            <div class="navbar-collapse">
                <ul class="navbar-nav h1">
                    <slot></slot>
                </ul>
            </div>
        </nav>
    `,

    components: { Fragment }
});

export const MenuSep = Vue.extend({
    template: `
        <li class="nav-item h1 p-0 mx-2">/</li>
    `
});

export const MenuImage = Vue.extend({
    template: `
        <li class="nav-item">
            <a class="nav-link dropdown-toggle h1 p-0" href="/" data-toggle="dropdown">
                <img src="/img/beans.png" style="height: 100%; width: 48px;" title="spill 'em" />
                Honu
            </a>
        </li>
    `,

    components: { Fragment }
});

export const MenuHomepage = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/">Homepage</a>
        </li>
    `
});

export const MenuRealtime = Vue.extend({
    template: `
        <li class="dropdown-submenu">
            <span class="dropdown-item dropdown-toggle">Realtime</span>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="/view/cobalt">Cobalt</a></li>
                <li><a class="dropdown-item" href="/view/connery">Connery</a></li>
                <li><a class="dropdown-item" href="/view/emerald">Emerald</a></li>
                <li><a class="dropdown-item" href="/view/jaeger">Jaeger</a></li>
                <li><a class="dropdown-item" href="/view/miller">Miller</a></li>
                <li><a class="dropdown-item" href="/view/soltech">SolTech</a></li>
            </ul>
        </li>
    `,

    components: { Fragment }
});

export const MenuCharacters = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/character">Characters</a>
        </li>
    `,

    components: { Fragment }
});

export const MenuOutfits = Vue.extend({
    template: `
        <li class="dropdown-submenu">
            <span class="dropdown-item dropdown-toggle">Outfits</span>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="/outfitfinder">Search</a></li>
                <li><a class="dropdown-item" href="/report">Report</a></li>
                <li><a class="dropdown-item" href="/outfitpop">Population</a></li>
            </ul>
        </li>
    `,

    components: { Fragment }
});

export const MenuRealtimeNetwork = Vue.extend({
    template: `
        <li class="dropdown-submenu">
            <span class="dropdown-item dropdown-toggle">Realtime Networks</span>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="/realtimenetwork/10">Cobalt</a></li>
                <li><a class="dropdown-item" href="/realtimenetwork/1">Connery</a></li>
                <li><a class="dropdown-item" href="/realtimenetwork/17">Emerald</a></li>
                <li><a class="dropdown-item" href="/realtimenetwork/19">Jaeger</a></li>
                <li><a class="dropdown-item" href="/realtimenetwork/13">Miller</a></li>
                <li><a class="dropdown-item" href="/realtimenetwork/40">SolTech</a></li>
            </ul>
        </li>
    `
});

export const MenuRealTimeMap = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/realtimemap">Realtime Map</a>
        </li>
    `,

    components: { Fragment }
});

export const MenuItems = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/items">Items</a>
        </li>
    `
});

export const MenuLedger = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/ledger">Ledger</a>
        </li>
    `,

    components: { Fragment }
});

export const MenuAlerts = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/alerts">Alerts</a>
        </li>
    `
});

export const MenuDropdown = Vue.extend({
    template: `
        <li class="nav-item dropdown">
            <menu-image></menu-image>
            <ul class="dropdown-menu mt-0">
                <slot>
                    <menu-homepage></menu-homepage>
                    <menu-realtime></menu-realtime>
                    <menu-alerts></menu-alerts>
                    <menu-characters></menu-characters>
                    <menu-outfits></menu-outfits>
                    <menu-ledger></menu-ledger>
                    <menu-real-time-map></menu-real-time-map>
                    <menu-realtime-network></menu-realtime-network>
                    <menu-items></menu-items>
                </slot>
            </ul>
        </li>
    `,

    components: {
        MenuImage, MenuHomepage, MenuRealtime, MenuCharacters, MenuOutfits, MenuLedger, MenuRealTimeMap, MenuItems, MenuAlerts, MenuRealtimeNetwork
    }
});