#!/bin/node
/// <reference lib="es2020" />
const fs = require("fs")
const readline = require("readline")
const path = require("path")

const reader = readline.createInterface({
    input: process.stdin,
    output: process.stdout
})


/**
 * @param {string} source 
 * @param {string} target 
 */
function link(source, target) {
    console.log(`Linking ${source} → ${target}`)
    try {
        fs.symlinkSync(path.resolve(source), path.resolve(target), "junction")
    } catch (err) {
        if (err.code == "EEXIST") {
            console.log("File already exists")
        } else {
            throw err
        }
    }
}

/**
 * @type {Record<string, {name : string, callback: () => void}}
 */
const projects = {
    cs_con: {
        name: "C# Console Test",
        callback: () => {
            link("./src/NSLCSharp", "./tests/NSLCSharpConsole/NSLCSharp")
            link("./tests/CSCommon", "./tests/NSLCSharpConsole/CSCommon")
        }
    },
    cs_repl: {
        name: "C# REPL",
        callback: () => {
            link("./src/NSLCSharp", "./tests/NSLCSharpRepl/NSLCSharp")
            link("./tests/CSCommon", "./tests/NSLCSharpRepl/CSCommon")
        }
    }
}

if (process.argv[2] != null) {
    const key = process.argv[2]
    if (key in projects) {
        projects[key].callback()
        reader.close()
    } else {
        console.error("Invalid project name")
    }
} else {
    console.log("Which project do you want to setup?")
    Object.entries(projects).forEach(([key, value]) => {
        console.log(`  (${key}): ${value.name}`)
    })

    function main() {
        reader.question("> ", (key) => {
            if (key in projects) {
                projects[key].callback()
                reader.close()
            } else {
                console.error("Invalid number")
                main()
            }
        })
    }

    main()
}
