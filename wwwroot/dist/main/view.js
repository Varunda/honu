/******/ (() => { // webpackBootstrap
/******/ 	"use strict";
/******/ 	var __webpack_modules__ = ({

/***/ "./src/BlockView.ts":
/*!**************************!*\
  !*** ./src/BlockView.ts ***!
  \**************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! vue */ "vue");
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(vue__WEBPACK_IMPORTED_MODULE_0__);

vue__WEBPACK_IMPORTED_MODULE_0___default().component("block-view", {
    props: {
        block: { required: true },
        title: { type: String, required: false, default: "Player" },
    },
    data: function () {
        return {};
    },
    methods: {},
    template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th>{{title}}</th>
					<th style="width: 12ch">Amount</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name">{{entry.name}}</td>
					<td>
						{{entry.value}} / 
						{{(entry.value / block.total * 100).toFixed(2)}}%
					</td>
				</tr>
				<tr class="table-secondary">
					<th colspan="2">
						Total: {{block.total}}
					</th>
				</tr>
			</tbody>
		</table>
	`
});


/***/ }),

/***/ "./src/KillData.ts":
/*!*************************!*\
  !*** ./src/KillData.ts ***!
  \*************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! vue */ "vue");
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(vue__WEBPACK_IMPORTED_MODULE_0__);

vue__WEBPACK_IMPORTED_MODULE_0___default().component("player-kill-block", {
    props: {
        block: { required: true },
        title: { type: String, required: false, default: "Player" },
    },
    data: function () {
        return {};
    },
    methods: {},
    template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th style="width: 40ch">Player</th>
					<th>Kills</th>
					<th>Deaths</th>
					<th>K/D</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name">{{entry.name}}</td>
					<td>{{entry.kills}}</td>
					<td>{{entry.deaths}}</td>
					<td>
						{{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
					</td>
				</tr>
				<tr class="table-secondary">
				</tr>
			</tbody>
		</table>
	`
});


/***/ }),

/***/ "./src/OutfitKillData.ts":
/*!*******************************!*\
  !*** ./src/OutfitKillData.ts ***!
  \*******************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! vue */ "vue");
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(vue__WEBPACK_IMPORTED_MODULE_0__);

vue__WEBPACK_IMPORTED_MODULE_0___default().component("outfit-kill-block", {
    props: {
        block: { required: true },
        title: { type: String, required: false, default: "Player" },
    },
    data: function () {
        return {};
    },
    methods: {},
    template: `
		<table class="wt-block table table-sm">
			<thead>
				<tr class="table-secondary">
					<th style="width: 40ch">Outfit</th>
					<th>Kills (Avg)</th>
					<th>Deaths (Avg)</th>
					<th>K/D</th>
					<th>Kills</th>
					<th>Deaths</th>
					<th>Players</th>
				</tr>
			</thead>

			<tbody>
				<tr v-for="entry in block.entries">
					<td :title="entry.name">[{{entry.tag}}] {{entry.name}}</td>
					<td>{{entry.kills / entry.members}}</td>
					<td>{{entry.deaths / entry.members}}</td>
					<td>
						{{(entry.kills / (entry.deaths || 1)).toFixed(2)}}
					</td>
					<td>{{entry.kills}}</td>
					<td>{{entry.deaths}}</td>
					<td>{{entry.members}}</td>
				</tr>
			</tbody>
		</table>
	`
});


/***/ }),

/***/ "./src/WorldData.ts":
/*!**************************!*\
  !*** ./src/WorldData.ts ***!
  \**************************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony export */ __webpack_require__.d(__webpack_exports__, {
/* harmony export */   "BlockEntry": () => /* binding */ BlockEntry,
/* harmony export */   "Block": () => /* binding */ Block,
/* harmony export */   "KillData": () => /* binding */ KillData,
/* harmony export */   "OutfitKillData": () => /* binding */ OutfitKillData,
/* harmony export */   "KillBlock": () => /* binding */ KillBlock,
/* harmony export */   "OutfitKillBlock": () => /* binding */ OutfitKillBlock,
/* harmony export */   "FactionData": () => /* binding */ FactionData,
/* harmony export */   "WorldData": () => /* binding */ WorldData
/* harmony export */ });
class BlockEntry {
    constructor() {
        this.id = "";
        this.name = "";
        this.value = 0;
    }
}
class Block {
    constructor() {
        this.name = "";
        this.entires = [];
        this.total = 0;
    }
}
class KillData {
    constructor() {
        this.id = "";
        this.name = "";
        this.kills = 0;
        this.deaths = 0;
    }
}
class OutfitKillData {
    constructor() {
        this.id = "";
        this.factionId = "";
        this.tag = null;
        this.name = "";
        this.kills = 0;
        this.deaths = 0;
        this.members = 0;
    }
}
class KillBlock {
    constructor() {
        this.entries = [];
    }
}
class OutfitKillBlock {
    constructor() {
        this.entires = [];
    }
}
class FactionData {
    constructor() {
        this.factionID = "";
        this.factionName = "";
        this.playerKills = new KillBlock();
        this.outfitKills = new OutfitKillBlock();
        this.outfitHeals = new Block();
        this.outfitResupplies = new Block();
        this.outfitRevives = new Block();
        this.playerHeals = new Block();
        this.playerResupplies = new Block();
        this.playerRevives = new Block();
    }
}
class WorldData {
    constructor() {
        this.worldID = "";
        this.worldName = "";
        this.nc = new FactionData();
        this.tr = new FactionData();
        this.vs = new FactionData();
    }
}


/***/ }),

/***/ "./src/main.ts":
/*!*********************!*\
  !*** ./src/main.ts ***!
  \*********************/
/***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

__webpack_require__.r(__webpack_exports__);
/* harmony import */ var signalR__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! signalR */ "signalR");
/* harmony import */ var signalR__WEBPACK_IMPORTED_MODULE_0___default = /*#__PURE__*/__webpack_require__.n(signalR__WEBPACK_IMPORTED_MODULE_0__);
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! vue */ "vue");
/* harmony import */ var vue__WEBPACK_IMPORTED_MODULE_1___default = /*#__PURE__*/__webpack_require__.n(vue__WEBPACK_IMPORTED_MODULE_1__);
/* harmony import */ var _WorldData__WEBPACK_IMPORTED_MODULE_2__ = __webpack_require__(/*! ./WorldData */ "./src/WorldData.ts");
/* harmony import */ var _BlockView__WEBPACK_IMPORTED_MODULE_3__ = __webpack_require__(/*! ./BlockView */ "./src/BlockView.ts");
/* harmony import */ var _KillData__WEBPACK_IMPORTED_MODULE_4__ = __webpack_require__(/*! ./KillData */ "./src/KillData.ts");
/* harmony import */ var _OutfitKillData__WEBPACK_IMPORTED_MODULE_5__ = __webpack_require__(/*! ./OutfitKillData */ "./src/OutfitKillData.ts");






const vm = new (vue__WEBPACK_IMPORTED_MODULE_1___default())({
    el: "#app",
    created: function () {
        this.socketState = "unconnected";
        const conn = new signalR__WEBPACK_IMPORTED_MODULE_0__.HubConnectionBuilder()
            .withUrl("/ws/data")
            .withAutomaticReconnect([5000, 10000, 20000, 20000])
            .build();
        conn.on("DataUpdate", (data) => {
            this.worldData = JSON.parse(data);
            this.lastUpdate = new Date();
        });
        conn.start().then(() => {
            this.socketState = "opened";
        }).catch(err => {
            console.error(err);
        });
        conn.onreconnected(() => { this.socketState = "opened"; });
        conn.onclose((err) => {
            this.socketState = "closed";
            if (err) {
                console.error("onclose: ", err);
            }
        });
        conn.onreconnecting((err) => {
            this.socketState = "reconnecting";
            if (err) {
                console.error("onreconnecting: ", err);
            }
        });
    },
    data: {
        worldData: new _WorldData__WEBPACK_IMPORTED_MODULE_2__.WorldData(),
        socketState: "",
        lastUpdate: new Date()
    },
    methods: {}
});
window.vm = vm;


/***/ }),

/***/ "vue":
/*!**********************!*\
  !*** external "Vue" ***!
  \**********************/
/***/ ((module) => {

module.exports = Vue;

/***/ }),

/***/ "signalR":
/*!**************************!*\
  !*** external "signalR" ***!
  \**************************/
/***/ ((module) => {

module.exports = signalR;

/***/ })

/******/ 	});
/************************************************************************/
/******/ 	// The module cache
/******/ 	var __webpack_module_cache__ = {};
/******/ 	
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/ 		// Check if module is in cache
/******/ 		if(__webpack_module_cache__[moduleId]) {
/******/ 			return __webpack_module_cache__[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = __webpack_module_cache__[moduleId] = {
/******/ 			// no module.id needed
/******/ 			// no module.loaded needed
/******/ 			exports: {}
/******/ 		};
/******/ 	
/******/ 		// Execute the module function
/******/ 		__webpack_modules__[moduleId](module, module.exports, __webpack_require__);
/******/ 	
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/ 	
/************************************************************************/
/******/ 	/* webpack/runtime/compat get default export */
/******/ 	(() => {
/******/ 		// getDefaultExport function for compatibility with non-harmony modules
/******/ 		__webpack_require__.n = (module) => {
/******/ 			var getter = module && module.__esModule ?
/******/ 				() => module['default'] :
/******/ 				() => module;
/******/ 			__webpack_require__.d(getter, { a: getter });
/******/ 			return getter;
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/define property getters */
/******/ 	(() => {
/******/ 		// define getter functions for harmony exports
/******/ 		__webpack_require__.d = (exports, definition) => {
/******/ 			for(var key in definition) {
/******/ 				if(__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
/******/ 					Object.defineProperty(exports, key, { enumerable: true, get: definition[key] });
/******/ 				}
/******/ 			}
/******/ 		};
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/hasOwnProperty shorthand */
/******/ 	(() => {
/******/ 		__webpack_require__.o = (obj, prop) => Object.prototype.hasOwnProperty.call(obj, prop)
/******/ 	})();
/******/ 	
/******/ 	/* webpack/runtime/make namespace object */
/******/ 	(() => {
/******/ 		// define __esModule on exports
/******/ 		__webpack_require__.r = (exports) => {
/******/ 			if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 				Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 			}
/******/ 			Object.defineProperty(exports, '__esModule', { value: true });
/******/ 		};
/******/ 	})();
/******/ 	
/************************************************************************/
/******/ 	// startup
/******/ 	// Load entry module
/******/ 	__webpack_require__("./src/main.ts");
/******/ 	// This entry module used 'exports' so it can't be inlined
/******/ })()
;