# NSL
NSL is a simple embeddable statically typed programming language. It's designed to be easily implemented in any host programming language. Don't expect a general purpose language, it's mostly meant to just call functions in the host environment.

This repository will hosts all official implementations.
  - [x] C#
  - [ ] TypeScript
  - [ ] Zig (?)

## Usage

This library can be used using UCPeM
```
https://github.com/bt7s7k7/NSL
    NSLCSharp
end
```
## Language guide
NSL has a syntax similar to many shell languages like bash. It's designed need the least amount of typing while minimizing implementation difficulty.
### Statements
NSL is made of statements. Each statement is a function call. Statements are separated by newlines or `;` All statements are followed by arguments. Arguments are separated by whitespace.
```bash
print "Hello" "world!"
# Output: Hello world
```
```bash
print "String 1"
print "String 2"; print "String 3"
# Output: 
#   String 1
#   String 2
#   String 3

```
Comments are denoted using `#` and continue until the end of a line. 
To get a list of all available functions and their signatures call the `help` function.
### Literals
String literals start with `"` or `` ` `` and end with their starting character. They can contain the following escape sequences:
```
\n → Newline
\t → Tab
\" → "
\` → `
\\ → \
```
String literals may be turned into template strings by prefixing them with `$`. Template elements are surrounded by `$<`, `>`.
```bash
print $"Hello $[getName]"
```
The statement in the template element will be executed and its result inserted into the string. So if for example `getName` returned Jano, the resulting string will be `Hello Jano`.

Number literals are simply numbers. 

Boolean literals are just literally `true` or `false`.
### Variables
Variables are defined by the `var` keyword.
```bash
var $x 5
# This defines a variable named $x of type Number and assignes value 5 to it
```
To change the value of the variable simply call it with the desired value.
```bash
$x 10
```
To get the value of the variable call it.
```bash
print $x
```
### Pipes
Results of other statements can be piped into other statements.
```bash
getName | print
```
The output of the first statements will be set as the first argument of the second statement. If the first statement returns an array, you can call the second statement for every element of that array.
```
getAllNames |> print
```
If you want to pipe the output of statement to more than one statement you can use blocks.
```bash
findDoor |{
    unlock
    open
}
```
### Actions / Callbacks
You can give a piece of code to a statement to be executed by it. It can give you an argument, accessed with the `$v` variable. The result of the action will be given back to the statement implementation.
```bash
arr 1 2 3 4 5 | filter $v => {$v > 2} |> print
# Prints all numbers greater than five
```
## C# Implementation
First you will have to construct a `FunctionRegistry`. To get a registry with the standard library functions:
```c#
var functions = FunctionRegistry.GetStandardFunctionRegistry();
```
To run some code use the `Runner` class.
```c#
var runner = new Runner(functions);
var (result, diagnostics) = runner.RunScript(script, Path.GetFullPath(file));
                                                  /* ↑ File path for debug output */
```
The `IEnumerable<Diagnostic> diagnostics` is a list of all errors during the compilation of your code. If it's empty that means the compilation was successful and the `NSLValue result` variable contains the resulting value.
```c#
object resultingValue = result.GetValue();
```
If you want deeper control over the compilation process you can call each compilation layer yourself.
```c#
var tokenizationResult = NSLTokenizer.Instance.Tokenize(script, path);
var parsingResult = Parser.Parse(tokenizationResult);
var emittingResult = Emitter.Emit(parsingResult, functions);
var diagnostics = emittingResult.diagnostics;

if (diagnostics.Count() == 0) 
{
    var result = runner.Run(emittingResult.program);
}
```
To get a nice example of using this check out the [REPLRunner.cs](./tests/NSLCSharp/REPLRunner.cs).

There are three ways to implement your own functions. For simple functions your signature can be automatically generated.

```c#
registry.Add(NSLFunction.MakeAuto<Func<string, bool>>("isFoo", (a) => a == "foo"));
/*                                     ↑       ↑       ↑ name  ↑ implementation
                                       |       | result
                                       | arguments
                                       
```
It's important to mention that you are not allowed to use `IEnumerable` other than `object`. To use something different you have to override the type symbols yourself.
```c#
registry.Add(NSLFunction.MakeAuto<Func<IEnumerable<object>>>(
    "getNumbers", 
    () => new object[] { 5, 20, 8, 14 }, 
    new Dictionary<int, TypeSymbol> { { -1, PrimitiveTypes.numberType.ToArray() } }
));
```
For more complex examples see [StandardFunctions.cs](./src/NSLCSharp/StandardFunctions.cs).
