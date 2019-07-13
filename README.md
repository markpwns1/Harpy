# Harpy
A programming language that compiles to Batch. 

W. I. P. but mostly stable.

Currently has almost no standard library. Compiler errors are vague and don't tell you much. Runtime errors will just fail silently. All of these things are being worked on at the moment--but if your code is right, it works like a charm.

Here are some examples: 
```
// A command-line program to calculate the greatest
// common denominator between two numbers

// Parameters a and b are command-line arguments. They
// must only be strings.
function main(a: string, b: string) {

    if not var_exists(&a) or not var_exists(&b) {
        error("Invalid arguments. Expected two integers.");
    }
    
    if not is_valid_number(a) or not is_valid_number(b) {
        error("Invalid arguments. Expected two integers.");
    }

    var i = 1;
    var gcd: int;
    
    var n1 = to_int(a);
    var n2 = to_int(b);
    
    while i <= n1 and i <= n2 {
        if n1 % i == 0 and n2 % i == 0 {
            gcd = i;
        }
        i++;
    }
    
    print(to_string(gcd));
    
}
```

```
// A double-click program to print the lines
// from a certain file

include {
    IO,
    ARRAYS
};

// Variables declared in the global scope
// must have their type specified
var filename: string = "test.txt";

function main() {
    var lines = file_read_lines(filename);
    array_print(lines);
    pause();
}
```
