In .NET, a `StackOverflowException` always terminates the process ; 
[it is impossible to catch it](https://docs.microsoft.com/en-us/archive/blogs/jaredpar/when-can-you-catch-a-stackoverflowexception), 
and [there are no plans to change this](https://github.com/dotnet/runtime/issues/8947).

But some situations call for recursive solutions, especially in languages 
like F#, and the risk of a stack overflow killing the entire process greatly
reduces the viability of these solutions.

This library provides two static methods:

 - `StackLimit.SetLimit(int bytes)` sets a limit on the growth of the stack, equal
   to `bytes` from the current stack depth.  
 - `StackLimit.Check()` throws a `StackOverflowException` if the stack has grown 
   beyond that limit. This exception _can_ be caught. 

You would call `SetLimit` before starting an operation that has a chance to overflow
the stack, such as a recursive function, and you would call `Check()` at various
points in that operation, such as at the beginning of the recursive function.

```fsharp
let rec dangerous = function
| Leaf leaf -> leaf
| Node (left, right) -> 
    StackLimit.Check()
    frobnicate (dangerous left) (dangerous right)
        
let safe tree = 
    StackLimit.SetLimit(100 * 1024) // 100 KB
    dangerous tree
```

If multiple calls to `SetLimit` are made, the last value is kept.

Calling `Check()` without having called `SetLimit` first will _never_ result
in an exception being thrown. 

The limit is set separately for each thread, so using `async` code may lead 
to false negatives (the `Check()` being called on a thread on which `SetLimit()`
was not called).
