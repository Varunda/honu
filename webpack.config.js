const path = require("path");

const CleanWebpackPlugin = require("clean-webpack-plugin");
const TsconfigPathsPlugin = require("tsconfig-paths-webpack-plugin");
const VueLoaderPlugin = require("vue-loader/lib/plugin");

const ModuleDependencyWarning = require("webpack/lib/ModuleDependencyWarning");

const IgnoreNotFoundPlugin = class IgnoreNotFoundPlugin {
    apply(compiler) {
        const regEx = /"export '.*' was not found in/;
        function doneHook(stats) {
            stats.compilation.warnings = stats.compilation.warnings.filter(warn => {
                if (warn instanceof ModuleDependencyWarning && regEx.test(warn.message)) {
                    return false;
                }
                return true;
            });
        }

        if (compiler.hooks) {
            compiler.hooks.done.tap("IgnoreNotFoundPlugin", doneHook);
        } else {
            compiler.plugin("done", doneHook);
        }
    }
}

module.exports = {
    mode: "development",
    devtool: false,
    target: "web",

    watch: module.exports.mode == "development",
    watchOptions: {
        aggregateTimeout: 1000,
        ignored: /node_modules/
    },

    module: {
        rules: [
            {
                test: /\.tsx?$/, 
                loader: "ts-loader",
                exclude: /node_modules/,
                options: {
                    appendTsSuffixTo: [ /\.vue$/ ],
                    transpileOnly: true
                }
            },
            {
                test: /\.vue$/,
                loader: "vue-loader",
                options: {
                    compiler: require("vue-template-compiler")
                }
            },
            {
                test: /\.css$/,
                use: [
                    "vue-style-loader",
                    "css-loader"
                ]
            }
        ]
    },

    plugins: [
        new CleanWebpackPlugin(["wwwroot/dist"]),
        new VueLoaderPlugin(),
        new IgnoreNotFoundPlugin()
    ],

    externals: {
        "jquery": "jQuery",
        "vue": "Vue",
        "moment": "moment",
        "signalR": "signalR",
        "file-saver": "saveAs"
    },

    resolve: {
        extensions: [".ts", ".js", ".vue"],
        plugins: [
            new TsconfigPathsPlugin({configFile: "./tsconfig.json"})
        ]
    },

    entry: "./src/main.ts",
    output: {
        publicPath: "/dist/",
        filename: "[name]/view.js",
        path: path.resolve(__dirname, "wwwroot/dist")
    }
};
