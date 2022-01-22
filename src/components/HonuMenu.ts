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

export const MenuDropdown = Vue.extend({
    template: `
        <li class="nav-item dropdown">
            <menu-image></menu-image>
            <ul class="dropdown-menu mt-0">
                <slot></slot>
            </ul>
        </li>
    `,

    components: { MenuImage }
});

export const MenuRealtime = Vue.extend({
    template: `
        <li class="dropdown-submenu">
            <span class="dropdown-item dropdown-toggle">Realtime</span>
            <ul class="dropdown-menu">
                <li><a class="dropdown-item" href="/view/connery">Connery</a></li>
                <li><a class="dropdown-item" href="/view/cobalt">Cobalt</a></li>
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

export const MenuLedger = Vue.extend({
    template: `
        <li>
            <a class="dropdown-item" href="/ledger">Ledger</a>
        </li>
    `,

    components: { Fragment }
});