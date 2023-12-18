import Vue, { VNode, VNodeData, CreateElement } from "vue";

//import DateTimePicker from "components/DateTimePicker.vue";
import { Loading, Loadable, ProblemDetails } from "Loading";

import Busy from "components/Busy.vue";
import { PropType } from "vue";

const ValidFilterTypes: string[] = [ "string", "number", "date", "boolean" ];

interface ConditionSettings {
    title: string;
    icon: string;
    color: string;
};

const Conditions: Map<string, ConditionSettings> = new Map([
    ["equals", { title: "Equals", icon: "fa-equals", color: "primary" }],
    ["not_equal", { title: "Not equal", icon: "fa-not-equal", color: "warning" }],
    ["less_than", { title: "Less than", icon: "fa-less-than", color: "info" }],
    ["greater_than", { title: "Greater than", icon: "fa-greater-than", color: "success" }],
    ["contains", { title: "Contains", icon: "fa-asterisk", color: "info" }],
    ["not_empty", { title: "Not empty", icon: "fa-circle", color: "info" }],
    ["empty", { title: "Empty", icon: "fa-empty-set", color: "info" }]
]);

interface Header {
    empty: boolean;
    colClass: string;
    children: VNode[] | undefined;
    field: string | undefined;
};

interface Filter {
    value: any;

    selectedCondition: string;
    colClass: string;
    placeholder: string | undefined;

    conditions: string[];
    method: string;
    type: string;
    field: string;

    source: undefined | FilterKeyValue[];
    sourceKey: string | undefined;
    sourceValue: string | undefined;

    width: string | undefined;
    vnode: VNode | undefined;
};

interface Footer {
    children: VNode[] | undefined;
};

export type FilterKeyValue = {
    key: string,
    value: any
}

export interface ATableType {

    applyFilter(index: number | string, parameters: Partial<Filter>): void;

    resetFilters(): void;

}

export const ATable = Vue.extend({
    props: {
        // Where the data comes from
        entries: { type: Object, required: true },

        // After data is bound this function is called
        PostProcess: { type: Function, required: false, default: undefined },

        // Will the header be displayed?
        ShowHeader: { type: Boolean, required: false, default: true },

        // Will the filters be displayed?
        ShowFilters: { type: Boolean, required: false, default: false },

        // Will the footer be displayed?
        ShowFooter: { type: Boolean, required: false, default: false },

        // Will data be split up into many pages?
        paginate: { type: Boolean, required: false, default: true },

        // How much padding will each row get
        RowPadding: { type: String, required: false, default: "normal" }, // "compact" | "normal" | "expanded"

        // Will the <a-table> be displayed as a <div>.list-group or a <table>
        //DisplayType: { type: String, required: false, default: "list" }, // "list" | "table",

        // Field to sort on by default, if undefined goes to first <a-col> with a sort-field
        DefaultSortField: { type: String, required: false, default: undefined },

        // Order to sort by default
        DefaultSortOrder: { type: String, required: false, default: "asc" },

        // Will the resulting table be rendered with .table-striped or no
        striped: { type: Boolean, required: false, default: true },

        // Will the resulting table be rendered with .table-hover or no
        hover: { type: Boolean, required: false, default: false },

        PageSizes: { type: Array as PropType<number[]>, required: false },

        DefaultPageSize: { type: Number, required: false },

        // Will the pages also be shown above the data?
        ShowTopPages: { type: Boolean, required: false, default: false },

        name: { type: String, required: false, default: null }
    },

    data: function() {
        return {
            ID: Math.floor(Math.random() * 100000),

            nodes: {
                columns: [] as VNode[],
                headers: [] as Header[],
                footers: [] as Footer[]
            },

            sorting: {
                field: "" as string,
                type: "unknown" as "string" | "number" | "date" | "boolean" | "unknown",
                order: "asc" as "asc" | "desc"
            },

            filters: [] as Filter[],

            paging: {
                size: this.DefaultPageSize || 50 as number,
                page: 0 as number
            },
        }
    },

    created: function(): void {
        this.nodes.columns = (this.$slots["default"] || [])
            .filter((iter: VNode) => iter.tag != undefined && iter.componentOptions != undefined);

        console.log(`<a-table>: Found ${this.nodes.columns.length} columns`);

        if (this.DefaultSortField != undefined) {
            this.sorting.field = this.DefaultSortField;
        }
        if (this.DefaultSortOrder == "asc" || this.DefaultSortOrder == "desc") {
            this.sorting.order = this.DefaultSortOrder;
        } else {
            throw `default-sort-order must be 'asc' | 'desc': Is '${this.DefaultSortOrder}'`;
        }

        // Iterate through all the columns and find any <a-header> elements
        for (const column of this.nodes.columns) {
            if (column.componentOptions?.children) {
                // Finding the <a-header> elements
                const headerNodes: VNode[] = column.componentOptions.children
                    .filter((iter: VNode) => iter.componentOptions?.tag == "a-header");

                // Ensure at most one exists for each column, cannot have multiple
                if (headerNodes.length > 1) {
                    throw "Cannot define multiple <a-header> elements in a single <a-col>";
                }

                // Get the HTML class used to define the columns
                const colClass: string = (column.componentOptions!.propsData as any).ColClass;
                const sortField: string | undefined = (column.componentOptions.propsData as any).SortField;

                const options: VNodeData = {};
                options.staticClass = colClass;

                if (sortField != undefined) {
                    options.on = {
                        click: this.sortTable.bind(this, sortField)
                    }
                }

                const header: Header = {
                    colClass: colClass,
                    field: sortField,
                    empty: true,
                    children: []
                };

                // A child <a-header> exists, use those options given
                if (headerNodes.length == 1) {
                    header.children = headerNodes[0].componentOptions?.children ?? [];
                    header.empty = false;

                    // Sort by the first field set
                    if (sortField != undefined && this.sorting.field == "") {
                        this.sorting.field = sortField;
                    }
                }

                this.nodes.headers.push(header);
            }
        }

        // Find the filters and create them
        for (const column of this.nodes.columns) {
            if (!column.componentOptions?.children) {
                continue;
            }

            const filterNodes: VNode[] = column.componentOptions.children
                .filter((iter: VNode) => iter.componentOptions?.tag == "a-filter");

            if (filterNodes.length > 1) {
                throw `Cannot define multiple <a-filter> elements in a single <a-col>`;
            }

            const colClass: string = (column.componentOptions.propsData as any).ColClass;

            const filter: Filter = {
                method: "empty",
                type: "empty",
                conditions: [],
                selectedCondition: "",
                colClass: colClass,
                field: "",
                value: "",
                source: undefined,
                sourceKey: undefined,
                sourceValue: undefined,
                placeholder: undefined,
                vnode: undefined,
                width: undefined
            };

            if (filterNodes.length == 0) {
                this.filters.push(filter);
                continue;
            }

            const filterNode: VNode = filterNodes[0];
            filter.vnode = filterNode;

            filter.width = (filterNode.componentOptions!.propsData as any).MaxWidth;

            // Validate method prop
            filter.method = (filterNode.componentOptions!.propsData as any).method;
            if (typeof filter.method != "string") {
                throw `Needed string for method of <a-filter>, got ${typeof filter.method}`;
            }
            if (filter.method == "reset" || filter.method == "template") {
                this.filters.push(filter);
                continue;
            }

            // Validate a correct source for a dropdown filter
            if (filter.method == "dropdown") {
                filter.source = (filterNode.componentOptions!.propsData as any).source;

                // A function was passed as the source, validate and begin the
                if (typeof (filter.source) == "function") {
                    filter.sourceKey = (filterNode.componentOptions!.propsData as any).SourceKey;
                    if (filter.sourceKey == undefined) {
                        throw `Missing source-key for <a-filter>`;
                    }

                    filter.sourceValue = (filterNode.componentOptions!.propsData as any).SourceValue;
                    if (filter.sourceValue == undefined) {
                        throw `Missing source-value for <a-filter>`;
                    }

                    const sourceRet: any = (filter.source as Function)();
                    if (typeof (sourceRet.ok) != "function") {
                        throw `Missing ok callback handler or is not a function. Did you pass a function that returns an ApiResponse?`;
                    }
                } else if (filter.source != undefined && Array.isArray(filter.source) == false) {
                    throw `<a-filter> source was given but was not an array`;
                }
            }

            // Validate type prop
            filter.type = (filterNode.componentOptions!.propsData as any).type;
            if (typeof filter.type != "string") {
                throw `Needed string for type of <a-filter>, got ${typeof filter.type}`;
            }
            if (ValidFilterTypes.indexOf(filter.type) == -1) {
                throw `Invalid filter type '${filter.type}'`;
            }

            // Validate conditions prop
            filter.conditions = (filterNode.componentOptions!.propsData as any).conditions;
            if (!Array.isArray(filter.conditions) && filter.method != "empty") {
                throw `Needed array for conditions of <a-filter>, got ${typeof filter.conditions} on field ${filter.field}`;
            }
            if (filter.conditions.length == 0) {
                throw `No conditions of <a-filter> given, need at least one`;
            }

            // Validate field prop
            filter.field = (filterNode.componentOptions!.propsData as any).field;
            if (typeof filter.field != "string") {
                throw `Need string for field of <a-filter>, got ${typeof filter.field}`;
            }

            filter.placeholder = (filterNode.componentOptions!.propsData as any).placeholder;

            filter.value = (filter.type == "number") ? null : "";
            filter.selectedCondition = filter.conditions[0];

            this.filters.push(filter);
        }

        for (const column of this.nodes.columns) {
            if (column.componentOptions?.children) {
                // Finding the <a-footer> elements
                const footerNodes: VNode[] = column.componentOptions.children
                    .filter((iter: VNode) => iter.componentOptions?.tag == "a-footer");

                // Ensure at most one exists for each column, cannot have multiple
                if (footerNodes.length > 1) {
                    throw "Cannot define multiple <a-footer> elements in a single <a-col>";
                }

                const footer: Footer = {
                    children: []
                };

                // A child <a-header> exists, use those options given
                if (footerNodes.length == 1) {
                    footer.children = footerNodes[0].componentOptions?.children ?? [];
                }

                this.nodes.footers.push(footer);
            }
        }
    },

    render: function(createElement: CreateElement): VNode {
        this.log("render");
        let rows: VNode[] = [];

        try {
            if (this.ShowHeader == true) {
                rows.push(this.renderHeader(createElement));
            }

            if (this.ShowFilters == true) {
                rows.push(this.renderFilter(createElement));
            }

            if (this.entries.state == "idle") {

            } else if (this.entries.state == "loading") {
                rows.push(createElement("tr", [
                    createElement("td", {
                        attrs: {
                            "colspan": `${this.nodes.columns.length}`
                        }
                    }, [
                        "Loading...",
                        createElement(Busy, {
                            staticStyle: {
                                "height": "1.5rem"
                            }
                        })
                    ])
                ]));

                this.$emit("rerender", Loadable.loading());
            } else if (this.entries.state == "loaded") {
                if (this.entries.data.length == 0) {
                    console.log(`<a-table> 0 entries, showing no data row`);
                    rows.push(this.renderNoDataRow(createElement));
                } else {
                    if (this.ShowTopPages == true && this.paging.size > 10 && this.paginate == true) {
                        rows.push(this.renderPages(createElement));
                    }

                    rows.push(createElement("tbody", {},
                        this.displayedEntries.map((iter, index) => {
                            return this.renderDataRow(createElement, iter, index);
                        }))
                    );
                }

                this.$emit("rerender", Loadable.loaded(this.displayedEntries));
            } else if (this.entries.state == "error") {

                const err: ProblemDetails = this.entries.problem;

                rows.push(createElement("tr",
                    {
                        staticClass: "table-danger"
                    },
                    [
                        createElement("td", {
                            attrs: {
                                "colspan": `${this.nodes.columns.length}`
                            }
                        },
                            [`Error loading data from source: ${err.title} (${err.instance}): ${err.detail}`]
                        )

                    ]
                ));

                this.$emit("rerender", Loadable.error(this.entries.problem));
            } else {
                rows.push(createElement("tr",
                    {
                        staticClass: "table-danger"
                    },
                    [
                        createElement("td", {
                            attrs: {
                                "colspan": `${this.nodes.columns.length}`
                            }
                        },
                            [`Unchecked state of entries: '${this.entries.state}'`]
                        )
                    ]
                ));
            }

            if (this.ShowFooter == true) {
                rows.push(this.renderFooter(createElement));
            }

            if (this.paginate == true) {
                rows.push(this.renderPages(createElement));
            }
        } catch (err) {
            rows.push(createElement("tr",
                {
                    staticClass: "table-danger"
                },
                [
                    createElement("td", {
                        attrs: {
                            "colspan": `${this.nodes.columns.length}`
                        }
                    },
                        [
                            `Error occured while rendering <a-table>`,
                            createElement("br"),
                            `${err}`
                        ]
                    )
                ]
            ));
            console.error(err);
        }

		return createElement("table",
			{
				staticClass: "table a-table",
				class: {
                    "table-sm": (this.RowPadding == "compact"),
                    "table-striped": (this.striped == true),
                    "table-hover": (this.hover == true)
				}
			},
			rows
		);
    },

    methods: {
        log: function(msg: string): void {
            if (this.name == null) {
                return;
            }

            console.log(`<a-table:${this.name}> ${msg}`);
        },

        createIcon: function(createElement: CreateElement, icon: string, style: string = "fas"): VNode {
            return createElement("span", { staticClass: `${style} fa-fw ${icon}` });
        },

        setPage: function(page: number): void {
            if (page > this.pageCount - 1) {
                this.paging.page = this.pageCount - 1;
            } else if (page <= 0) {
                this.paging.page = 0;
            } else {
                this.paging.page = page;
            }
        },

        sortTable: function(field: string): void {
            console.log(`<a-table>: sorting ATable on field '${field}'`);

            // Toggle between the two orders if the currently selected field is the same
            if (this.sorting.field == field) {
                if (this.sorting.order == "asc") {
                    this.sorting.order = "desc";
                } else {
                    this.sorting.order = "asc";
                }
                return;
            }

            // Can we check the type to update the sorting function?
            if (this.entries.state == "loaded" && this.entries.data.length > 0) {
                let first: any = this.entries.data[0];
                if (!first.hasOwnProperty(field)) {
                    throw `Cannot sort on field '${field}', not a property of ${JSON.stringify(first)}`;
                }

                // In the case where some of the data is null, find one row that doesn't have that row null to get the data
                let type: string = "object";
                for (const iter of this.entries.data) {
                    if (iter[field] != null && iter[field] != undefined) {
                        first = iter;
                        type = typeof iter[field];
                    }
                }

                if (type == "object") {
                    if (first[field] instanceof Date) {
                        type = "date";
                    }
                    if (first[field] == null) {
                        throw `Got a null value when sorting on ${type}. Need a non-null value to sort`;
                    }
                }

                if (type == "string") {
                    this.sorting.type = "string";
                } else if (type == "number") {
                    this.sorting.type = "number";
                } else if (type == "boolean") {
                    this.sorting.type = "boolean";
                } else if (type == "date") {
                    this.sorting.type = "date";
                } else {
                    throw `Unchecked sorting type: ${type}. Expected 'string' | 'number' | 'date' | 'boolean'. From value ${first[field]}`;
                }
            } else {
                // We don't know what type the field is, it will be found on next render
                this.sorting.type = "unknown";
            }

            // Else begin sorting by this field
            this.sorting.field = field;
        },

        renderHeader(createElement: CreateElement): VNode {
            let headers: VNode[] = [];

            // Iterate through all the columns and find any <a-header> elements
            for (const header of this.nodes.headers) {
                const options: VNodeData = {};
                options.staticClass = header.colClass;

                if (header.field != undefined) {
                    options.on = {
                        click: this.sortTable.bind(this, header.field)
                    }
                }

                if (header.empty == false) {
					options.staticClass = "table-secondary";

                    headers.push(createElement("td", options, [
                        header.children,
                        (header.field != undefined) ? this.createSortable(createElement, header.field) : []
                    ]));
                } else {
                    // No <a-header> exists, input an empty col so the table stays lined up
					headers.push(createElement("td", options));
                }
            }

            // Return the .list-group-item for the header along with all of the headers set
            return createElement("thead", {}, [
                createElement("tr",
                    {
                        staticClass: `a-table-header-row`
                    },
                    headers
                )
            ]);
        },

        renderPages(createElement: CreateElement): VNode {
			return createElement("tr", [
				createElement("td",
					{
						attrs: {
							"colspan": `${this.nodes.columns.length}`
						}
					},
					this.createPageButtons(createElement)
				)
			]);
        },

        renderDataRow(createElement: CreateElement, data: object, index: number): VNode {
            const cols: VNode[] = [];

            for (const column of this.nodes.columns) {
                if (column.componentOptions?.children) {
                    // Finding the <a-header> elements
                    const headerNodes: VNode[] = column.componentOptions.children
                        .filter((iter: VNode) => iter.componentOptions?.tag == "a-body");

                    if (headerNodes.length > 1) {
                        throw `Cannot have multiple <a-body>s per <a-col>`;
                    }

                    const colClass: string = (column.componentOptions!.propsData as any).ColClass;

                    if (headerNodes.length == 0) {
                        cols.push(createElement("div", { staticClass: colClass }));
                        continue;
                    }

                    const bodyNode: VNode = headerNodes[0];

                    const options: VNodeData = {
                        staticClass: colClass,
                        staticStyle: {
                            //"line-height": lineHeight
                        }
                    }

                    // If the <a-body> has a <a-rank> child, render a <td> with a child
                    // Useful for an ordered list, such as the weapon stat top table
                    if ((bodyNode.componentOptions?.children?.length ?? 0 > 0) && bodyNode.componentOptions?.children![0].componentOptions?.tag == "a-rank") {
                        cols.push(createElement("td", options, `${this.pageOffset + index + 1}`));
                    } else {
                        if (bodyNode.data?.scopedSlots == undefined) {
                            throw `No slots defined for an <a-body>`;
                        }

                        const slot = bodyNode.data?.scopedSlots["default"];
                        if (slot == undefined) {
                            throw `Missing default slot for a <a-body>`;
                        }

                        let lineHeight: string = "1.5";
                        /*
                        switch (this.RowPadding) {
                            case "compact": lineHeight = "1"; break;
                            case "expanded": lineHeight = "2"; break;
                            case "tiny": lineHeight = "0.8"; break;
                            default: lineHeight = "1.5"; break;
                        }
                        */


                        // Copy listeners to the generated node
                        if (bodyNode.componentOptions?.listeners) {
                            options.on = { ...bodyNode.componentOptions.listeners };
                        }

                        options.staticClass = "";
                        cols.push(createElement("td", options, [slot(data)]));
                    }
                }
            }

			return createElement("tr", cols);
        },

        renderNoDataRow(createElement: CreateElement): VNode {
            return createElement("tr", {}, [
                createElement("td", {
                    domProps: {
                        "col-span": this.nodes.columns.length
                    }
                }, ["No data in table"])
            ]);
        },

        renderFilter(createElement: CreateElement): VNode {
            let filters: VNode[] = [];

            for (const filter of this.filters) {
                let inputNode: VNode;

                // Ensure the source is only populated once
                if (filter.method == "dropdown" && filter.source == undefined && this.entries.state == "loaded") {
                    console.log(`<a-filter:dropdown> no src, can load from data`);

                    // Find all unique value of the filter's field
                    // https://stackoverflow.com/questions/11246758
                    const uniqueFields: FilterKeyValue[] = this.entries.data
                        .map((iter: any) => iter[filter.field]) // Collect the field
                        .filter((iter: any, i: any, ar: any) => ar.indexOf(iter) == i) // Find unique
                        .map((iter: any) => ({ key: iter, value: iter })); // Map to the pair

                    // Include an all field
                    uniqueFields.unshift({ key: "All", value: null });

                    filter.source = uniqueFields;
                }

                // Create the appropriate <input> element 
                if (filter.method == "input") {
                    inputNode = this.createInputFilter(createElement, filter);
                } else if (filter.method == "dropdown") {
                    inputNode = this.createDropdownFilter(createElement, filter);
                } else if (filter.method == "reset") {
                    inputNode = this.createResetFilter(createElement, filter);
                } else if (filter.method == "empty") {
                    inputNode = createElement("div");
                } else if (filter.method == "template") {
                    inputNode = this.createTemplateFilter(createElement, filter);
                } else {
                    throw `Unknown filter method '${filter.method}'`;
                }

                // Filter column wrapped in the .input-group
				filters.push(createElement("td",
					{ staticStyle: { "max-width": filter.width ?? "", "flex-grow": 0 } },
                    [inputNode]
                ));
            }

			// <tr> containing all the filter columns
			return createElement("tr", filters);
        },

        renderFooter(createElement: CreateElement): VNode {
            let footers: VNode[] = [];

            for (const footer of this.nodes.footers) {
                footers.push(createElement("td", [
                    footer.children
                ]));
            }

            return createElement("tr", footers);
        },

        createTemplateFilter(createElement: CreateElement, filter: Filter): VNode {
            if (filter.vnode == undefined) {
                throw new Error(`Missing vnode for template <a-filter>`);
            }
            if (filter.vnode.data == undefined) {
                throw new Error(`Missing vnode.data for template <a-filter>`);
            }
            if (filter.vnode.data.scopedSlots == undefined) {
                throw new Error(`Missing vnode.data.scopedSlots for template <a-filter>. Did you put the v-slot in the <a-filter>?`);
            }

            const slot = filter.vnode.data?.scopedSlots["default"];
            if (slot == undefined) {
                throw new Error(`Missing default slot for a template <a-filter>`);
            }

            const options: VNodeData = {
                staticClass: filter.colClass,
                staticStyle: {
                    //"line-height": lineHeight
                }
            }

            // Copy listeners to the generated node
            if (filter.vnode.componentOptions?.listeners) {
                options.on = { ...filter.vnode.componentOptions.listeners };
            }

            options.staticClass = "";
            return createElement("div", options, [slot(null)]);
        },

        createInputFilter(createElement: CreateElement, filter: Filter): VNode {
            let input: VNode;

            if (filter.type == "string") {
                input = createElement("input", {
                    staticClass: "form-control a-table-filter-input",
                    attrs: {
                        "type": "text",
                        "placeholder": (filter.placeholder != undefined) ? filter.placeholder : ""
                    },
                    domProps: {
                        value: filter.value,
                    },
                    on: {
                        input: (ev: Event): void => {
                            filter.value = (ev.target as any).value;
                        }
                    }
                });
            } else if (filter.type == "number") {
                input = createElement("input", {
                    staticClass: "form-control a-table-filter-input",
                    domProps: {
                        value: filter.value,
                    },
                    attrs: {
                        "type": "text",
                        "placeholder": (filter.placeholder != undefined) ? filter.placeholder : ""
                    },
                    on: {
                        keydown: function(ev: KeyboardEvent): void {
                            const num: number = parseFloat(ev.key);
                            if (Number.isNaN(num)) {
                                // Keys like backspace give a key of "Backspace", while letters give single characters
                                if (ev.key.length == 1) {
                                    ev.preventDefault();
                                }
                            }
                        },
                        input: function(ev: InputEvent): void {
                            const value: string = (ev.target as HTMLInputElement).value;
                            if (value == "") {
                                filter.value = null;
                            } else {
                                const num: number = Number.parseFloat(value);
                                if (!Number.isNaN(num)) {
                                    filter.value = num;
                                }
                            }
                        }
                    }
                });
            } else if (filter.type == "date") {
                /*
                input = createElement(DateTimePicker, {
                    staticClass: "a-table-filter-date",
                    props: {
                        AllowNull: true,
                    },
                    scopedSlots: {
                        // <date-time-picker> uses the default slot to allow additional buttons to be added to
                        //      the calendar icon it uses
                        default: (): VNode => {
                            return this.createFilterConditionButton(createElement, filter);
                        }
                    },
                    on: {
                        // Because the DateTimePicker is generating the event, ev is a string, not an Event
                        input: (ev: string | null): void => {
                            filter.value = (ev == null) ? null : new Date(ev);
                        }
                    }
                });
                */
                throw `Type 'date' currently broken`;
            } else if (filter.type == "boolean") {
                input = createElement("input", {
                    staticClass: "form-control a-table-filter-input",
                    domProps: {
                        value: filter.value
                    },
                    attrs: {
                        "type": "checkbox"
                    },
                    on: {
                        input: function(ev: InputEvent): void {
                            const value: boolean = (ev.target as HTMLInputElement).checked;
                            filter.value = value;
                        }
                    }
                });
            } else {
                throw `Unknown type: '${filter.type}'`;
            }

            if (filter.type == "number" || filter.type == "string" || filter.type == "boolean") {
                // .input-group that wraps the conditions a filter can use in a Bootstrap dropdown
                return createElement("div",
                    {
                        staticClass: "input-group",
                    },
                    [
                        input, // Input being wrapped
                        this.createFilterCondition(createElement, filter) // Buttons to swap between conditions
                    ]
                );
            } else if (filter.type == "date") {
                // Condition buttons are added using the default slot of the <date-time-picker>
                return input;
            } else {
                throw `Unchecked type: '${filter.type}'. Cannot create element`;
            }
        },

        createDropdownFilter(createElement: CreateElement, filter: Filter): VNode {
            return createElement("select",
                {
                    staticClass: "form-control a-table-filter-select",
                    on: {
                        input: (event: InputEvent): void => {
                            if (filter.type == "number") {
                                filter.value = Number.parseFloat((event.target as any).value);
                            } else if (filter.type == "string") {
                                filter.value = ((event.target) as any).value;
                            } else {
                                throw `Cannot update the value for an <a-filter>, unchecked type: '${filter.type}'`;
                            }
                        }
                    }
                },
                filter.source?.map((iter: any) => {
                    if (typeof (iter.key) != "string") {
                        throw `Expected to find string for ${iter.key}, got type ${typeof(iter.key)} instead!`;
                    }

                    return createElement("option", { domProps: { value: iter.value } }, iter.key);
                })
            );
        },

        createResetFilter(createElement: CreateElement, filter: Filter): VNode {
            return createElement("button",
                {
                    staticClass: "btn btn-secondary",
                    on: {
                        click: () => {
                            this.resetFilters();
                        }
                    }
                },
                ["Reset"]
            );
        },

        createPageButtons(createElement: CreateElement): VNode[] {
            const nodes: VNode[] = [
                // Page selection buttons
                createElement("div", { staticClass: "btn-group mr-2" }, [
                    // First page button
                    createElement("button",
                        {
                            staticClass: "btn btn-light",
                            domProps: {
                                type: "button"
                            },
                            on: {
                                click: (): void => { this.paging.page = 0; }
                            }
                        },
                        [this.createIcon(createElement, "fa-chevron-circle-left", "fas")]
                    ),

                    // Previous page button
                    createElement("button",
                        {
                            staticClass: "btn btn-light",
                            domProps: {
                                type: "button"
                            },
                            on: {
                                click: (): void => { this.setPage(this.paging.page - 1) }
                            }
                        },
                        [this.createIcon(createElement, "fa-chevron-left", "fas")]
                    ),

                    // Page selection buttons, show 10 max
                    [...Array(Math.min(this.pageCount, 10)).keys()] // Get [0, N]
                        .map(i => ++i) // Transform to [1, N + 1]
                        .filter(i => i + this.pageOffset <= this.pageCount) // Ignore those over page count
                        .map((index: number): VNode => {
                            return createElement("button",
                                {
                                    staticClass: "btn",
                                    class: {
                                        "btn-primary": this.paging.page + 1 == index + this.pageOffset,
                                        "btn-light": this.paging.page + 1 != index + this.pageOffset
                                    },
                                    on: {
                                        click: (): void => { this.setPage(index + this.pageOffset - 1) }
                                    }
                                },
                                [`${this.pageOffset + index}`]
                            )
                        }
                        ),

                    // Next page button
                    createElement("button",
                        {
                            staticClass: "btn btn-light",
                            domProps: {
                                type: "button"
                            },
                            on: {
                                click: (): void => { this.setPage(this.paging.page + 1) }
                            }
                        },
                        [this.createIcon(createElement, "fa-chevron-right", "fas")]
                    ),

                    // Last page button
                    createElement("button",
                        {
                            staticClass: "btn btn-light",
                            domProps: {
                                type: "button"
                            },
                            on: {
                                click: (): void => { this.setPage(this.pageCount - 1); }
                            }
                        },
                        [this.createIcon(createElement, "fa-chevron-circle-right", "fas")]
                    )]
                ),

                // Viewing text
                createElement("span",
                    `Viewing ${Math.min(this.displayedEntries.length, this.paging.size)}/${this.filteredEntries.length}
                        entries in ${this.pageCount} pages`),

                // Page size selector
                createElement("span", { staticClass: "float-right" }, [
                    "Page size:",

                    // Input to select the page size
                    createElement("select",
                        {
                            staticClass: "form-control w-auto d-inline-block",
                            staticStyle: {
                                "vertical-align": "middle"
                            },
                            domProps: {
                                value: this.paging.size
                            },
                            on: {
                                input: (ev: InputEvent): void => {
                                    this.paging.size = Number.parseInt((ev.target as any).value);
                                }
                            },
                        },
                        [
                            this.pageSizes.map((amount: number): VNode => {
                                return createElement("option",
                                    {
                                        domProps: {
                                            value: amount
                                        }
                                    },
                                    [`${amount}`]
                                )
                            })
                        ]
                    )
                ])
            ];

            return nodes;
        },

        createFilterConditionButton(createElement: CreateElement, filter: Filter): VNode {
            // .btn-group that contains the button for the currently selected condition
            //      as well as the dropdown to change the current filter condition
            return createElement("div", { staticClass: "btn-group" }, [
                // Button describing the currently selected condition
                createElement("button",
                    {
                        staticClass: `btn btn-${Conditions.get(filter.selectedCondition)!.color} a-table-filter-dropdown`,
                        staticStyle: {
                            "border-top-left-radius": "0rem",
                            "border-top-right-radius": "0.25rem",
                            "border-bottom-right-radius": "0.25rem",
                            "border-bottom-left-radius": "0rem",
                        },
                        domProps: {
                            "type": "button",
                            "title": Conditions.get(filter.selectedCondition)!.title
                        },
                        // There is no click listener because Bootstrap creates it when we have the data-toggle
                        attrs: {
                            "data-toggle": "dropdown"
                        }
                    },
                    [
                        createElement("span", {
                            staticClass: `fas ${Conditions.get(filter.selectedCondition)!.icon}`
                        })
                    ]
                ),

                // Dropdown menu that contains all the possible filter conditions
                createElement("div",
                    {
                        staticClass: "dropdown-menu dropdown-menu-right border border-dark py-0",
                        staticStyle: {
                            "min-width": "1rem"
                        }
                    },
                    // Dropdown item for each filter condition
                    filter.conditions.map((condition: string): VNode => {
                        const condIcon: string = Conditions.get(condition)!.icon;
                        const condTitle: string = Conditions.get(condition)!.title;
                        const condColor: string = Conditions.get(condition)!.color;

                        return createElement("button",
                            {
                                staticClass: `dropdown-item border-bottom border-dark text-white bg-${condColor}`,
                                on: {
                                    click: () => {
                                        filter.selectedCondition = condition;
                                    }
                                }
                            },
                            [
                                createElement("span", { staticClass: `fas fa-fw ${condIcon}` }),
                                condTitle
                            ]
                        )
                    })
                )
            ]);
        },

        createFilterCondition(createElement: CreateElement, filter: Filter): VNode | undefined {
            // Do not add the dropdown, as it doesn't make sense to toggle between 1 condition
            if (filter.conditions.length == 1) {
                return undefined;
            }

            // Parent element containing the .btn-group
            return createElement("div",
                { staticClass: "input-group-append" },
                [this.createFilterConditionButton(createElement, filter)]
            );
        },

        createSortable(createElement: CreateElement, fieldName: string): VNode {
            if (this.sorting.field == fieldName) {
                return createElement("span", {
                    staticClass: (this.sorting.order == "asc")
                        ? "fas fa-caret-square-up fa-fw mr-auto"
                        : "fas fa-caret-square-down fa-fw mr-auto",
                });
            }

            return createElement("span");
        },

        applyFilter(index: number | string, parameters: Partial<Filter>): void {
            if (typeof (index) != "number") {
                index = this.filters.findIndex(iter => iter.field == index);
            }

            if (index < 0) {
                throw `index was less than 0: ${index}`;
            }

            if (index > this.filters.length) {
                throw `index was greater than number of filters (${this.filters.length}}: ${index}`;
            }

            console.log(parameters);

            for (const key in parameters) {
                (this.filters[index] as any)[key] = (parameters as any)[key];
            }

            /*
             * usually, doing a partial update like this would be done using the following:
                this.filters[index] = {
                    ...this.filters[index],
                    ...parameters
                };

             * this does not work for how a <a-table> works, as setting the filter removes the object used
             * in input (used for the filter), meaning the input no longer controls the filter used to
             * control what rows are set
             * 
             * instead, we go thru each key of the passed parameters and set them one by one
             */
        },

        resetFilters: function(): void {
            for (const filter of this.filters) {
                if (filter.type == "empty") {
                    continue;
                }

                if (filter.type == "number") {
                    filter.value = null;
                } else if (filter.type == "string") {
                    filter.value = "";
                } else if (filter.type == "date") {
                    filter.value = null;
                } else if (filter.type == "boolean") {
                    filter.value = null;
                } else {
                    throw `Cannot reset the value for an <a-filter>, unchecked type: '${filter.type}'`;
                }
            }
        }

    },

    computed: {
        filteredEntries: function(): object[] {
            if (this.entries.state != "loaded" || this.entries.data.length == 0) {
                return [];
            }

            const enabledFilters: Filter[] = this.filters.filter((iter: Filter) => {
                if (iter.selectedCondition == "empty" || iter.selectedCondition == "not_empty") {
                    return true;
                }
                if (iter.type == "string") {
                    return (iter.value as string).trim().length > 0;
                } else if (iter.type == "number") {
                    return (iter.value != null && Number.isNaN(iter.value) == false);
                } else if (iter.type == "date") {
                    return (iter.value != null && iter.value != "");
                } else if (iter.type == "boolean") {
                    return (iter.value != null && iter.value != "");
                } else if (iter.type == "empty") {
                    return false;
                } else if (iter.type == "reset") {
                    return false;
                } else {
                    throw `Unchecked filter type: '${iter.type}'. Cannot check if enabled`;
                }
            });

            if (enabledFilters.length == 0) {
                return this.entries.data;
            }

            console.log(`<a-table> Enabled filters:\n${enabledFilters.map(iter => `${iter.field} ${iter.type}: ${iter.value}`).join("\n")}`);

            const filterFuncs: ((iter: object) => boolean)[] = enabledFilters.map((iter: Filter) => {
                if (iter.type == "string") {
                    if (iter.selectedCondition == "equals") {
                        return ((elem: any) => elem[iter.field] == iter.value);
                    } else if (iter.selectedCondition == "contains") {
                        const valueLower: string = iter.value.toLowerCase();
                        return ((elem: any) => elem[iter.field]?.toLowerCase().indexOf(valueLower) > -1);
                    } else if (iter.selectedCondition == "empty") {
                        return ((elem: any) => !elem[iter.field])
                    } else if (iter.selectedCondition == "not_empty") {
                        return ((elem: any) => !!elem[iter.field]);
                    } else {
                        throw `Invalid condition ${iter.selectedCondition} for type 'string' on field ${iter.field}`;
                    }
                } else if (iter.type == "number") {
                    if (iter.selectedCondition == "equals") {
                        return ((elem: any) => elem[iter.field] == iter.value);
                    } else if (iter.selectedCondition == "not_equal") {
                        return ((elem: any) => elem[iter.field] != iter.value);
                    } else if (iter.selectedCondition == "greater_than") {
                        return ((elem: any) => elem[iter.field] > iter.value);
                    } else if (iter.selectedCondition == "less_than") {
                        return ((elem: any) => elem[iter.field] < iter.value);
                    } else if (iter.selectedCondition == "empty") {
                        return ((elem: any) => elem[iter.field] == null || elem[iter.field] == undefined);
                    } else if (iter.selectedCondition == "not_empty") {
                        return ((elem: any) => elem[iter.field] != null && elem[iter.field] != undefined);
                    } else {
                        throw `Invalid condition ${iter.selectedCondition} for type 'number' on field ${iter.field}`;
                    }
                } else if (iter.type == "date") {
                    const iterTime: number = (iter.value as Date).getTime();
                    if (iter.selectedCondition == "equals") {
                        return ((elem: any) => elem[iter.field]?.getTime() == iterTime);
                    } else if (iter.selectedCondition == "not_equal") {
                        return ((elem: any) => elem[iter.field]?.getTime() != iterTime);
                    } else if (iter.selectedCondition == "greater_than") {
                        return ((elem: any) => elem[iter.field]?.getTime() > iterTime);
                    } else if (iter.selectedCondition == "less_than") {
                        return ((elem: any) => elem[iter.field]?.getTime() < iterTime);
                    } else if (iter.selectedCondition == "empty") {
                        return ((elem: any) => elem[iter.field] == null || elem[iter.field] == undefined);
                    } else if (iter.selectedCondition == "not_empty") {
                        return ((elem: any) => elem[iter.field] != null && elem[iter.field] != undefined);
                    } else {
                        throw `Invalid condition ${iter.selectedCondition} for type 'date' on field ${iter.field}`;
                    }
                } else if (iter.type == "boolean") {
                    if (iter.selectedCondition == "equals") {
                        return ((elem: any) => elem[iter.field] == iter.value);
                    } else {
                        throw `Invalid condition ${iter.selectedCondition} for type 'boolean' on field ${iter.field}`;
                    }
                }
                throw `Uncheck type to create a filter function for: '${iter.type}' on field ${iter.field}`;
            });

            return this.entries.data.filter((iter: object) => {
                for (const func of filterFuncs) {
                    if (func(iter) == false) {
                        return false;
                    }
                }
                return true;
            });
        },

        displayedEntries: function(): object[] {
            if (this.entries.state != "loaded" || this.entries.data.length == 0) {
                return [];
            }

            if (this.paging.page >= this.pageCount) {
                console.log(`<a-table> current page ${this.paging.page} would be cut off as the current page count is ${this.pageCount}, setting to ${this.pageCount - 1}`);
                this.paging.page = Math.max(0, this.pageCount - 1);
            }

            if (this.sorting.field == "") {
                if (this.paginate == true) {
                    return this.filteredEntries
                        .slice(this.paging.page * this.paging.size, (this.paging.page + 1) * this.paging.size);
                } else {
                    return this.filteredEntries;
                }
            }

            if (this.sorting.type == "unknown" && this.sorting.field != "") {
                let first: object = this.entries.data[0];
                if (!first.hasOwnProperty(this.sorting.field)) {
                    throw `Cannot sort on '${this.sorting.field}', is not in ${JSON.stringify(first)}`;
                }

                let val: any = undefined;
                for (let i = 0; i < this.entries.data.length; ++i) {
                    const obj: any = this.entries.data[i];
                    val = obj[this.sorting.field];

                    if (val != null && val != undefined) {
                        console.log(`<a-table> Took ${i} iterations to find a non null value on field ${this.sorting.field}`);
                        break;
                    }
                }

                if (val == null || val == undefined) {
                    console.error(`<a-table> Found all null or undefined values on ${this.sorting.field}`);
                }

                let type: string = typeof val;
                if (type == "object") {
                    if (val instanceof Date) {
                        type = "date";
                    }
                }

                if (type == "string") {
                    this.sorting.type = "string";
                } else if (type == "number") {
                    this.sorting.type = "number";
                } else if (type == "date") {
                    this.sorting.type = "date";
                } else {
                    throw `Unchecked type '${type}' from field ${this.sorting.field}, expected 'string' | 'number' | 'date'`;
                }
            }

            let baseFunc: (a: object, b: object) => number = (a, b) => 1;
            let sortFunc: (a: object, b: object) => number;

            if (this.sorting.field != "") {
                if (this.sorting.type == "string") {
                    baseFunc = (a: any, b: any): number => {
                        const av: string | null = a[this.sorting.field];
                        const bv: string | null = b[this.sorting.field];

                        if (av == null && bv != null) { // 1 = B > A
                            return -1;
                        } else if (av != null && bv == null) {
                            return 1;
                        } else if (av == null && bv == null) {
                            return 0;
                        } else {
                            return av!.localeCompare(bv!);
                        }
                    }
                } else if (this.sorting.type == "number") {
                    baseFunc = (a: any, b: any): number => {
                        const av: number | null = a[this.sorting.field];
                        const bv: number | null = b[this.sorting.field];

                        // Because !0 == true in JS, explicitly check for 0 values,
                        //      which are handled differently from null or undefined values
                        if (!av && av != 0 && (bv || bv == 0)) { // 1 = B > A
                            return -1;
                        } else if ((av || av == 0) && !bv && bv != 0) {
                            return 1;
                        } else if (!av && av != 0 && !bv && bv != 0) {
                            return 0;
                        } else {
                            return av! - bv!;
                        }
                    }
                } else if (this.sorting.type == "date") {
                    baseFunc = (a: any, b: any): number => {
                        const av: Date | null = a[this.sorting.field];
                        const bv: Date | null = b[this.sorting.field];

                        if (av == null && bv != null) { // 1 = B > A
                            return -1;
                        } else if (av != null && bv == null) {
                            return 1;
                        } else if (av == null && bv == null) {
                            return 0;
                        } else {
                            return av!.getTime() - bv!.getTime();
                        }
                    }
                } else if (this.sorting.type == "boolean") {
                    baseFunc = (a: any, b: any): number => {
                        const av: boolean = a[this.sorting.field] ?? false;
                        const bv: boolean = b[this.sorting.field] ?? false;

                        if (av == true && bv == true) {
                            return 0;
                        } else if (av == true && bv == false) {
                            return -1;
                        } else if (av == false && bv == true) {
                            return 1;
                        } else if (av == false && av == false) {
                            return 0;
                        }
                        throw `invalid state of booleans ${av} and ${bv}`;
                    };
                } else {
                    throw `Unchecked sorting type: '${this.sorting.type}'. Expected 'string' | 'number' | 'date'`;
                }
            }

            if (this.sorting.order == "desc") {
                sortFunc = (a: object, b: object): number => {
                    return -baseFunc(a, b); // Swap order for descending sort
                }
            } else {
                sortFunc = baseFunc;
            }

            // ensure if the entries prop is shared between multiple components, that calling .sort() won't trigger
            //      filteredEntries to be re-computed on the other components, which would then trigger a re-compute
            //      on this instance of the componenent, which would then keep going until Vue said stop
            const filtered: object[] = [...this.filteredEntries];

            if (this.paginate == true) {
                return filtered.sort(sortFunc)
                    .slice(this.paging.page * this.paging.size, (this.paging.page + 1) * this.paging.size);
            } else {
                return filtered.sort(sortFunc);
            }
        },

        pageCount: function(): number {
            if (this.entries.state != "loaded") {
                return 0;
            }

            return Math.ceil(this.filteredEntries.length / this.paging.size);
        },

        pageOffset: function(): number {
            return Math.floor(this.paging.page / 10) * 10;
        },

        pageSizes: function(): number[] {
            return this.PageSizes || [5, 10, 25, 50, 100, 200];
        },

        defaultPageSize: function(): number {
            return this.DefaultPageSize || 50;
        }
    },

    components: {
        Busy
    }
});
export default ATable;

const ACol = Vue.extend({
    props: {
        ColClass: { type: String, required: false, default: "col-auto" },
        SortField: { type: String, required: false, default: undefined }
    },
    template: `<div></div>`
});

const AHeader = Vue.extend({
    template: `<div></div>`
});

const AFooter = Vue.extend({
    template: `<div></div>`
});

const ABody = Vue.extend({
    template: `<div></div>`
});

const AFilter = Vue.extend({
    props: {
        method: { type: String, required: true },
        type: { type: String, required: true },
        conditions: { type: Array, required: true },
        field: { type: String, required: true },
        placeholder: { type: String, required: false, default: undefined },
        MaxWidth: { type: String, required: false, default: undefined },
        source: { type: Object, required: true },
        SourceKey: { type: String, required: false },
        SourceValue: { type: String, required: false }
    },
    template: `<div></div>`
});

const ARank = Vue.extend({
    template: `<div></div>`
});

export { ACol, AHeader, ABody, AFilter, AFooter, ARank };
