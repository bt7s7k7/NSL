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
    cs: {
        name: "C# Generic",
        callback: () => {
            link("./src/NSLCSharp", "./tests/NSLCSharp/NSLCSharp")
            link("./tests/CSCommon", "./tests/NSLCSharp/CSCommon")
        }
    },
    "cs-unit": {
        name: "C# Generic",
        callback: () => {
            link("./src/NSLCSharp", "./tests/NSLCSharpNUnit/NSLCSharp")
            link("./tests/Units", "./tests/NSLCSharpNUnit/Units")
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
        Object.entries(projects).forEach(([key, value]) => {
            console.log(`  (${key}): ${value.name}`)
        })
        process.exit(1)
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
